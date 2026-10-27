using System.Text;
using Xunit;

namespace OrbitDrop.Tests;

/// <summary>
/// Structural assertions for the hand-written single-image PDF. The important one is the
/// cross-reference check: it re-reads the xref table and proves every byte offset lands exactly
/// on its object header, which is the part of a hand-rolled PDF most likely to be wrong.
/// </summary>
internal static class PdfTestHelpers
{
    public static void AssertValidSingleImagePdf(byte[] pdf)
    {
        // Latin1 keeps a 1:1 char/byte mapping, so string indices equal byte offsets.
        var text = Encoding.Latin1.GetString(pdf);

        Assert.StartsWith("%PDF-1.4", text);
        Assert.EndsWith("%%EOF", text);
        Assert.Contains("/Type /Catalog", text);
        Assert.Contains("/Type /Pages", text);
        Assert.Contains("/Type /Page ", text); // the page object (trailing space rules out /Pages)
        Assert.Contains("/Count 1", text);
        Assert.Contains("/Filter /DCTDecode", text);
        Assert.Contains("/MediaBox", text);

        var startXrefToken = "startxref";
        var sx = text.LastIndexOf(startXrefToken, StringComparison.Ordinal);
        Assert.True(sx >= 0, "startxref missing");

        var after = text[(sx + startXrefToken.Length)..].TrimStart('\r', '\n', ' ');
        var digits = new string(after.TakeWhile(char.IsDigit).ToArray());
        var xrefOffset = int.Parse(digits);

        Assert.Equal("xref", text.Substring(xrefOffset, 4));

        // Header "xref\n0 6\n" is 9 bytes; entry 0 is the free entry; entries are 20 bytes each.
        var entriesStart = xrefOffset + "xref\n0 6\n".Length;
        for (var i = 1; i <= 5; i++)
        {
            var entry = text.Substring(entriesStart + i * 20, 20);
            var offset = int.Parse(entry[..10]);
            var header = $"{i} 0 obj";
            Assert.True(
                text.Substring(offset, header.Length) == header,
                $"xref entry {i} points to offset {offset} which is not '{header}'");
        }
    }
}
