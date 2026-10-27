using OrbitDrop.Core.Files;
using OrbitDrop.Core.Operations;
using Xunit;

namespace OrbitDrop.Tests;

public class OperationContextTests
{
    private static OperationContext Make(Dictionary<string, object?> p) => new()
    {
        Subject = new FileSubject("/tmp/a.png"),
        OutputDirectory = "/tmp",
        Parameters = p,
    };

    [Fact]
    public void ReturnsTypedValue()
    {
        var ctx = Make(new() { ["format"] = "jpg" });
        Assert.Equal("jpg", ctx.GetParameter<string>("format"));
    }

    [Fact]
    public void ReturnsFallback_WhenMissing()
    {
        var ctx = Make(new());
        Assert.Equal("png", ctx.GetParameter("format", "png"));
    }

    [Fact]
    public void WidensBoxedNumeric_ToNullableInt()
    {
        var ctx = Make(new() { ["quality"] = 90 });
        Assert.Equal(90, ctx.GetParameter<int?>("quality"));
    }

    [Fact]
    public void ReturnsFallback_OnUnconvertibleType()
    {
        var ctx = Make(new() { ["quality"] = "not-a-number" });
        Assert.Null(ctx.GetParameter<int?>("quality"));
    }
}
