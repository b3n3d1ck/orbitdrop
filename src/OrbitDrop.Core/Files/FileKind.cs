namespace OrbitDrop.Core.Files;

/// <summary>Coarse classification of a dropped item, used to decide which actions apply.</summary>
public enum FileKind
{
    Unknown,
    Image,
    Pdf,
    Video,
    Audio,
    Document,
    Archive,
    Folder,
}
