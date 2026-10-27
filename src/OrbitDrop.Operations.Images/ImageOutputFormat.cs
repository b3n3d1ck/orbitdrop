namespace OrbitDrop.Operations.Images;

/// <summary>The image formats OrbitDrop can write. Kept small and verified against ImageSharp.</summary>
public enum ImageOutputFormat
{
    Jpeg,
    Png,
    Webp,
    Bmp,
    Gif,
    Tiff,
}

public static class ImageOutputFormatInfo
{
    /// <summary>Canonical lower-case extension (no dot) for the format.</summary>
    public static string Extension(this ImageOutputFormat format) => format switch
    {
        ImageOutputFormat.Jpeg => "jpg",
        ImageOutputFormat.Png => "png",
        ImageOutputFormat.Webp => "webp",
        ImageOutputFormat.Bmp => "bmp",
        ImageOutputFormat.Gif => "gif",
        ImageOutputFormat.Tiff => "tiff",
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };

    /// <summary>Whether the format preserves an alpha channel. JPEG and BMP do not.</summary>
    public static bool SupportsAlpha(this ImageOutputFormat format) => format switch
    {
        ImageOutputFormat.Png or ImageOutputFormat.Webp
            or ImageOutputFormat.Gif or ImageOutputFormat.Tiff => true,
        _ => false,
    };

    /// <summary>Whether a lossy quality setting applies. Only JPEG and WebP here.</summary>
    public static bool UsesQuality(this ImageOutputFormat format)
        => format is ImageOutputFormat.Jpeg or ImageOutputFormat.Webp;

    public static bool TryParse(string? token, out ImageOutputFormat format)
    {
        switch (token?.Trim().ToLowerInvariant())
        {
            case "jpg" or "jpeg": format = ImageOutputFormat.Jpeg; return true;
            case "png": format = ImageOutputFormat.Png; return true;
            case "webp": format = ImageOutputFormat.Webp; return true;
            case "bmp": format = ImageOutputFormat.Bmp; return true;
            case "gif": format = ImageOutputFormat.Gif; return true;
            case "tif" or "tiff": format = ImageOutputFormat.Tiff; return true;
            default: format = default; return false;
        }
    }
}
