using System.Globalization;
using System.Text;

namespace OrbitDrop.Operations.Pdf;

/// <summary>
/// Writes a minimal, valid PDF that embeds one JPEG image on one page. The JPEG is stored
/// directly with the <c>DCTDecode</c> filter (no re-encoding), and the cross-reference table
/// is built from byte-accurate offsets recorded as the file is written. No third-party PDF
/// dependency, so nothing here constrains OrbitDrop's license.
/// </summary>
public static class MinimalPdf
{
    /// <summary>
    /// Builds the PDF bytes for a single page of <paramref name="pageWidthPt"/> x
    /// <paramref name="pageHeightPt"/> points, drawing the JPEG (whose pixel size is
    /// <paramref name="imageWidthPx"/> x <paramref name="imageHeightPx"/>) at the given
    /// position and size in points. The PDF origin is the bottom-left corner.
    /// </summary>
    public static byte[] SingleImagePage(
        byte[] jpegBytes,
        int imageWidthPx,
        int imageHeightPx,
        double pageWidthPt,
        double pageHeightPt,
        double drawX,
        double drawY,
        double drawWidth,
        double drawHeight)
    {
        ArgumentNullException.ThrowIfNull(jpegBytes);
        if (jpegBytes.Length == 0)
            throw new ArgumentException("JPEG data is empty.", nameof(jpegBytes));
        if (imageWidthPx <= 0 || imageHeightPx <= 0)
            throw new ArgumentOutOfRangeException(nameof(imageWidthPx), "Image dimensions must be positive.");

        using var ms = new MemoryStream();
        var offsets = new long[6]; // objects 1..5; index 0 is the free entry

        void Ascii(string s)
        {
            var b = Encoding.ASCII.GetBytes(s);
            ms.Write(b, 0, b.Length);
        }

        Ascii("%PDF-1.4\n");

        offsets[1] = ms.Position;
        Ascii("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets[2] = ms.Position;
        Ascii("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets[3] = ms.Position;
        Ascii($"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(pageWidthPt)} {N(pageHeightPt)}]" +
              " /Resources << /XObject << /Im0 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");

        // Content stream: place the unit image via a transform matrix (sw 0 0 sh tx ty cm).
        var content = $"q {N(drawWidth)} 0 0 {N(drawHeight)} {N(drawX)} {N(drawY)} cm /Im0 Do Q";
        var contentBytes = Encoding.ASCII.GetBytes(content);
        offsets[4] = ms.Position;
        Ascii($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes, 0, contentBytes.Length);
        Ascii("\nendstream\nendobj\n");

        offsets[5] = ms.Position;
        Ascii($"5 0 obj\n<< /Type /XObject /Subtype /Image /Width {imageWidthPx} /Height {imageHeightPx}" +
              $" /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpegBytes.Length} >>\nstream\n");
        ms.Write(jpegBytes, 0, jpegBytes.Length);
        Ascii("\nendstream\nendobj\n");

        var xrefOffset = ms.Position;
        Ascii("xref\n0 6\n");
        Ascii("0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
            Ascii($"{offsets[i].ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");

        Ascii($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF");

        return ms.ToArray();
    }

    private static string N(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
