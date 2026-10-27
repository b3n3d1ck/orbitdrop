using OrbitDrop.Core.Files;

namespace OrbitDrop.Core.Operations;

/// <summary>
/// Builds the concrete wheel choices for a subject. Only actions whose underlying operation
/// genuinely applies are produced, so the wheel never offers something that will fail. Labels
/// are kept short because they sit inside wheel segments.
/// </summary>
public static class ActionCatalog
{
    public static IReadOnlyList<RadialAction> ForSubject(FileSubject subject, IOperationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(registry);

        var actions = new List<RadialAction>();

        foreach (var op in registry.GetApplicable(subject))
        {
            switch (op.Id)
            {
                case "image.convert":
                    actions.Add(Convert(op, "JPEG", "jpg"));
                    actions.Add(Convert(op, "PNG", "png"));
                    actions.Add(Convert(op, "WebP", "webp"));
                    actions.Add(Convert(op, "BMP", "bmp"));
                    actions.Add(Convert(op, "GIF", "gif"));
                    actions.Add(Convert(op, "TIFF", "tiff"));
                    break;

                case "image.to-pdf":
                    actions.Add(new RadialAction { Label = "PDF", Operation = op });
                    break;
            }
        }

        return actions;
    }

    private static RadialAction Convert(IFileOperation op, string label, string format) => new()
    {
        Label = label,
        Operation = op,
        Parameters = new Dictionary<string, object?> { ["format"] = format },
    };
}
