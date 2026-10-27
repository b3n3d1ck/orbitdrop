using System.IO;

namespace OrbitDrop.Core.Files;

/// <summary>
/// Maps a path to a <see cref="FileKind"/>. Classification is by extension so it
/// works on paths that do not yet exist (useful for previews and tests); an
/// existing directory is always reported as <see cref="FileKind.Folder"/>.
/// </summary>
public static class FileClassifier
{
    private static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase)
    { "jpg", "jpeg", "jfif", "png", "webp", "bmp", "tiff", "tif", "gif", "ico" };

    private static readonly HashSet<string> VideoExt = new(StringComparer.OrdinalIgnoreCase)
    { "mp4", "mov", "mkv", "webm", "avi" };

    private static readonly HashSet<string> AudioExt = new(StringComparer.OrdinalIgnoreCase)
    { "mp3", "wav", "flac", "aac", "m4a", "ogg", "opus" };

    private static readonly HashSet<string> DocumentExt = new(StringComparer.OrdinalIgnoreCase)
    { "txt", "md", "markdown", "html", "htm", "rtf", "docx", "odt" };

    private static readonly HashSet<string> ArchiveExt = new(StringComparer.OrdinalIgnoreCase)
    { "zip", "tar", "gz", "tgz", "xz", "7z" };

    public static FileKind Classify(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return FileKind.Unknown;

        // A real directory wins regardless of any dot in its name.
        if (Directory.Exists(path))
            return FileKind.Folder;

        var ext = Path.GetExtension(path).TrimStart('.');
        if (ext.Length == 0)
            return FileKind.Unknown;

        if (ImageExt.Contains(ext)) return FileKind.Image;
        if (ext.Equals("pdf", StringComparison.OrdinalIgnoreCase)) return FileKind.Pdf;
        if (VideoExt.Contains(ext)) return FileKind.Video;
        if (AudioExt.Contains(ext)) return FileKind.Audio;
        if (DocumentExt.Contains(ext)) return FileKind.Document;
        if (ArchiveExt.Contains(ext)) return FileKind.Archive;

        return FileKind.Unknown;
    }
}
