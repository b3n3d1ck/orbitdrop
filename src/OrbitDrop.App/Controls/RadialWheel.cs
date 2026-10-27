using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using OrbitDrop.App.ViewModels;
using OrbitDrop.Core.Ui;

namespace OrbitDrop.App.Controls;

/// <summary>
/// The donut-style radial action wheel. Everything is drawn in <see cref="OnRender"/> from the
/// current <see cref="UIElement.RenderSize"/>, so it is always correct at any size and DPI (no
/// one-shot layout events). Segments are annular sectors; the centre hole shows a preview of the
/// file and acts as Cancel. Pointer hit-testing and segment geometry come from the pure
/// <see cref="RadialLayout"/> helpers, which are unit-tested.
/// </summary>
public sealed class RadialWheel : FrameworkElement
{
    private static readonly Brush Dim = Frozen(Color.FromArgb(0xB3, 0x0A, 0x0B, 0x0D));
    private static readonly Brush SegmentA = Frozen(Color.FromRgb(0x23, 0x26, 0x2D));
    private static readonly Brush SegmentB = Frozen(Color.FromRgb(0x2A, 0x2E, 0x36));
    private static readonly Brush Accent = Frozen(Color.FromRgb(0x4C, 0x8D, 0xFF));
    private static readonly Brush Hole = Frozen(Color.FromRgb(0x1B, 0x1D, 0x22));
    private static readonly Brush TextBrush = Frozen(Color.FromRgb(0xEC, 0xEE, 0xF2));
    private static readonly Brush CancelBrush = Frozen(Color.FromRgb(0xE5, 0x53, 0x4B));
    private static readonly Brush Scrim = Frozen(Color.FromArgb(0xAA, 0x12, 0x13, 0x16));
    private static readonly Pen Separator = FrozenPen(Color.FromRgb(0x12, 0x13, 0x16), 2);
    private static readonly Pen HoleRing = FrozenPen(Color.FromRgb(0x33, 0x37, 0x3F), 1.5);
    private static readonly Typeface Face = new("Segoe UI");

    private IReadOnlyList<RadialActionViewModel> _actions = Array.Empty<RadialActionViewModel>();
    private ImageSource? _preview;
    private int _focusIndex;
    private int _hoverIndex = -1;
    private bool _hoverHole;

    public event EventHandler<RadialActionViewModel>? ActionSelected;
    public event EventHandler? Cancelled;

    public RadialWheel()
    {
        Focusable = true;
        FocusVisualStyle = null;
        Cursor = Cursors.Hand;

        // Take keyboard focus whenever the wheel becomes visible, so arrow/Enter/Esc work
        // on the first open without needing a click first.
        IsVisibleChanged += (_, e) =>
        {
            if (e.NewValue is true)
                Focus();
        };
    }

    public void Configure(IReadOnlyList<RadialActionViewModel> actions, ImageSource? preview)
    {
        _actions = actions;
        _preview = preview;
        _focusIndex = 0;
        _hoverIndex = -1;
        _hoverHole = false;
        InvalidateVisual();
        Focus();
    }

    private double Width2 => RenderSize.Width;
    private double Height2 => RenderSize.Height;

    protected override void OnRender(DrawingContext dc)
    {
        var w = RenderSize.Width;
        var h = RenderSize.Height;
        dc.DrawRectangle(Dim, null, new Rect(0, 0, w, h));

        var count = _actions.Count;
        var outer = RadialLayout.OuterRadius(w, h);
        var inner = RadialLayout.InnerRadius(outer);
        if (count == 0 || outer <= 0)
            return;

        var center = ToPoint(RadialLayout.Center(w, h));
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        for (var i = 0; i < count; i++)
        {
            var (start, end) = RadialLayout.SegmentAngles(i, count);
            const double pad = 0.012; // small angular gap between segments
            var geometry = SectorGeometry(center, inner, outer, start + pad, end - pad);

            var highlighted = i == _hoverIndex || i == _focusIndex;
            var fill = highlighted ? Accent : (i % 2 == 0 ? SegmentA : SegmentB);
            dc.DrawGeometry(fill, Separator, geometry);

            var label = _actions[i].Label;
            var anchor = RadialLayout.LabelPoint(w, h, i, count, inner, outer);
            var text = new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Face, 15, TextBrush, dpi);
            dc.DrawText(text, new Point(anchor.X - text.Width / 2, anchor.Y - text.Height / 2));
        }

        DrawHole(dc, center, inner, dpi);
    }

    private void DrawHole(DrawingContext dc, Point center, double inner, double dpi)
    {
        dc.DrawEllipse(_hoverHole ? CancelBrush : Hole, HoleRing, center, inner, inner);

        if (_preview is not null && inner > 16)
        {
            var radius = inner - 8;
            var clip = new EllipseGeometry(center, radius, radius);
            clip.Freeze();
            dc.PushClip(clip);

            var pw = _preview.Width;
            var ph = _preview.Height;
            if (pw > 0 && ph > 0)
            {
                var side = radius * 2;
                var scale = Math.Max(side / pw, side / ph); // cover the circle
                var dw = pw * scale;
                var dh = ph * scale;
                dc.DrawImage(_preview, new Rect(center.X - dw / 2, center.Y - dh / 2, dw, dh));
            }
            dc.Pop();
        }

        // "Cancel" hint on a scrim near the bottom of the hole, always legible.
        var cancel = new FormattedText("✕ Cancel", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Face, 13, _hoverHole ? TextBrush : CancelBrush, dpi);
        var cx = center.X - cancel.Width / 2;
        var cy = center.Y + inner - cancel.Height - 10;
        dc.DrawRoundedRectangle(Scrim, null,
            new Rect(cx - 8, cy - 2, cancel.Width + 16, cancel.Height + 4), 6, 6);
        dc.DrawText(cancel, new Point(cx, cy));
    }

    private static StreamGeometry SectorGeometry(Point center, double inner, double outer, double a0, double a1)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var innerStart = OnCircle(center, inner, a0);
            var outerStart = OnCircle(center, outer, a0);
            var outerEnd = OnCircle(center, outer, a1);
            var innerEnd = OnCircle(center, inner, a1);
            var large = (a1 - a0) > Math.PI;

            ctx.BeginFigure(innerStart, isFilled: true, isClosed: true);
            ctx.LineTo(outerStart, true, false);
            ctx.ArcTo(outerEnd, new Size(outer, outer), 0, large, SweepDirection.Clockwise, true, false);
            ctx.LineTo(innerEnd, true, false);
            ctx.ArcTo(innerStart, new Size(inner, inner), 0, large, SweepDirection.Counterclockwise, true, false);
        }
        geometry.Freeze();
        return geometry;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var hit = HitAt(e.GetPosition(this));
        var hoverIndex = hit.Region == WheelRegion.Segment ? hit.Index : -1;
        var hoverHole = hit.Region == WheelRegion.Hole;
        if (hoverIndex != _hoverIndex || hoverHole != _hoverHole)
        {
            _hoverIndex = hoverIndex;
            _hoverHole = hoverHole;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoverIndex != -1 || _hoverHole)
        {
            _hoverIndex = -1;
            _hoverHole = false;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        var hit = HitAt(e.GetPosition(this));
        switch (hit.Region)
        {
            case WheelRegion.Segment when hit.Index >= 0 && hit.Index < _actions.Count:
                ActionSelected?.Invoke(this, _actions[hit.Index]);
                break;
            case WheelRegion.Hole:
            case WheelRegion.Outside:
                Cancelled?.Invoke(this, EventArgs.Empty);
                break;
        }
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_actions.Count == 0)
            return;

        switch (e.Key)
        {
            case Key.Escape:
                Cancelled?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Down:
                Move(1);
                e.Handled = true;
                break;
            case Key.Left:
            case Key.Up:
                Move(-1);
                e.Handled = true;
                break;
            case Key.Enter:
            case Key.Space:
                if (_focusIndex >= 0 && _focusIndex < _actions.Count)
                    ActionSelected?.Invoke(this, _actions[_focusIndex]);
                e.Handled = true;
                break;
        }
    }

    private void Move(int delta)
    {
        _focusIndex = (_focusIndex + delta + _actions.Count) % _actions.Count;
        InvalidateVisual();
    }

    private WheelHit HitAt(Point p)
    {
        var outer = RadialLayout.OuterRadius(Width2, Height2);
        var inner = RadialLayout.InnerRadius(outer);
        return RadialLayout.HitTest(Width2, Height2, inner, outer, _actions.Count, new LayoutPoint(p.X, p.Y));
    }

    private static Point OnCircle(Point center, double radius, double angle)
        => new(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle));

    private static Point ToPoint(LayoutPoint p) => new(p.X, p.Y);

    private static Brush Frozen(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    private static Pen FrozenPen(Color c, double thickness)
    {
        var p = new Pen(Frozen(c), thickness);
        p.Freeze();
        return p;
    }
}
