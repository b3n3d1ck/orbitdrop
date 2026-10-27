namespace OrbitDrop.Core.Operations;

/// <summary>How an operation reacts when its intended output path already exists.</summary>
public enum CollisionPolicy
{
    /// <summary>Pick the next free "name (n).ext". The safe default.</summary>
    Rename,

    /// <summary>Overwrite the existing file. Only ever used on explicit user consent.</summary>
    Overwrite,

    /// <summary>Abort the operation and report a failure.</summary>
    Fail,
}
