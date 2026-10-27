using System.Text;
using OrbitDrop.Operations.Pdf;
using Xunit;

namespace OrbitDrop.Tests;

public class MinimalPdfTests
{
    private static byte[] DummyJpeg(int length)
    {
        // The writer treats the JPEG as opaque bytes, so any payload exercises the structure.
        var bytes = new byte[length];
        for (var i = 0; i < length; i++)
            bytes[i] = (byte)(i % 256);
        return bytes;
    }

    [Fact]
    public void ProducesAStructurallyValidPdf_WithAccurateXref()
    {
        var pdf = MinimalPdf.SingleImagePage(DummyJpeg(500), 200, 100, 400, 300, 10, 10, 380, 280);
        PdfTestHelpers.AssertValidSingleImagePdf(pdf);
    }

    [Fact]
    public void XrefStaysAccurate_RegardlessOfJpegLength()
    {
        // The image-stream length shifts every later offset; the xref must still be exact.
        foreach (var len in new[] { 1, 37, 4096, 65537 })
        {
            var pdf = MinimalPdf.SingleImagePage(DummyJpeg(len), 640, 480, 595, 842, 18, 18, 559, 806);
            PdfTestHelpers.AssertValidSingleImagePdf(pdf);
        }
    }

    [Fact]
    public void EmbedsTheExactJpegBytes()
    {
        var jpeg = DummyJpeg(1234);
        var pdf = MinimalPdf.SingleImagePage(jpeg, 100, 100, 200, 200, 0, 0, 200, 200);

        // The /Length in the image object must equal the JPEG byte count we passed.
        var text = Encoding.Latin1.GetString(pdf);
        Assert.Contains($"/Filter /DCTDecode /Length {jpeg.Length} >>", text);
    }

    [Fact]
    public void RejectsEmptyJpeg()
    {
        Assert.Throws<ArgumentException>(
            () => MinimalPdf.SingleImagePage(Array.Empty<byte>(), 10, 10, 100, 100, 0, 0, 100, 100));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-5, 10)]
    public void RejectsNonPositiveImageDimensions(int w, int h)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MinimalPdf.SingleImagePage(DummyJpeg(10), w, h, 100, 100, 0, 0, 100, 100));
    }
}
