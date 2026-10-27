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
    public void OuterRadius_FitsInsideBox_AndIsNonNegative()
    {
        Assert.Equal(200 - 14, RadialLayout.OuterRadius(600, 400), 3); // min(600,400)/2 - margin
        Assert.Equal(0, RadialLayout.OuterRadius(10, 10)); // margin larger than half -> clamped to 0
    }

    [Fact]
    public void InnerRadius_IsSmallerThanOuter()
    {
        var outer = RadialLayout.OuterRadius(800, 600);
        var inner = RadialLayout.InnerRadius(outer);
        Assert.True(inner > 0 && inner < outer, $"inner={inner}, outer={outer}");
    }

    [Fact]
    public void SegmentAngles_StartAtTop_AndTileTheCircle()
    {
        const int count = 7;
        var span = 2 * Math.PI / count;

        var (firstStart, firstEnd) = RadialLayout.SegmentAngles(0, count);
        Assert.Equal(RadialLayout.TopRad, firstStart, 9);
        Assert.Equal(RadialLayout.TopRad + span, firstEnd, 9);

        // Each segment begins exactly where the previous ended; total coverage is the full circle.
        double total = 0;
        double prevEnd = firstStart;
        for (var i = 0; i < count; i++)
        {
            var (s, e) = RadialLayout.SegmentAngles(i, count);
            Assert.Equal(prevEnd, s, 9);
            total += e - s;
            prevEnd = e;
        }
        Assert.Equal(2 * Math.PI, total, 9);
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    [InlineData(0, 0)]
    public void SegmentAngles_RejectsBadIndexOrCount(int index, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RadialLayout.SegmentAngles(index, count));
    }

    [Fact]
    public void PointOnCircle_IsAtTheGivenRadius()
    {
        var center = new LayoutPoint(100, 100);
        var p = RadialLayout.PointOnCircle(center, 50, 0); // 0 rad = +x
        Assert.Equal(150, p.X, 6);
        Assert.Equal(100, p.Y, 6);
    }

    [Fact]
    public void LabelPoint_SitsBetweenTheTwoRings()
    {
        const double w = 500, h = 500, inner = 80, outer = 200;
        var center = RadialLayout.Center(w, h);
        for (var i = 0; i < 6; i++)
        {
            var p = RadialLayout.LabelPoint(w, h, i, 6, inner, outer);
            var dist = Math.Sqrt(Math.Pow(p.X - center.X, 2) + Math.Pow(p.Y - center.Y, 2));
            Assert.Equal((inner + outer) / 2, dist, 6);
        }
    }

    [Fact]
    public void HitTest_CentreHole_IsCancel()
    {
        var hit = RadialLayout.HitTest(400, 400, innerRadius: 80, outerRadius: 180, count: 6,
            point: new LayoutPoint(200, 200));
        Assert.Equal(WheelRegion.Hole, hit.Region);
    }

    [Fact]
    public void HitTest_OutsideOuterRadius_IsOutside()
    {
        var hit = RadialLayout.HitTest(400, 400, 80, 180, 6, new LayoutPoint(395, 200));
        Assert.Equal(WheelRegion.Outside, hit.Region);
    }

    [Fact]
    public void HitTest_TopOfRing_IsFirstSegment()
    {
        // Straight up from centre, between the rings, lands in segment 0 (which starts at the top).
        var hit = RadialLayout.HitTest(400, 400, 80, 180, 6, new LayoutPoint(200, 200 - 130));
        Assert.Equal(WheelRegion.Segment, hit.Region);
        Assert.Equal(0, hit.Index);
    }

    [Fact]
    public void HitTest_EverySegmentIsReachable_AndIndicesAreUnique()
    {
        const double w = 400, h = 400, inner = 80, outer = 180;
        const int count = 7;
        var mid = (inner + outer) / 2;
        var seen = new HashSet<int>();

        for (var i = 0; i < count; i++)
        {
            var (s, e) = RadialLayout.SegmentAngles(i, count);
            var angle = (s + e) / 2;
            var p = RadialLayout.PointOnCircle(RadialLayout.Center(w, h), mid, angle);
            var hit = RadialLayout.HitTest(w, h, inner, outer, count, p);
            Assert.Equal(WheelRegion.Segment, hit.Region);
            Assert.Equal(i, hit.Index);
            Assert.True(seen.Add(hit.Index), $"segment {i} collided with another");
        }

        Assert.Equal(count, seen.Count);
    }
}
