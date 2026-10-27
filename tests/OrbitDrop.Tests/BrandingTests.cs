using OrbitDrop.Infrastructure;
using OrbitDrop.Infrastructure.Updates;
using OrbitDrop.Windows.Shell;
using Xunit;

namespace OrbitDrop.Tests;

/// <summary>
/// Guards the Flickwheel rebrand so a regression can't quietly point the app back at the upstream
/// OrbitDrop identity or release feed. These assert the few constants that carry the product identity.
/// </summary>
public class BrandingTests
{
    [Fact]
    public void Update_asset_name_is_Flickwheel()
        => Assert.Equal("Flickwheel-Setup.exe", UpdateService.SetupAsset);

    [Fact]
    public void Single_instance_identity_is_Flickwheel()
        => Assert.Equal("Flickwheel.Running", SingleInstance.MutexName);

    [Fact]
    public void Data_folders_use_the_Flickwheel_name_not_OrbitDrop()
    {
        var paths = new AppPaths();
        Assert.EndsWith("Flickwheel", paths.RoamingDirectory);
        Assert.EndsWith("Flickwheel", paths.LocalDirectory);
        Assert.DoesNotContain("OrbitDrop", paths.RoamingDirectory);
    }

    [Fact]
    public void Feed_download_urls_use_the_Flickwheel_asset()
    {
        // The release-feed fallback builds asset URLs from the SetupAsset constant.
        const string feed = """
        <feed xmlns="http://www.w3.org/2005/Atom">
          <entry><title>Flickwheel v0.1.0</title><link rel="alternate" type="text/html" href="https://github.com/b3n3d1ck/orbitdrop/releases/tag/v0.1.0"/></entry>
        </feed>
        """;
        var release = UpdateService.PickLatestFromFeed(feed, "https://github.com/b3n3d1ck/orbitdrop/releases/download")!;
        Assert.EndsWith("/Flickwheel-Setup.exe", release.SetupUrl);
        Assert.Contains("b3n3d1ck/orbitdrop", release.SetupUrl);
    }
}
