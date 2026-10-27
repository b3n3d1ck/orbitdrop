namespace OrbitDrop.Core.Ui;

/// <summary>A point in device-independent layout coordinates. Kept WPF-free so it is testable anywhere.</summary>
public readonly record struct LayoutPoint(double X, double Y);

/// <summary>
/// Pure geometry for the radial menu. No WPF types, no side effects, so the placement rules can
/// be unit-tested directly. The WPF panel delegates to this; the two never disagree.
/// </summary>
public static class RadialLayout
{
    /// <summary>The centre of the wheel within a box of the given size.</summary>
    public static LayoutPoint Center(double width, double height) => new(width / 2.0, height / 2.0);

    /// <summary>
    /// The largest radius that keeps every item fully inside the box, given each item's half-extent
    /// and a margin, while not exceeding a proportional preferred radius. Never negative.
    /// </summary>
    public static double FitRadius(
        double width,
        double height,
        double itemHalfWidth,
        double itemHalfHeight,
        double preferredFactor = 0.62,
        double margin = 8.0)
    {
        if (width <= 0 || height <= 0)
            return 0;

        var preferred = Math.Min(width, height) / 2.0 * preferredFactor;
        var capForWidth = width / 2.0 - itemHalfWidth - margin;
        var capForHeight = height / 2.0 - itemHalfHeight - margin;
        var cap = Math.Min(capForWidth, capForHeight);

        return Math.Max(0, Math.Min(preferred, cap));
    }

    /// <summary>
    /// Centre point of segment <paramref name="index"/> of <paramref name="count"/>, starting at
    /// 12 o'clock and going clockwise, on a circle of <paramref name="radius"/> about the box centre.
    /// </summary>
    public static LayoutPoint SegmentCenter(double width, double height, int index, int count, double radius)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var angle = -Math.PI / 2.0 + index * (2.0 * Math.PI / count);
        var center = Center(width, height);
        return new LayoutPoint(
            center.X + radius * Math.Cos(angle),
            center.Y + radius * Math.Sin(angle));
    }
}
