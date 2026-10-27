using OrbitDrop.Core.Files;

namespace OrbitDrop.Core.Operations;

/// <summary>
/// A single file action the radial menu can offer. Implementations must do real work
/// and report honest success or failure; they never throw for ordinary bad input, they
/// return <see cref="OperationResult.Fail"/>.
/// </summary>
public interface IFileOperation
{
    /// <summary>Stable identifier, e.g. "image.convert". Used for settings and telemetry-free logging.</summary>
    string Id { get; }

    /// <summary>Human label shown on the wheel.</summary>
    string DisplayName { get; }

    OperationCategory Category { get; }

    /// <summary>True only when this operation can genuinely run against the subject. Keeps impossible actions off the wheel.</summary>
    bool CanApplyTo(FileSubject subject);

    /// <summary>
    /// Runs the operation. Must write to a temporary location and move the finished result into
    /// place, must never mutate the input, and must honour cancellation promptly.
    /// </summary>
    Task<OperationResult> ExecuteAsync(OperationContext context, CancellationToken cancellationToken);
}
