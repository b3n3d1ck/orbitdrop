using OrbitDrop.Core.Files;
using OrbitDrop.Core.Jobs;
using OrbitDrop.Core.Operations;
using Xunit;

namespace OrbitDrop.Tests;

public class JobQueueTests
{
    private static Job MakeJob(IFileOperation op)
        => JobFactory.Create(op, new FileSubject("/tmp/a.png"), "/tmp");

    [Fact]
    public async Task SuccessfulOperation_MarksJobSucceeded()
    {
        using var queue = new JobQueue();
        var job = MakeJob(new FakeOperation(ctx => OperationResult.Ok("/tmp/a.jpg")));

        await queue.RunAsync(job);

        Assert.Equal(JobStatus.Succeeded, job.Status);
        Assert.Equal(1.0, job.Progress);
        Assert.NotNull(job.Duration);
    }

    [Fact]
    public async Task FailedResult_MarksJobFailed_WithoutThrowing()
    {
        using var queue = new JobQueue();
        var job = MakeJob(new FakeOperation(_ => OperationResult.Fail("bad input")));

        await queue.RunAsync(job);

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("bad input", job.Result!.Message);
    }

    [Fact]
    public async Task ThrownException_BecomesFailedJob_NotUnhandled()
    {
        using var queue = new JobQueue();
        var job = MakeJob(new FakeOperation(_ => throw new InvalidOperationException("boom")));

        await queue.RunAsync(job);

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.IsType<InvalidOperationException>(job.Result!.Error);
    }

    [Fact]
    public async Task Cancellation_MarksJobCancelled()
    {
        using var queue = new JobQueue();
        using var cts = new CancellationTokenSource();
        var job = MakeJob(new FakeOperation(async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return OperationResult.Ok("/tmp/a.jpg");
        }));

        var run = queue.RunAsync(job, cts.Token);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Equal(JobStatus.Cancelled, job.Status);
    }

    [Fact]
    public async Task Concurrency_IsBounded()
    {
        const int limit = 2;
        using var queue = new JobQueue(limit);
        var tracker = new ConcurrencyTracker();

        var jobs = Enumerable.Range(0, 8)
            .Select(_ => MakeJob(new FakeOperation(async (_, ct) =>
            {
                tracker.Enter();
                await Task.Delay(25, ct);
                tracker.Exit();
                return OperationResult.Ok("/tmp/a.jpg");
            })))
            .ToList();

        await Task.WhenAll(jobs.Select(j => queue.RunAsync(j)));

        Assert.True(tracker.Peak <= limit, $"Peak concurrency {tracker.Peak} exceeded limit {limit}.");
        Assert.All(jobs, j => Assert.Equal(JobStatus.Succeeded, j.Status));
    }

    private sealed class ConcurrencyTracker
    {
        private int _current;
        private int _peak;
        private readonly object _lock = new();

        public int Peak { get { lock (_lock) return _peak; } }

        public void Enter()
        {
            lock (_lock)
            {
                _current++;
                if (_current > _peak) _peak = _current;
            }
        }

        public void Exit()
        {
            lock (_lock) _current--;
        }
    }

    private sealed class FakeOperation : IFileOperation
    {
        private readonly Func<OperationContext, CancellationToken, Task<OperationResult>> _body;

        public FakeOperation(Func<OperationContext, OperationResult> body)
            => _body = (ctx, _) => Task.FromResult(body(ctx));

        public FakeOperation(Func<OperationContext, CancellationToken, Task<OperationResult>> body)
            => _body = body;

        public string Id => "fake.op";
        public string DisplayName => "Fake";
        public OperationCategory Category => OperationCategory.FileAction;
        public bool CanApplyTo(FileSubject subject) => true;

        public Task<OperationResult> ExecuteAsync(OperationContext context, CancellationToken cancellationToken)
            => _body(context, cancellationToken);
    }
}
