using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;
using OrbitDrop.Operations.Images;
using OrbitDrop.Operations.Pdf;
using Xunit;

namespace OrbitDrop.Tests;

public class ActionCatalogTests
{
    private static IOperationRegistry FullRegistry()
    {
        var registry = new OperationRegistry();
        registry.Register(new ImageConvertOperation());
        registry.Register(new ImageToPdfOperation());
        return registry;
    }

    [Fact]
    public void ImageSubject_ProducesFormatWedgesAndPdf()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.png"), FullRegistry());

        Assert.Equal(
            new[] { "JPEG", "PNG", "WebP", "BMP", "GIF", "TIFF", "PDF" },
            actions.Select(a => a.Label).ToArray());
    }

    [Fact]
    public void ConvertActions_CarryTheRightFormatParameter()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.png"), FullRegistry());

        Assert.Equal("jpg", FormatOf(actions, "JPEG"));
        Assert.Equal("webp", FormatOf(actions, "WebP"));
        Assert.Equal("tiff", FormatOf(actions, "TIFF"));
    }

    [Fact]
    public void PdfAction_UsesTheImageToPdfOperation()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/photo.png"), FullRegistry());
        var pdf = actions.Single(a => a.Label == "PDF");
        Assert.Equal("image.to-pdf", pdf.Operation.Id);
    }

    [Fact]
    public void JfifIsTreatedAsAnImage()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/scan.jfif"), FullRegistry());
        Assert.Contains(actions, a => a.Label == "PDF");
        Assert.Contains(actions, a => a.Label == "JPEG");
    }

    [Fact]
    public void NonImageSubject_ProducesNoActions()
    {
        var actions = ActionCatalog.ForSubject(new FileSubject("/x/notes.txt"), FullRegistry());
        Assert.Empty(actions);
    }

    private static string? FormatOf(IReadOnlyList<RadialAction> actions, string label)
        => actions.Single(a => a.Label == label).Parameters["format"] as string;
}
