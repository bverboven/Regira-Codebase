using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Utilities;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class SkiaUtility
{
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/drawing

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
        var info = new SKImageInfo(src.Width, src.Height, src.ColorType, SKAlphaType.Premul);
        var converted = new SKBitmap(info);
        using (var canvas = new SKCanvas(converted))
        {
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(src, new SKPoint());
        }
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
        var target = new SKBitmap(new SKImageInfo((int)rect.Width, (int)rect.Height, src.ColorType, src.AlphaType));
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
        var target = new SKBitmap(new SKImageInfo((int)wantedSize.Width, (int)wantedSize.Height, src.ColorType, src.AlphaType));
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
        rgb ??= [245, 245, 245];
        if (rgb.Length != 3)
        {
            throw new ArgumentException($"{nameof(rgb)} should have 3 values (red, green, blue)");
        }

        var transparentImage = new SKBitmap(new SKImageInfo(src.Width, src.Height, src.ColorType, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(transparentImage))
        {
            canvas.DrawBitmap(src, 0, 0);
        }

        for (var r = 0; r < transparentImage.Height; r++)
        {
            for (var c = 0; c < transparentImage.Width; c++)
            {
                var color = transparentImage.GetPixel(c, r);
                if (color.Red > rgb[0] && color.Green > rgb[1] && color.Blue > rgb[2])
                {
                    transparentImage.SetPixel(c, r, SKColors.Transparent);
                }
            }
        }

        return transparentImage;
    }
    public static bool IsPixelTransparent(SKColor pixel)
    {
        return pixel.Alpha == 0;
    }
    public static SKBitmap ChangeOpacity(SKBitmap img, double opacity)
    {
        if (Math.Abs(opacity - 1) < double.Epsilon)
        {
            return img.Copy();
        }

        var info = new SKImageInfo(img.Width, img.Height);
        var newImg = new SKBitmap(info);

        using var canvas = new SKCanvas(newImg);

        canvas.DrawBitmap(img, 0, 0);

        using var paint = new SKPaint();
        paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
        paint.BlendMode = SKBlendMode.DstIn;

        canvas.DrawRect(SKRect.Create(img.Width, img.Height), paint);

        return newImg;
    }


    public static SKBitmap Create(int width, int height, string? backgroundColor = "#FFFFFF", ImageFormat? format = null)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColor.Parse(backgroundColor));
        return bitmap;
    }
    public static SKBitmap CreateTextImage(string input, TextImageOptions? options = null)
    {
        options ??= new TextImageOptions();

        var textColor = SKColor.Parse(options.TextColor);
        var backgroundColor = SKColor.Parse(options.BackgroundColor);

        // Create SKFont for measuring
        using var typeface = SKTypeface.FromFamilyName(options.FontName, SKFontStyle.Normal);
        using var font = new SKFont(typeface, options.FontSize);

        // Measure text width using SKFont
        float textWidth = font.MeasureText(input);
        float textHeight = font.Metrics.Descent - font.Metrics.Ascent;

        // Add padding
        int width = (int)Math.Ceiling(textWidth);
        int height = (int)Math.Ceiling(textHeight);

        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(backgroundColor);

        // Use SKPaint for styling draw
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = textColor;

        // Baseline-adjusted position
        float x = 0;
        float y = - font.Metrics.Ascent;

        canvas.DrawText(input, x, y, font, paint);

        // Step 5: Save to file
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return SKBitmap.Decode(data);
    }
}