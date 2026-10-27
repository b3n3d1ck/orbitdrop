using System.IO;

namespace OrbitDrop.Core.IO;

/// <summary>
/// Produces collision-safe output paths. The scheme matches Windows Explorer:
/// <c>photo.jpg</c>, <c>photo (2).jpg</c>, <c>photo (3).jpg</c>, ...
/// </summary>
public static class OutputNaming
{
    /// <summary>
    /// Returns a path inside <paramref name="directory"/> for <paramref name="desiredFileName"/>
    /// that does not collide with an existing file or directory.
    /// </summary>
    public static string GetNonCollidingPath(string directory, string desiredFileName)
        => GetNonCollidingPath(directory, desiredFileName,
            candidate => File.Exists(candidate) || Directory.Exists(candidate));

    /// <summary>
    /// Testable overload: <paramref name="exists"/> decides whether a candidate path is taken.
    /// </summary>
    public static string GetNonCollidingPath(string directory, string desiredFileName, Func<string, bool> exists)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(desiredFileName);
        ArgumentNullException.ThrowIfNull(exists);

        var name = Path.GetFileNameWithoutExtension(desiredFileName);
        var ext = Path.GetExtension(desiredFileName); // includes leading dot, or empty

        var candidate = Path.Combine(directory, desiredFileName);
        var counter = 2;
        while (exists(candidate))
        {
            candidate = Path.Combine(directory, $"{name} ({counter}){ext}");
            counter++;
        }

        return candidate;
    }
}
