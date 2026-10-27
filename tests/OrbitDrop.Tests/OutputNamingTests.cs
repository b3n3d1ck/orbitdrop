using System.IO;
using OrbitDrop.Core.IO;
using Xunit;

namespace OrbitDrop.Tests;

public class OutputNamingTests
{
    [Fact]
    public void FirstName_IsUsed_WhenNothingCollides()
    {
        var result = OutputNaming.GetNonCollidingPath("/out", "photo.jpg", _ => false);
        Assert.Equal(Path.Combine("/out", "photo.jpg"), result);
    }

    [Fact]
    public void SecondName_FollowsExplorerScheme()
    {
        var taken = new HashSet<string> { Path.Combine("/out", "photo.jpg") };
        var result = OutputNaming.GetNonCollidingPath("/out", "photo.jpg", taken.Contains);
        Assert.Equal(Path.Combine("/out", "photo (2).jpg"), result);
    }

    [Fact]
    public void Counter_SkipsPastMultipleCollisions()
    {
        var taken = new HashSet<string>
        {
            Path.Combine("/out", "photo.jpg"),
            Path.Combine("/out", "photo (2).jpg"),
            Path.Combine("/out", "photo (3).jpg"),
        };
        var result = OutputNaming.GetNonCollidingPath("/out", "photo.jpg", taken.Contains);
        Assert.Equal(Path.Combine("/out", "photo (4).jpg"), result);
    }

    [Fact]
    public void ExtensionlessNames_AreHandled()
    {
        var taken = new HashSet<string> { Path.Combine("/out", "README") };
        var result = OutputNaming.GetNonCollidingPath("/out", "README", taken.Contains);
        Assert.Equal(Path.Combine("/out", "README (2)"), result);
    }

    [Fact]
    public void RealFilesystem_ProducesAnUnusedPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "orbitdrop-naming-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.txt"), "x");
            var result = OutputNaming.GetNonCollidingPath(dir, "a.txt");
            Assert.Equal(Path.Combine(dir, "a (2).txt"), result);
            Assert.False(File.Exists(result));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
