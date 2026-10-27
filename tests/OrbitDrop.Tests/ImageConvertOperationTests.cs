using System.IO;
using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;
using OrbitDrop.Operations.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace OrbitDrop.Tests;

public sealed class ImageConvertOperationTests : IDisposable
{
    private readonly string _dir;
    private readonly ImageConvertOperation _op = new();

    public ImageConvertOperationTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "orbitdrop-img-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    private string MakePng(string name, int width, int height, bool opaque)
    {
        using var image = new Image<Rgba32>(width, height); // transparent by default
        if (opaque)
            image.Mutate(x => x.BackgroundColor(Color.Red));
        var path = Path.Combine(_dir, name);
        image.Save(path); // PNG inferred from extension
        return path;
    }

    private OperationContext Context(string input, Dictionary<string, object?> parameters,
        CollisionPolicy policy = CollisionPolicy.Rename)
        => new()
        {
            Subject = new FileSubject(input),
            OutputDirectory = _dir,
            Parameters = parameters,
            CollisionPolicy = policy,
        };

    [Fact]
    public async Task Converts_Png_To_Jpeg_PreservingDimensions()
    {
        var input = MakePng("photo.png", 120, 80, opaque: true);
        var result = await _op.ExecuteAsync(Context(input, new() { ["format"] = "jpg" }), CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.OutputPath);
        Assert.EndsWith(".jpg", result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
        Assert.True(File.Exists(input), "input must be preserved");

        var info = Image.Identify(result.OutputPath!);
        Assert.Equal(120, info.Width);
        Assert.Equal(80, info.Height);
    }

    [Fact]
    public async Task MaxDimension_Shrinks_PreservingAspect()
    {
        var input = MakePng("big.png", 100, 50, opaque: true);
        var result = await _op.ExecuteAsync(
            Context(input, new() { ["format"] = "png", ["maxDimension"] = 50 }), CancellationToken.None);

        Assert.True(result.Success, result.Message);
        var info = Image.Identify(result.OutputPath!);
        Assert.Equal(50, info.Width);
        Assert.Equal(25, info.Height);
    }

    [Fact]
    public async Task MaxDimension_NeverUpscales()
    {
        var input = MakePng("small.png", 10, 10, opaque: true);
        var result = await _op.ExecuteAsync(
            Context(input, new() { ["format"] = "png", ["maxDimension"] = 100 }), CancellationToken.None);

        Assert.True(result.Success, result.Message);
        var info = Image.Identify(result.OutputPath!);
        Assert.Equal(10, info.Width);
        Assert.Equal(10, info.Height);
    }

    [Fact]
    public async Task Collision_ProducesRenamedCopy_AndKeepsExisting()
    {
        var input = MakePng("pic.png", 20, 20, opaque: true);
        // Convert to JPEG once...
        var first = await _op.ExecuteAsync(Context(input, new() { ["format"] = "jpg" }), CancellationToken.None);
        Assert.True(first.Success);
        Assert.Equal(Path.Combine(_dir, "pic.jpg"), first.OutputPath);

        // ...and again: the second output must not clobber the first.
        var second = await _op.ExecuteAsync(Context(input, new() { ["format"] = "jpg" }), CancellationToken.None);
        Assert.True(second.Success);
        Assert.Equal(Path.Combine(_dir, "pic (2).jpg"), second.OutputPath);
        Assert.True(File.Exists(first.OutputPath));
    }

    [Fact]
    public async Task SameFormatSameFolder_DoesNotOverwriteInput()
    {
        var input = MakePng("keep.png", 16, 16, opaque: true);
        var originalBytes = await File.ReadAllBytesAsync(input);

        var result = await _op.ExecuteAsync(Context(input, new() { ["format"] = "png" }), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEqual(input, result.OutputPath);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(input));
    }

    [Fact]
    public async Task UnsupportedFormat_FailsCleanly()
    {
        var input = MakePng("x.png", 8, 8, opaque: true);
        var result = await _op.ExecuteAsync(Context(input, new() { ["format"] = "heic" }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Unsupported", result.Message);
    }

    [Fact]
    public async Task CorruptInput_FailsGracefully()
    {
        var input = Path.Combine(_dir, "broken.png");
        await File.WriteAllTextAsync(input, "this is not a png");

        var result = await _op.ExecuteAsync(Context(input, new() { ["format"] = "jpg" }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task FlatteningTransparency_ToJpeg_EmitsWarning()
    {
        var input = MakePng("alpha.png", 24, 24, opaque: false);
        var result = await _op.ExecuteAsync(
            Context(input, new() { ["format"] = "jpg", ["background"] = "white" }), CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Contains(result.Warnings, w => w.Contains("flattened", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InvalidQuality_IsRejected()
    {
        var input = MakePng("q.png", 8, 8, opaque: true);
        var result = await _op.ExecuteAsync(
            Context(input, new() { ["format"] = "jpg", ["quality"] = 150 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Quality", result.Message);
    }
}
