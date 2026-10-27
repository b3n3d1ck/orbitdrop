using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;

namespace OrbitDrop.App.ViewModels;

/// <summary>
/// Builds the concrete wheel choices for a subject. Only actions whose underlying operation
/// genuinely applies are produced, so the wheel never offers something that will fail.
/// </summary>
public static class ActionCatalog
{
    public static IReadOnlyList<RadialAction> ForSubject(FileSubject subject, IOperationRegistry registry)
    {
        var actions = new List<RadialAction>();
        var applicable = registry.GetApplicable(subject);

        foreach (var op in applicable)
        {
            if (op.Id == "image.convert")
            {
                actions.Add(Convert(op, "Convert to JPEG", "jpg"));
                actions.Add(Convert(op, "Convert to PNG", "png"));
                actions.Add(Convert(op, "Convert to WebP", "webp"));
                actions.Add(new RadialAction
                {
                    Label = "Resize to 1080px",
                    Operation = op,
                    Parameters = new Dictionary<string, object?>
                    {
                        ["format"] = subject.Extension is "jpg" or "jpeg" ? "jpg" : "png",
                        ["maxDimension"] = 1080,
                    },
                });
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
