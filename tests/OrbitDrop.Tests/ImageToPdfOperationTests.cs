using System.IO;
using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;
using OrbitDrop.Operations.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace OrbitDrop.Tests;

public sealed class ImageToPdfOperationTests : IDisposable
{
    private readonly string _dir;
    private readonly ImageToPdfOperation _op = new();

    public ImageToPdfOperationTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "orbitdrop-pdf-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    private string MakePng(string name, int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(x => x.BackgroundColor(Color.Red));
        var path = Path.Combine(_dir, name);
        image.Save(path);
        return path;
    }

    private OperationContext Context(string input, CollisionPolicy policy = CollisionPolicy.Rename)
        => new()
        {
            Subject = new FileSubject(input),
            OutputDirectory = _dir,
            Parameters = new Dictionary<string, object?>(),
            CollisionPolicy = policy,
        };

    [Fact]
    public async Task ProducesAValidPdf_FromAnImage()
    {
        var input = MakePng("photo.png", 400, 300);
        var result = await _op.ExecuteAsync(Context(input), CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.EndsWith(".pdf", result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
        Assert.True(File.Exists(input), "input must be preserved");

        var pdf = await File.ReadAllBytesAsync(result.OutputPath!);
        PdfTestHelpers.AssertValidSingleImagePdf(pdf);
    }

    [Fact]
    public async Task Portrait_And_Landscape_BothSucceed()
    {
        foreach (var (name, w, h) in new[] { ("tall.png", 300, 600), ("wide.png", 600, 300) })
        {
            var input = MakePng(name, w, h);
            var result = await _op.ExecuteAsync(Context(input), CancellationToken.None);
            Assert.True(result.Success, result.Message);
            PdfTestHelpers.AssertValidSingleImagePdf(await File.ReadAllBytesAsync(result.OutputPath!));
        }
    }

    [Fact]
    public async Task Collision_ProducesRenamedCopy()
    {
        var input = MakePng("pic.png", 100, 100);
        var first = await _op.ExecuteAsync(Context(input), CancellationToken.None);
        var second = await _op.ExecuteAsync(Context(input), CancellationToken.None);

        Assert.Equal(Path.Combine(_dir, "pic.pdf"), first.OutputPath);
        Assert.Equal(Path.Combine(_dir, "pic (2).pdf"), second.OutputPath);
        Assert.True(File.Exists(first.OutputPath));
    }

    [Fact]
    public async Task ReportsRasterisationWarning()
    {
        var input = MakePng("note.png", 50, 50);
        var result = await _op.ExecuteAsync(Context(input), CancellationToken.None);
        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Contains("JPEG", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CorruptInput_FailsGracefully()
    {
        var input = Path.Combine(_dir, "broken.png");
        await File.WriteAllTextAsync(input, "not an image");
        var result = await _op.ExecuteAsync(Context(input), CancellationToken.None);
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }
}
