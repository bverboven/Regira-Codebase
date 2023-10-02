using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.Core;
using Regira.Drawing.Enums;
using Regira.Drawing.Utilities;
using Regira.IO.Extensions;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class SkiaUtility
{
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/drawing

    public static SKFilterQuality ToFilterQuality(int quality)
    {
        var skiaQuality = SKFilterQuality.Medium;
        if (quality < 100 * (1 / 3))
        {
            skiaQuality = SKFilterQuality.Low;
        }
        else if (quality > 100 * (2 / 3))
        {
            skiaQuality = SKFilterQuality.High;
        }

        return skiaQuality;
    }
    public static SKSize ToSkiaSize(this Size2D size)
    {
        return new SKSize(size.Width, size.Height);
    }

    public static SKEncodedImageFormat GetFormat(Stream stream)
    {
        var codec = SKCodec.Create(stream);
        stream.Position = 0;
        return codec.EncodedFormat;
    }
    public static SKEncodedImageFormat ToSkiaFormat(this ImageFormat format)
    {
#if NETSTANDARD2_0
            return (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), format.ToString());
#else
        return Enum.Parse<SKEncodedImageFormat>(format.ToString());
#endif
    }
    public static ImageFormat ToImageFormat(this SKEncodedImageFormat format)
    {
#if NETSTANDARD2_0
            return (ImageFormat)Enum.Parse(typeof(ImageFormat), format.ToString());
#else
        return Enum.Parse<ImageFormat>(format.ToString());
#endif
    }
    public static SKBitmap ChangeFormat(SKBitmap src, SKEncodedImageFormat targetFormat)
    {
        var converted = new SKBitmap(new SKImageInfo(src.Width, src.Height));
        using var canvas = new SKCanvas(converted);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(src, new SKPoint());
        using var img = SKImage.FromBitmap(converted);
        var data = img.Encode(targetFormat, 100);
        return SKBitmap.Decode(data);
    }

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
    {
        return file.HasStream()
            ? SKBitmap.Decode(file.Stream)
            : SKBitmap.Decode(file.GetBytes());
    }

    public static SKBitmap CropRectangle(SKBitmap src, SKRect rect)
    {
        var target = new SKBitmap(new SKImageInfo((int)rect.Width, (int)rect.Height));
        using var canvas = new SKCanvas(target);
        canvas.DrawBitmap(src, rect, new SKRect(0, 0, rect.Width, rect.Height));
        return target;
    }


    public static SKBitmap Resize(SKBitmap src, SKSize wantedSize, int quality = 80)
    {
        var targetSize = SizeUtility.CalculateSize(new[] { src.Width, src.Height }, new[] { wantedSize.Width, wantedSize.Height });
        return ResizeFixed(src, new SKSize(targetSize.Width, targetSize.Height), quality);
    }
    public static SKBitmap ResizeFixed(SKBitmap src, SKSize wantedSize, int quality = 80)
    {
        var target = new SKBitmap(new SKImageInfo((int)wantedSize.Width, (int)wantedSize.Height));
        src.ScalePixels(target, ToFilterQuality(quality));
        if (quality < 100)
        {
            using var data = target.Encode(SKEncodedImageFormat.Jpeg, quality);
            return SKBitmap.Decode(data);
        }
        return target;
    }
    public static SKBitmap Rotate(SKBitmap src, double degrees, string? background = null)
    {
        //degrees = (360 + degrees) % 360;
        var newSize = RotateUtility.CalculateSize(new[] { src.Width, src.Height }, degrees);
        var rotated = new SKBitmap(new SKImageInfo((int)newSize.Width, (int)newSize.Height));
        using var canvas = new SKCanvas(rotated);
        var backgroundColor = string.IsNullOrEmpty(background) ? SKColors.Transparent : SKColor.Parse(background);
        canvas.Clear(backgroundColor);
        canvas.Translate(rotated.Width / 2f, rotated.Height / 2f);
        canvas.RotateDegrees((float)degrees);
        canvas.Translate(-src.Width / 2f, -src.Height / 2f);
        canvas.DrawBitmap(src, new SKPoint());
        return rotated;
    }
    public static SKBitmap FlipHorizontal(SKBitmap src)
    {
        var flippedBitmap = new SKBitmap(new SKImageInfo(src.Width, src.Height));
        using var canvas = new SKCanvas(flippedBitmap);
        canvas.Clear();
        canvas.Scale(-1, 1, src.Width / 2f, 0);
        canvas.DrawBitmap(src, new SKPoint());
        return flippedBitmap;
    }
    public static SKBitmap FlipVertical(SKBitmap source)
    {
        var flippedBitmap = new SKBitmap(new SKImageInfo(source.Width, source.Height));
        using var canvas = new SKCanvas(flippedBitmap);
        canvas.Clear();
        canvas.Scale(1, -1, 0, source.Height / 2f);
        canvas.DrawBitmap(source, new SKPoint());
        return flippedBitmap;
    }

    public static SKBitmap MakeTransparent(SKBitmap src, int[]? rgb = null)
    {
        rgb ??= new[] { 245, 245, 245 };
        if (rgb.Length != 3)
        {
            throw new ArgumentException($"{nameof(rgb)} should have 3 values (red, green, blue)");
        }

        for (var r = 0; r < src.Height; r++)
        {
            for (var c = 0; c < src.Width; c++)
            {
                var color = src.GetPixel(c, r);
                if (color.Red > rgb[0] && color.Green > rgb[1] && color.Blue > rgb[2])
                {
                    src.SetPixel(c, r, SKColor.Parse("FFFFFFFF"));
                }
            }
        }

        return src;
    }
    public static bool IsPixelTransparent(SKColor pixel)
    {
        return pixel.Alpha == 0;
    }
}