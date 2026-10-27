using OrbitDrop.Core.Operations;

namespace OrbitDrop.Core.Jobs;

/// <summary>
/// Runs jobs with bounded concurrency so a batch cannot exhaust the machine. The queue
/// owns the job lifecycle: it marks running/succeeded/failed/cancelled and never lets an
/// operation exception escape as anything other than a failed job.
/// </summary>
public sealed class JobQueue : IDisposable
{
    private readonly SemaphoreSlim _gate;

    public JobQueue(int maxConcurrency = 2)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "At least one worker is required.");
        _gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    /// <summary>
    /// Runs a single job to completion, respecting the concurrency gate and cancellation.
    /// Returns when the job reaches a terminal state. Cancellation surfaces as a cancelled
    /// job and a rethrown <see cref="OperationCanceledException"/>.
    /// </summary>
    public async Task RunAsync(Job job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        try
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            job.MarkCancelled();
            throw;
        }

        try
        {
            job.MarkRunning();

            OperationResult result;
            try
            {
                result = await job.Operation.ExecuteAsync(job.Context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                job.MarkCancelled();
                throw;
            }
            catch (Exception ex)
            {
                // Never swallow: surface it as a failed job with the exception attached.
                job.MarkFailed(OperationResult.Fail("The operation threw an unexpected error.", ex));
                return;
            }

            if (result.Success)
                job.MarkSucceeded(result);
            else
                job.MarkFailed(result);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
