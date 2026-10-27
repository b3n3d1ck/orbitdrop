using System.IO;
using OrbitDrop.Core.IO;
using Xunit;

namespace OrbitDrop.Tests;

public class PathSafetyTests
{
    private static readonly string Base = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "orbitdrop-extract"));

    [Fact]
    public void PlainEntry_IsInside()
    {
        Assert.True(PathSafety.IsWithinDirectory(Base, Path.Combine(Base, "readme.txt")));
    }

    [Fact]
    public void NestedEntry_IsInside()
    {
        Assert.True(PathSafety.IsWithinDirectory(Base, Path.Combine(Base, "sub", "deep", "file.bin")));
    }

    [Fact]
    public void TraversalEntry_IsRejected()
    {
        var escaping = Path.Combine(Base, "..", "..", "etc", "passwd");
        Assert.False(PathSafety.IsWithinDirectory(Base, escaping));
    }

    [Fact]
    public void ResolveSafeEntryPath_ThrowsOnTraversal()
    {
        Assert.Throws<UnsafePathException>(
            () => PathSafety.ResolveSafeEntryPath(Base, "../../evil.sh"));
    }

    [Fact]
    public void ResolveSafeEntryPath_NormalisesBackslashes()
    {
        var resolved = PathSafety.ResolveSafeEntryPath(Base, "sub\\inner\\ok.txt");
        Assert.StartsWith(Path.GetFullPath(Base), resolved);
        Assert.Contains("ok.txt", resolved);
    }
}
