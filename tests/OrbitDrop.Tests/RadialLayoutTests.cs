using OrbitDrop.Core.Ui;
using Xunit;

namespace OrbitDrop.Tests;

public class RadialLayoutTests
{
    [Fact]
    public void Center_IsMiddleOfBox()
    {
        var c = RadialLayout.Center(400, 300);
        Assert.Equal(200, c.X, 3);
        Assert.Equal(150, c.Y, 3);
    }

    [Fact]
    public void SegmentCenter_FirstSegment_IsAtTop()
    {
        // index 0 of 4 starts at 12 o'clock: same X as centre, radius above it.
        var p = RadialLayout.SegmentCenter(400, 400, index: 0, count: 4, radius: 100);
        Assert.Equal(200, p.X, 3);
        Assert.Equal(100, p.Y, 3);
    }

    [Fact]
    public void SegmentCenter_ThirdOfFour_IsAtBottom()
    {
        var p = RadialLayout.SegmentCenter(400, 400, index: 2, count: 4, radius: 100);
        Assert.Equal(200, p.X, 3);
        Assert.Equal(300, p.Y, 3);
    }

    [Fact]
    public void SegmentCenter_AllSegmentsLieOnTheCircle()
    {
        const double r = 120;
        var center = RadialLayout.Center(500, 500);
        for (var i = 0; i < 6; i++)
        {
            var p = RadialLayout.SegmentCenter(500, 500, i, 6, r);
            var dist = Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2));
            Assert.Equal(r, dist, 3);
        }
    }

    [Theory]
    [InlineData(-1, 4)]  // negative index
    [InlineData(4, 4)]   // index == count (out of range)
    [InlineData(5, 4)]   // index > count
    [InlineData(0, 0)]   // non-positive count
    public void SegmentCenter_RejectsBadIndexOrCount(int index, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RadialLayout.SegmentCenter(400, 400, index, count, 100));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-5, 100)]
    [InlineData(100, -5)]
    public void FitRadius_DegenerateBox_IsZero(double w, double h)
    {
        Assert.Equal(0, RadialLayout.FitRadius(w, h, 80, 23));
    }

    [Fact]
    public void FitRadius_NeverExceedsTheBoundingCaps()
    {
        const double halfW = 80, halfH = 23, margin = 8;
        foreach (var (w, h) in new[] { (400d, 300d), (720d, 480d), (980d, 640d), (1920d, 1080d) })
        {
            var r = RadialLayout.FitRadius(w, h, halfW, halfH, margin: margin);
            Assert.True(r <= w / 2 - halfW - margin + 1e-9, $"radius {r} too wide for {w}x{h}");
            Assert.True(r <= h / 2 - halfH - margin + 1e-9, $"radius {r} too tall for {w}x{h}");
            Assert.True(r >= 0);
        }
    }

    [Fact]
    public void FitRadius_KeepsEverySegmentFullyInsideTheWindow()
    {
        const double halfW = 80, halfH = 23;
        foreach (var (w, h) in new[] { (400d, 300d), (720d, 480d), (980d, 640d), (1920d, 1080d) })
        {
            var r = RadialLayout.FitRadius(w, h, halfW, halfH);
            for (var count = 1; count <= 8; count++)
            {
                for (var i = 0; i < count; i++)
                {
                    var c = RadialLayout.SegmentCenter(w, h, i, count, r);
                    Assert.True(c.X - halfW >= -1e-9, "segment left edge off-screen");
                    Assert.True(c.X + halfW <= w + 1e-9, "segment right edge off-screen");
                    Assert.True(c.Y - halfH >= -1e-9, "segment top edge off-screen");
                    Assert.True(c.Y + halfH <= h + 1e-9, "segment bottom edge off-screen");
                }
            }
        }
    }

    [Fact]
    public void FitRadius_GrowsWithWindowSize()
    {
        var small = RadialLayout.FitRadius(600, 400, 80, 23);
        var large = RadialLayout.FitRadius(1200, 800, 80, 23);
        Assert.True(large > small, $"expected radius to grow: small={small}, large={large}");
    }
}
