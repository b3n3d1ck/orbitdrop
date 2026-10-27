using System.IO;

namespace OrbitDrop.Core.IO;

/// <summary>
/// Guards against path-traversal (the classic "zip slip") when combining a
/// trusted base directory with an untrusted relative entry name. Used by archive
/// extraction and anywhere a filename comes from file contents rather than the user.
/// </summary>
public static class PathSafety
{
    /// <summary>
    /// True if <paramref name="candidatePath"/> resolves to a location at or beneath
    /// <paramref name="baseDirectory"/>. Comparison is done on fully-resolved paths so
    /// <c>..</c> segments cannot escape.
    /// </summary>
    public static bool IsWithinDirectory(string baseDirectory, string candidatePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);

        var baseFull = Path.GetFullPath(baseDirectory);
        var candidateFull = Path.GetFullPath(candidatePath);

        var relative = Path.GetRelativePath(baseFull, candidateFull);
        if (relative == ".")
            return true;

        // Escapes the base if it has to climb out ("..") or is rooted elsewhere.
        return !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }

    /// <summary>
    /// Resolves an untrusted archive entry name against a destination directory,
    /// throwing if the result would escape it.
    /// </summary>
    /// <exception cref="UnsafePathException">The entry escapes the destination.</exception>
    public static string ResolveSafeEntryPath(string destinationDirectory, string entryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryName);

        // Normalise separators coming from foreign archives before combining.
        var normalized = entryName.Replace('\\', '/');
        var combined = Path.Combine(destinationDirectory, normalized);

        if (!IsWithinDirectory(destinationDirectory, combined))
            throw new UnsafePathException(entryName);

        return Path.GetFullPath(combined);
    }
}

/// <summary>Thrown when an untrusted path would escape its intended directory.</summary>
public sealed class UnsafePathException : Exception
{
    public UnsafePathException(string entryName)
        : base($"Archive entry '{entryName}' would be written outside the destination directory and was rejected.")
    {
        EntryName = entryName;
    }

    public string EntryName { get; }
}
