using OrbitDrop.Core.Files;
using Xunit;

namespace OrbitDrop.Tests;

public class FileClassifierTests
{
    [Theory]
    [InlineData("a.png", FileKind.Image)]
    [InlineData("a.JPG", FileKind.Image)]
    [InlineData("a.webp", FileKind.Image)]
    [InlineData("doc.pdf", FileKind.Pdf)]
    [InlineData("clip.mp4", FileKind.Video)]
    [InlineData("song.flac", FileKind.Audio)]
    [InlineData("notes.md", FileKind.Document)]
    [InlineData("bundle.zip", FileKind.Archive)]
    [InlineData("mystery.xyz", FileKind.Unknown)]
    [InlineData("noext", FileKind.Unknown)]
    public void Classify_MapsByExtension(string name, FileKind expected)
    {
        Assert.Equal(expected, FileClassifier.Classify(name));
    }

    [Fact]
    public void Subject_ExposesLowercaseExtension()
    {
        var subject = new FileSubject("/tmp/Photo.PNG");
        Assert.Equal("png", subject.Extension);
        Assert.Equal(FileKind.Image, subject.Kind);
        Assert.Equal("Photo.PNG", subject.FileName);
    }
}
