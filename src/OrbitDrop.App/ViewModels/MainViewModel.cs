using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OrbitDrop.Core.Files;
using OrbitDrop.Core.Jobs;
using OrbitDrop.Core.Operations;
using OrbitDrop.Operations.Images;
using OrbitDrop.Operations.Pdf;

namespace OrbitDrop.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IOperationRegistry _registry = new OperationRegistry();
    private readonly JobQueue _queue = new(maxConcurrency: 2);

    [ObservableProperty] private bool _isWheelOpen;
    [ObservableProperty] private string _statusText = "Drop an image anywhere in this window to begin.";
    [ObservableProperty] private FileSubject? _currentSubject;

    public MainViewModel()
    {
        // Compose the operations available in this slice. More engines register here later.
        _registry.Register(new ImageConvertOperation());
        _registry.Register(new ImageToPdfOperation());
    }

    public ObservableCollection<RadialActionViewModel> Actions { get; } = new();
    public ObservableCollection<Job> Jobs { get; } = new();

    public string SubjectName => CurrentSubject?.FileName ?? "No file selected";

    partial void OnCurrentSubjectChanged(FileSubject? value) => OnPropertyChanged(nameof(SubjectName));

    [RelayCommand]
    private void PickFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a file for OrbitDrop",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.tiff;*.tif;*.gif;*.ico|All files|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog() == true)
            LoadFiles(new[] { dialog.FileName });
    }

    [RelayCommand]
    private void CancelWheel()
    {
        IsWheelOpen = false;
        StatusText = "Cancelled. Drop another file to start again.";
    }

    /// <summary>Command form of <see cref="InvokeActionAsync"/> for binding (accessible action list).</summary>
    [RelayCommand]
    private Task Invoke(RadialActionViewModel? action)
        => action is null ? Task.CompletedTask : InvokeActionAsync(action);

    /// <summary>Entry point from drag-and-drop and the file picker. Uses the first item for this slice.</summary>
    public void LoadFiles(IReadOnlyList<string> paths)
    {
        var first = paths.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
        if (first is null)
            return;

        CurrentSubject = new FileSubject(first);

        Actions.Clear();
        foreach (var action in ActionCatalog.ForSubject(CurrentSubject, _registry))
            Actions.Add(new RadialActionViewModel(action));

        if (Actions.Count > 0)
        {
            IsWheelOpen = true;
            StatusText = $"{CurrentSubject.FileName} — choose an action, or press Esc to cancel.";
        }
        else
        {
            IsWheelOpen = false;
            StatusText = $"No actions available for {CurrentSubject.FileName} yet.";
        }
    }

    public async Task InvokeActionAsync(RadialActionViewModel actionViewModel)
    {
        var subject = CurrentSubject;
        if (subject is null)
            return;

        var action = actionViewModel.Action;
        var outputDirectory = Path.GetDirectoryName(subject.Path);
        if (string.IsNullOrEmpty(outputDirectory))
            outputDirectory = Environment.CurrentDirectory;

        var job = JobFactory.Create(action.Operation, subject, outputDirectory, action.Parameters);
        Jobs.Insert(0, job);

        IsWheelOpen = false;
        StatusText = $"Running {action.Label}…";

        try
        {
            await _queue.RunAsync(job);
        }
        catch (OperationCanceledException)
        {
            StatusText = $"{action.Label} was cancelled.";
            return;
        }

        StatusText = job.Status == JobStatus.Succeeded && job.Result?.OutputPath is { } output
            ? $"Done → {Path.GetFileName(output)}"
            : $"Failed: {job.Result?.Message ?? "unknown error"}";
    }
}
