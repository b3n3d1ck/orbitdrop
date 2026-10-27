using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;

namespace OrbitDrop.Core.Jobs;

/// <summary>
/// Builds a <see cref="Job"/> with its progress sink already wired to the job, so every
/// caller (UI and tests alike) reports progress the same way. The <see cref="Progress{T}"/>
/// marshals callbacks to the captured synchronization context, which is the UI thread when
/// created there.
/// </summary>
public static class JobFactory
{
    public static Job Create(
        IFileOperation operation,
        FileSubject subject,
        string outputDirectory,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CollisionPolicy collisionPolicy = CollisionPolicy.Rename)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(subject);

        Job? job = null;
        var progress = new Progress<double>(p =>
        {
            if (job is not null)
                job.Progress = p;
        });

        var context = new OperationContext
        {
            Subject = subject,
            OutputDirectory = outputDirectory,
            Parameters = parameters ?? new Dictionary<string, object?>(),
            CollisionPolicy = collisionPolicy,
            Progress = progress,
        };

        job = new Job(operation, context);
        return job;
    }
}
