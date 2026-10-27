using System.ComponentModel;
using System.Runtime.CompilerServices;
using OrbitDrop.Core.Operations;

namespace OrbitDrop.Core.Jobs;

/// <summary>
/// A tracked unit of work: one operation against one input. Raises
/// <see cref="INotifyPropertyChanged"/> so the UI can bind directly to it.
/// State transitions are the only way to change status, which keeps the lifecycle honest.
/// </summary>
public sealed class Job : INotifyPropertyChanged
{
    private JobStatus _status = JobStatus.Pending;
    private double _progress;
    private OperationResult? _result;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _completedAt;

    public Job(IFileOperation operation, OperationContext context)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Guid Id { get; } = Guid.NewGuid();
    public IFileOperation Operation { get; }
    public OperationContext Context { get; }

    public string InputPath => Context.InputPath;
    public string OperationName => Operation.DisplayName;

    public JobStatus Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    /// <summary>0.0–1.0. Only meaningful while <see cref="Status"/> is <see cref="JobStatus.Running"/>.</summary>
    public double Progress
    {
        get => _progress;
        internal set => Set(ref _progress, Math.Clamp(value, 0.0, 1.0));
    }

    public OperationResult? Result
    {
        get => _result;
        private set => Set(ref _result, value);
    }

    public DateTimeOffset? StartedAt
    {
        get => _startedAt;
        private set => Set(ref _startedAt, value);
    }

    public DateTimeOffset? CompletedAt
    {
        get => _completedAt;
        private set => Set(ref _completedAt, value);
    }

    public TimeSpan? Duration =>
        StartedAt is { } s && CompletedAt is { } c ? c - s : null;

    internal void MarkRunning()
    {
        if (Status != JobStatus.Pending)
            throw new InvalidOperationException($"Cannot start a job in state {Status}.");
        StartedAt = DateTimeOffset.Now;
        Status = JobStatus.Running;
    }

    internal void MarkSucceeded(OperationResult result)
    {
        Result = result;
        Progress = 1.0;
        CompletedAt = DateTimeOffset.Now;
        Status = JobStatus.Succeeded;
    }

    internal void MarkFailed(OperationResult result)
    {
        Result = result;
        CompletedAt = DateTimeOffset.Now;
        Status = JobStatus.Failed;
    }

    internal void MarkCancelled()
    {
        CompletedAt = DateTimeOffset.Now;
        Status = JobStatus.Cancelled;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        if (name == nameof(StartedAt) || name == nameof(CompletedAt))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
    }
}
