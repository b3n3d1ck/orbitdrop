using OrbitDrop.Core.Files;

namespace OrbitDrop.Core.Operations;

/// <summary>Holds every registered operation and answers "what can I do with this file?".</summary>
public interface IOperationRegistry
{
    void Register(IFileOperation operation);
    IReadOnlyList<IFileOperation> All { get; }

    /// <summary>Operations valid for the subject, grouped and ordered for the wheel.</summary>
    IReadOnlyList<IFileOperation> GetApplicable(FileSubject subject);
}

public sealed class OperationRegistry : IOperationRegistry
{
    private readonly List<IFileOperation> _operations = new();

    public void Register(IFileOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (_operations.Any(o => o.Id == operation.Id))
            throw new InvalidOperationException($"An operation with id '{operation.Id}' is already registered.");
        _operations.Add(operation);
    }

    public IReadOnlyList<IFileOperation> All => _operations;

    public IReadOnlyList<IFileOperation> GetApplicable(FileSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return _operations
            .Where(o => o.CanApplyTo(subject))
            .OrderBy(o => o.Category)
            .ThenBy(o => o.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }
}
