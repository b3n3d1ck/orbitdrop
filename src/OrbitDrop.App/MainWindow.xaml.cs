using System.ComponentModel;
using System.Windows;
using OrbitDrop.App.ViewModels;

namespace OrbitDrop.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Wheel.ActionSelected += OnWheelActionSelected;
        Wheel.Cancelled += OnWheelCancelled;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsWheelOpen) && _viewModel.IsWheelOpen)
        {
            Wheel.SetActions(_viewModel.Actions);
        }
    }

    private async void OnWheelActionSelected(object? sender, RadialActionViewModel action)
    {
        await _viewModel.InvokeActionAsync(action);
    }

    private void OnWheelCancelled(object? sender, EventArgs e)
    {
        _viewModel.CancelWheelCommand.Execute(null);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        if (e.Data.GetDataPresent(DataFormats.FileDrop)
            && e.Data.GetData(DataFormats.FileDrop) is string[] files
            && files.Length > 0)
        {
            _viewModel.LoadFiles(files);
        }
    }

    private void OnAboutClick(object sender, RoutedEventArgs e)
    {
        new AboutWindow { Owner = this }.ShowDialog();
    }
}
