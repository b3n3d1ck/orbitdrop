using System.Windows;
using System.Windows.Controls;
using OrbitDrop.Core.Ui;

namespace OrbitDrop.App.Controls;

/// <summary>
/// Arranges its children evenly around a circle centred in the panel. Placement happens in
/// <see cref="ArrangeOverride"/>, which the layout system calls on every pass — first show,
/// every resize, and every DPI change — so the segments are always positioned correctly.
/// This is the fix for the old Canvas approach, which depended on a one-shot SizeChanged and
/// left all segments stacked at (0,0) when that did not fire in time.
/// </summary>
public sealed class RadialPanel : Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var infinity = new Size(double.PositiveInfinity, double.PositiveInfinity);
        foreach (UIElement child in InternalChildren)
            child.Measure(infinity);

        // Fill the space the parent offers; fall back to 0 when unconstrained.
        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var count = InternalChildren.Count;
        if (count == 0)
            return finalSize;

        double itemHalfWidth = 0, itemHalfHeight = 0;
        foreach (UIElement child in InternalChildren)
        {
            itemHalfWidth = Math.Max(itemHalfWidth, child.DesiredSize.Width / 2.0);
            itemHalfHeight = Math.Max(itemHalfHeight, child.DesiredSize.Height / 2.0);
        }

        var radius = RadialLayout.FitRadius(finalSize.Width, finalSize.Height, itemHalfWidth, itemHalfHeight);

        for (var i = 0; i < count; i++)
        {
            var child = InternalChildren[i];
            var center = RadialLayout.SegmentCenter(finalSize.Width, finalSize.Height, i, count, radius);
            var w = child.DesiredSize.Width;
            var h = child.DesiredSize.Height;
            child.Arrange(new Rect(center.X - w / 2.0, center.Y - h / 2.0, w, h));
        }

        return finalSize;
    }
}
