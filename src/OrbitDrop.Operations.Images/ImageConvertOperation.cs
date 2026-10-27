using OrbitDrop.Core.Files;
using OrbitDrop.Core.IO;
using OrbitDrop.Core.Operations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace OrbitDrop.Operations.Images;

/// <summary>
/// Converts an image to another format, optionally resizing to fit a maximum dimension
/// (never upscaling) and flattening transparency onto a chosen background when the target
/// format has no alpha channel. Writes to a temporary file and moves it into place so the
/// input is never at risk.
/// </summary>
public sealed class ImageConvertOperation : IFileOperation
{
    public string Id => "image.convert";
    public string DisplayName => "Convert image";
    public OperationCategory Category => OperationCategory.Image;

    public bool CanApplyTo(FileSubject subject) => subject.Kind == FileKind.Image;

    public async Task<OperationResult> ExecuteAsync(OperationContext context, CancellationToken cancellationToken)
    {
        var input = context.InputPath;
        if (!File.Exists(input))
            return OperationResult.Fail($"Input file does not exist: {input}");

        if (!ImageOutputFormatInfo.TryParse(context.GetParameter<string>("format", "png"), out var format))
            return OperationResult.Fail($"Unsupported target format '{context.GetParameter<string>("format")}'.");

        var warnings = new List<string>();

        int? maxDimension = context.GetParameter<int?>("maxDimension");
        int? quality = context.GetParameter<int?>("quality");
        if (quality is < 1 or > 100)
            return OperationResult.Fail("Quality must be between 1 and 100.");
        if (quality.HasValue && !format.UsesQuality())
            warnings.Add($"Quality is ignored for {format.Extension().ToUpperInvariant()} output.");

        try
        {
            Directory.CreateDirectory(context.OutputDirectory);

            using var image = await Image.LoadAsync(input, cancellationToken).ConfigureAwait(false);

            if (maxDimension is int max)
            {
                if (max < 1)
                    return OperationResult.Fail("Maximum dimension must be at least 1 pixel.");

                var scale = Math.Min((double)max / image.Width, (double)max / image.Height);
                if (scale < 1.0) // only ever shrink
                {
                    var w = Math.Max(1, (int)Math.Round(image.Width * scale));
                    var h = Math.Max(1, (int)Math.Round(image.Height * scale));
                    image.Mutate(x => x.Resize(w, h));
                }
            }

            if (!format.SupportsAlpha())
            {
                var background = ParseBackground(context.GetParameter<string>("background"));
                image.Mutate(x => x.BackgroundColor(background));
                warnings.Add($"Transparency was flattened onto {context.GetParameter("background", "white")} for {format.Extension().ToUpperInvariant()}.");
            }

            var encoder = BuildEncoder(format, quality);

            var desiredName = Path.GetFileNameWithoutExtension(input) + "." + format.Extension();
            var resolved = ResolveOutputPath(context, input, desiredName, out var collisionFailure);
            if (collisionFailure is not null)
                return OperationResult.Fail(collisionFailure);

            cancellationToken.ThrowIfCancellationRequested();

            var temp = resolved + ".orbitdrop.tmp";
            try
            {
                await using (var stream = File.Create(temp))
                {
                    await image.SaveAsync(stream, encoder, cancellationToken).ConfigureAwait(false);
                }

                MoveIntoPlace(temp, resolved, context.CollisionPolicy);
            }
            catch
            {
                TryDelete(temp); // leave no half-written output behind
                throw;
            }

            context.Progress?.Report(1.0);
            return OperationResult.Ok(resolved, $"Converted to {format.Extension().ToUpperInvariant()}.", warnings);
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
            return OperationResult.Fail($"Could not write the output: {ex.Message}", ex);
        }
    }

    private static string ResolveOutputPath(OperationContext context, string inputPath, string desiredName, out string? failure)
    {
        failure = null;
        var candidate = Path.Combine(context.OutputDirectory, desiredName);

        // Protect the source: never let any policy resolve onto the input file itself.
        var collidesWithInput = PathsEqual(candidate, inputPath);

        if (context.CollisionPolicy == CollisionPolicy.Rename || collidesWithInput)
            return OutputNaming.GetNonCollidingPath(context.OutputDirectory, desiredName);

        if (context.CollisionPolicy == CollisionPolicy.Fail && File.Exists(candidate))
        {
            failure = $"Output already exists: {candidate}";
            return candidate;
        }

        return candidate; // Overwrite, or path is free
    }

    private static void MoveIntoPlace(string temp, string destination, CollisionPolicy policy)
    {
        var overwrite = policy == CollisionPolicy.Overwrite && File.Exists(destination);
        File.Move(temp, destination, overwrite);
    }

    private static IImageEncoder BuildEncoder(ImageOutputFormat format, int? quality) => format switch
    {
        ImageOutputFormat.Jpeg => new JpegEncoder { Quality = quality ?? 85 },
        ImageOutputFormat.Webp => new WebpEncoder { Quality = quality ?? 85 },
        ImageOutputFormat.Png => new PngEncoder(),
        ImageOutputFormat.Bmp => new BmpEncoder(),
        ImageOutputFormat.Gif => new GifEncoder(),
        ImageOutputFormat.Tiff => new TiffEncoder(),
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };

    private static Color ParseBackground(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Color.White;

        token = token.Trim();
        if (token.StartsWith('#') && Color.TryParseHex(token, out var hex))
            return hex;

        return token.ToLowerInvariant() switch
        {
            "white" => Color.White,
            "black" => Color.Black,
            _ => Color.White,
        };
    }

    private static bool PathsEqual(string a, string b)
        => string.Equals(Path.GetFullPath(a), Path.GetFullPath(b),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best effort cleanup; never mask the original error */ }
    }
}
