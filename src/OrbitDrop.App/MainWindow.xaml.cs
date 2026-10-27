using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OrbitDrop.App.ViewModels;
using OrbitDrop.Core.Files;

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
            Wheel.Configure(_viewModel.Actions, LoadPreview(_viewModel.CurrentSubject));
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

    /// <summary>Loads a small thumbnail for the wheel centre. Images only; null otherwise or on failure.</summary>
    private static ImageSource? LoadPreview(FileSubject? subject)
    {
        if (subject is null || subject.Kind != FileKind.Image || !File.Exists(subject.Path))
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(subject.Path);
            bitmap.DecodePixelWidth = 240;          // small; keeps memory low
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // do not keep the file locked
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null; // unsupported/corrupt preview is not fatal; the wheel still works
        }
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
