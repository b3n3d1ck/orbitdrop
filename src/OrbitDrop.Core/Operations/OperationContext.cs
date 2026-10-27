using System.Globalization;
using OrbitDrop.Core.Files;

namespace OrbitDrop.Core.Operations;

/// <summary>
/// Everything an operation needs for one input: the subject, where output should go,
/// typed parameters, a progress sink, and the collision policy to honour.
/// </summary>
public sealed class OperationContext
{
    public required FileSubject Subject { get; init; }

    /// <summary>Directory the result is written to. Defaults, at the UI layer, to the input's folder.</summary>
    public required string OutputDirectory { get; init; }

    public IReadOnlyDictionary<string, object?> Parameters { get; init; }
        = new Dictionary<string, object?>();

    public CollisionPolicy CollisionPolicy { get; init; } = CollisionPolicy.Rename;

    /// <summary>Receives 0.0–1.0 progress when the underlying operation can report it; may be null.</summary>
    public IProgress<double>? Progress { get; init; }

    public string InputPath => Subject.Path;

    /// <summary>Reads a typed parameter, returning <paramref name="fallback"/> when absent or not convertible.</summary>
    public T? GetParameter<T>(string key, T? fallback = default)
    {
        if (!Parameters.TryGetValue(key, out var raw) || raw is null)
            return fallback;

        if (raw is T typed)
            return typed;

        // Dictionaries of object frequently hold a boxed numeric of a different width;
        // tolerate that rather than forcing callers to box exactly the right type.
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        try
        {
            return (T)Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return fallback;
        }
    }
}
