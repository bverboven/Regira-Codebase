using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class ConversionUtility
{
    // Quality
    public static SKSamplingOptions ToFilterQuality(int quality)
    {
#if NETCOREAPP3_0_OR_GREATER
        quality = Math.Clamp(quality, 0, 100);
#endif

        var skiaQuality = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        if (quality < 100 * 0.3333)
        {
            skiaQuality = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
        }
        else if (quality > 100 * 0.6667)
        {
            skiaQuality = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
        }

        return skiaQuality;
    }

    // Size
    public static SKSize ToSkiaSize(this Size2D size)
        => new(size.Width, size.Height);

    // Format
    public static SKEncodedImageFormat GetFormat(Stream stream)
    {
        var codec = SKCodec.Create(stream);
        stream.Position = 0;
        return codec.EncodedFormat;
    }
    public static SKEncodedImageFormat ToSkiaFormat(this ImageFormat format)
#if NETSTANDARD2_0
        => (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), format.ToString());
#else
        => Enum.Parse<SKEncodedImageFormat>(format.ToString());
#endif
    public static ImageFormat ToImageFormat(this SKEncodedImageFormat format)
#if NETSTANDARD2_0
        => (ImageFormat)Enum.Parse(typeof(ImageFormat), format.ToString());
#else
        => Enum.Parse<ImageFormat>(format.ToString());
#endif

    // Color
    public static Color ToColor(this SKColor color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);
    public static SKColor ToSkiaColor(this Color color)
        => new(color.Red, color.Green, color.Blue, color.Alpha);

    // File
    public static IImageFile ToImageFile(this SKBitmap src, SKEncodedImageFormat format = SKEncodedImageFormat.Png)
    {
        using var img = SKImage.FromBitmap(src);
        using var data = img.Encode(format, 100);
        return new ImageFile
        {
            Bytes = data.ToArray(),
            Size = new(src.Width, src.Height),
            Format = ToImageFormat(format),
            Length = data.Size
        };
    }
    public static SKBitmap ToBitmap(this IImageFile file)
        => file.HasStream()
            ? SKBitmap.Decode(file.Stream)
            : SKBitmap.Decode(file.GetBytes());
}