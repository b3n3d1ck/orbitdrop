using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;
using OrbitDrop.Operations.Images;
using Xunit;

namespace OrbitDrop.Tests;

public class ActionCatalogTests
{
    private static IOperationRegistry RegistryWithImages()
    {
        var registry = new OperationRegistry();
        registry.Register(new ImageConvertOperation());
        return registry;
    }

    [Fact]
    public void ImageSubject_ProducesTheExpectedActions()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.png"), RegistryWithImages());

        Assert.Equal(
            new[] { "Convert to JPEG", "Convert to PNG", "Convert to WebP", "Resize to 1080px" },
            actions.Select(a => a.Label).ToArray());
    }

    [Fact]
    public void ConvertActions_CarryTheRightFormatParameter()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.png"), RegistryWithImages());

        Assert.Equal("jpg", FormatOf(actions, "Convert to JPEG"));
        Assert.Equal("png", FormatOf(actions, "Convert to PNG"));
        Assert.Equal("webp", FormatOf(actions, "Convert to WebP"));
    }

    [Fact]
    public void Resize_KeepsJpegSourceAsJpeg_AndCapsDimension()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.jpg"), RegistryWithImages());
        var resize = actions.Single(a => a.Label == "Resize to 1080px");

        Assert.Equal("jpg", resize.Parameters["format"]);
        Assert.Equal(1080, resize.Parameters["maxDimension"]);
    }

    [Fact]
    public void Resize_DefaultsNonJpegSourceToPng()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/image.webp"), RegistryWithImages());
        var resize = actions.Single(a => a.Label == "Resize to 1080px");

        Assert.Equal("png", resize.Parameters["format"]);
    }

    [Fact]
    public void NonImageSubject_ProducesNoActions()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/notes.txt"), RegistryWithImages());
        Assert.Empty(actions);
    }

    private static string? FormatOf(IReadOnlyList<RadialAction> actions, string label)
        => actions.Single(a => a.Label == label).Parameters["format"] as string;
}
