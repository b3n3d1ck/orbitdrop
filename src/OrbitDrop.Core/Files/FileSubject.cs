using System.IO;

namespace OrbitDrop.Core.Files;

/// <summary>
/// A single item a user dropped or picked. Immutable snapshot of the path plus
/// the classification the rest of the app reasons about.
/// </summary>
public sealed class FileSubject
{
    public FileSubject(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path must be provided.", nameof(path));

        Path = path;
        Kind = FileClassifier.Classify(path);
    }

    public string Path { get; }

    public string FileName => System.IO.Path.GetFileName(Path);

    /// <summary>Lower-case extension without the leading dot, e.g. "png". Empty for folders / extensionless files.</summary>
    public string Extension =>
        System.IO.Path.GetExtension(Path).TrimStart('.').ToLowerInvariant();

    public FileKind Kind { get; }

    public bool IsFolder => Kind == FileKind.Folder;
}
