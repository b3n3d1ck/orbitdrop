using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OrbitDrop.App.ViewModels;

namespace OrbitDrop.App.Controls;

/// <summary>
/// The radial action wheel. Segments are laid out around a circle; the centre button cancels.
/// Fully keyboard-navigable (arrows move between segments, Enter/Space activates, Esc cancels),
/// which also serves as the accessible path alongside the mouse/drag interaction.
/// </summary>
public sealed class RadialMenu : UserControl
{
    private readonly Canvas _surface = new();
    private readonly Button _center;
    private readonly List<Button> _buttons = new();
    private int _focusIndex = -1;

    public event EventHandler<RadialActionViewModel>? ActionSelected;
    public event EventHandler? Cancelled;

    public RadialMenu()
    {
        Focusable = true;

        _center = new Button { Content = "Cancel", Width = 104, Height = 104, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        ApplyWheelStyle(_center, accent: false);
        _center.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        var root = new Grid { Background = new SolidColorBrush(Color.FromArgb(0xB3, 0x0A, 0x0B, 0x0D)) };
        root.Children.Add(_surface);
        root.Children.Add(_center);
        Content = root;

        SizeChanged += (_, _) => Relayout();
        Loaded += (_, _) => { Relayout(); FocusCurrent(); };
    }

    public void SetActions(IReadOnlyList<RadialActionViewModel> actions)
    {
        _surface.Children.Clear();
        _buttons.Clear();

        foreach (var action in actions)
        {
            var button = new Button
            {
                Content = action.Label,
                Tag = action,
                MinWidth = 160,
                Height = 46,
                FocusVisualStyle = null,
            };
            ApplyWheelStyle(button, accent: true);
            button.Click += OnSegmentClick;
            _buttons.Add(button);
            _surface.Children.Add(button);
        }

        _focusIndex = _buttons.Count > 0 ? 0 : -1;
        Relayout();
        FocusCurrent();
    }

    private void OnSegmentClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: RadialActionViewModel action })
            ActionSelected?.Invoke(this, action);
    }

    private void Relayout()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0 || _buttons.Count == 0)
            return;

        var cx = ActualWidth / 2.0;
        var cy = ActualHeight / 2.0;
        var radius = Math.Max(140, Math.Min(cx, cy) * 0.64);

        for (var i = 0; i < _buttons.Count; i++)
        {
            var button = _buttons[i];
            button.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var bw = button.DesiredSize.Width;
            var bh = button.DesiredSize.Height;

            // Start at 12 o'clock and go clockwise.
            var angle = -Math.PI / 2.0 + i * (2.0 * Math.PI / _buttons.Count);
            var x = cx + radius * Math.Cos(angle);
            var y = cy + radius * Math.Sin(angle);

            Canvas.SetLeft(button, x - bw / 2.0);
            Canvas.SetTop(button, y - bh / 2.0);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_buttons.Count == 0)
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
        }
    }

    private void Move(int delta)
    {
        if (_buttons.Count == 0)
            return;
        _focusIndex = (_focusIndex + delta + _buttons.Count) % _buttons.Count;
        FocusCurrent();
    }

    private void FocusCurrent()
    {
        if (_focusIndex >= 0 && _focusIndex < _buttons.Count)
            _buttons[_focusIndex].Focus();
    }

    private void ApplyWheelStyle(Button button, bool accent)
    {
        if (TryFindResource("PrimaryButton") is Style style)
            button.Style = style;
        if (accent && TryFindResource("Accent") is Brush accentBrush)
            button.BorderBrush = accentBrush;
    }
}
