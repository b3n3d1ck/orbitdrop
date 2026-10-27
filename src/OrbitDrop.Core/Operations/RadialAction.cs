namespace OrbitDrop.Core.Operations;

/// <summary>
/// A concrete choice on the wheel: an operation plus the exact parameters it will run with,
/// and the label the user sees. The wheel shows outcomes ("Convert to JPEG"), not bare operations.
/// WPF-free so the catalog that builds these can be unit-tested.
/// </summary>
public sealed class RadialAction
{
    public required string Label { get; init; }
    public required IFileOperation Operation { get; init; }
    public IReadOnlyDictionary<string, object?> Parameters { get; init; }
        = new Dictionary<string, object?>();
}
