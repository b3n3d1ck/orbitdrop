using OrbitDrop.Core.Files;
using OrbitDrop.Core.IO;
using OrbitDrop.Core.Operations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OrbitDrop.Operations.Pdf;

/// <summary>
/// Places a single image onto one PDF page, fitted within an A4 page (portrait or landscape to
/// match the image) with a margin, preserving aspect ratio. The image is embedded as JPEG, so
/// transparency is flattened onto white; this is a lossy rasterisation, which the result reports.
/// Writes to a temporary file and moves it into place so the input is never at risk.
/// </summary>
public sealed class ImageToPdfOperation : IFileOperation
{
    // A4 in PostScript points (1/72 inch).
    private const double A4Short = 595.276;
    private const double A4Long = 841.890;
    private const double Margin = 18.0; // 0.25 inch

    public string Id => "image.to-pdf";
    public string DisplayName => "Convert to PDF";
    public OperationCategory Category => OperationCategory.Pdf;

    public bool CanApplyTo(FileSubject subject) => subject.Kind == FileKind.Image;

    public async Task<OperationResult> ExecuteAsync(OperationContext context, CancellationToken cancellationToken)
    {
        var input = context.InputPath;
        if (!File.Exists(input))
            return OperationResult.Fail($"Input file does not exist: {input}");

        var warnings = new List<string>
        {
            "The image is embedded as JPEG (quality 90) and transparency is flattened onto white.",
        };

        try
        {
            Directory.CreateDirectory(context.OutputDirectory);

            byte[] jpeg;
            int widthPx, heightPx;
            using (var image = await Image.LoadAsync(input, cancellationToken).ConfigureAwait(false))
            {
                image.Mutate(x => x.BackgroundColor(Color.White)); // flatten any alpha
                widthPx = image.Width;
                heightPx = image.Height;

                using var jpegStream = new MemoryStream();
                await image.SaveAsync(jpegStream, new JpegEncoder { Quality = 90 }, cancellationToken).ConfigureAwait(false);
                jpeg = jpegStream.ToArray();
            }

            var (pageWidth, pageHeight) = widthPx >= heightPx ? (A4Long, A4Short) : (A4Short, A4Long);
            var availableWidth = pageWidth - 2 * Margin;
            var availableHeight = pageHeight - 2 * Margin;
            var scale = Math.Min(availableWidth / widthPx, availableHeight / heightPx);
            var drawWidth = widthPx * scale;
            var drawHeight = heightPx * scale;
            var drawX = (pageWidth - drawWidth) / 2.0;
            var drawY = (pageHeight - drawHeight) / 2.0;

            var pdfBytes = MinimalPdf.SingleImagePage(
                jpeg, widthPx, heightPx, pageWidth, pageHeight, drawX, drawY, drawWidth, drawHeight);

            var desiredName = Path.GetFileNameWithoutExtension(input) + ".pdf";
            var resolved = ResolveOutputPath(context, desiredName, out var failure);
            if (failure is not null)
                return OperationResult.Fail(failure);

            cancellationToken.ThrowIfCancellationRequested();

            var temp = resolved + ".orbitdrop.tmp";
            try
            {
                await File.WriteAllBytesAsync(temp, pdfBytes, cancellationToken).ConfigureAwait(false);
                var overwrite = context.CollisionPolicy == CollisionPolicy.Overwrite && File.Exists(resolved);
                File.Move(temp, resolved, overwrite);
            }
            catch
            {
                TryDelete(temp);
                throw;
            }

            context.Progress?.Report(1.0);
            return OperationResult.Ok(resolved, "Created a one-page PDF.", warnings);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UnknownImageFormatException ex)
        {
            return OperationResult.Fail("The file is not a recognised image or is corrupt.", ex);
        }
        catch (InvalidImageContentException ex)
        {
            return OperationResult.Fail("The image appears to be damaged and could not be decoded.", ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return OperationResult.Fail($"Could not write the PDF: {ex.Message}", ex);
        }
    }

    private static string ResolveOutputPath(OperationContext context, string desiredName, out string? failure)
    {
        failure = null;
        var candidate = Path.Combine(context.OutputDirectory, desiredName);

        if (context.CollisionPolicy == CollisionPolicy.Rename)
            return OutputNaming.GetNonCollidingPath(context.OutputDirectory, desiredName);

        if (context.CollisionPolicy == CollisionPolicy.Fail && File.Exists(candidate))
        {
            failure = $"Output already exists: {candidate}";
            return candidate;
        }

        return candidate;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best effort */ }
    }
}
