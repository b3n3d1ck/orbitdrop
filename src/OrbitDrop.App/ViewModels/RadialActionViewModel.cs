using CommunityToolkit.Mvvm.ComponentModel;
using OrbitDrop.Core.Operations;

namespace OrbitDrop.App.ViewModels;

/// <summary>Wheel-segment view model. Thin wrapper so the view can bind to <see cref="Label"/>.</summary>
public sealed partial class RadialActionViewModel : ObservableObject
{
    public RadialActionViewModel(RadialAction action)
    {
        Action = action;
    }

    public RadialAction Action { get; }

    public string Label => Action.Label;
}
