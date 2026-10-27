namespace OrbitDrop.Core.Operations;

/// <summary>Outcome of running one operation against one input. Immutable.</summary>
public sealed class OperationResult
{
    private OperationResult(bool success, string? outputPath, string? message,
        IReadOnlyList<string> warnings, Exception? error)
    {
        Success = success;
        OutputPath = outputPath;
        Message = message;
        Warnings = warnings;
        Error = error;
    }

    public bool Success { get; }
    public string? OutputPath { get; }
    public string? Message { get; }
    public IReadOnlyList<string> Warnings { get; }
    public Exception? Error { get; }

    public static OperationResult Ok(string outputPath, string? message = null, IReadOnlyList<string>? warnings = null)
        => new(true, outputPath, message, warnings ?? Array.Empty<string>(), null);

    public static OperationResult Fail(string message, Exception? error = null)
        => new(false, null, message, Array.Empty<string>(), error);
}
