namespace OrbitDrop.Core.Ui;

/// <summary>A point in device-independent layout coordinates. WPF-free so it is testable anywhere.</summary>
public readonly record struct LayoutPoint(double X, double Y);

/// <summary>Which part of the wheel a point falls in.</summary>
public enum WheelRegion
{
    Outside,
    Hole,
    Segment,
}

/// <summary>Result of hit-testing the wheel: the region and, for <see cref="WheelRegion.Segment"/>, its index.</summary>
public readonly record struct WheelHit(WheelRegion Region, int Index);

/// <summary>
/// Pure geometry for the donut-style radial menu: segment angles, points on the circle, label
/// anchors, and pointer hit-testing. No WPF types and no side effects, so the placement and
/// hit-test rules are unit-tested directly and the control cannot silently disagree with them.
///
/// Convention: screen coordinates (x right, y down). Segment 0 starts at 12 o'clock
/// (angle -PI/2) and segments run clockwise.
/// </summary>
public static class RadialLayout
{
    public const double TopRad = -Math.PI / 2.0;

    public static LayoutPoint Center(double width, double height) => new(width / 2.0, height / 2.0);

    /// <summary>Outer radius of the ring: fills the box minus a margin, never negative.</summary>
    public static double OuterRadius(double width, double height, double margin = 14.0)
        => Math.Max(0, Math.Min(width, height) / 2.0 - margin);

    /// <summary>Inner radius (the centre hole), a fraction of the outer radius.</summary>
    public static double InnerRadius(double outerRadius, double holeRatio = 0.42)
        => Math.Clamp(holeRatio, 0.0, 0.95) * Math.Max(0, outerRadius);

    /// <summary>Start and end angle (radians) of segment <paramref name="index"/> of <paramref name="count"/>.</summary>
    public static (double StartRad, double EndRad) SegmentAngles(int index, int count, double startRad = TopRad)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var span = 2.0 * Math.PI / count;
        var start = startRad + index * span;
        return (start, start + span);
    }

    public static LayoutPoint PointOnCircle(LayoutPoint center, double radius, double angleRad)
        => new(center.X + radius * Math.Cos(angleRad), center.Y + radius * Math.Sin(angleRad));

    /// <summary>Anchor point for a segment's label: mid-angle, mid-radius between the two rings.</summary>
    public static LayoutPoint LabelPoint(double width, double height, int index, int count, double innerRadius, double outerRadius)
    {
        var (start, end) = SegmentAngles(index, count);
        var midAngle = (start + end) / 2.0;
        var midRadius = (innerRadius + outerRadius) / 2.0;
        return PointOnCircle(Center(width, height), midRadius, midAngle);
    }

    /// <summary>Classifies a point relative to the wheel centred in a box of the given ring radii.</summary>
    public static WheelHit HitTest(double width, double height, double innerRadius, double outerRadius, int count, LayoutPoint point, double startRad = TopRad)
    {
        var center = Center(width, height);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance <= innerRadius)
            return new WheelHit(WheelRegion.Hole, -1);
        if (distance > outerRadius || count <= 0)
            return new WheelHit(WheelRegion.Outside, -1);

        var span = 2.0 * Math.PI / count;
        var relative = (((Math.Atan2(dy, dx) - startRad) % (2.0 * Math.PI)) + 2.0 * Math.PI) % (2.0 * Math.PI);
        var index = Math.Min(count - 1, (int)(relative / span));
        return new WheelHit(WheelRegion.Segment, index);
    }
}
