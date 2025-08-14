using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class DrawUtility
{
    public static SKBitmap Draw(IEnumerable<ImageToAdd> imagesToAdd, SKBitmap? target = null, int dpi = ImageConstants.DEFAULT_DPI)
    {
        var images = imagesToAdd.ToList();
        target ??= CreateSizedCanvas(images);

        if (!images.Any())
        {
            return target;
        }

        var canvas = new SKCanvas(target);

        foreach (var img in images)
        {
            AddImage(img, canvas, new Size2D(target.Width, target.Height), dpi);
        }

        canvas.Flush();
        return target;
    }

    public static SKBitmap CreateSizedCanvas(IEnumerable<ImageToAdd> imagesToAdd)
    {
        var images = imagesToAdd.ToList();
        var size = new System.Drawing.Size(
            (int)images.Max(x => x.Width > 0 ? x.Width : x.Image.Size?.Width ?? 0),
            (int)images.Max(x => x.Height > 0 ? x.Height : x.Image.Size?.Height ?? 0)
        );
        return new SKBitmap(size.Width, size.Height);
    }

    public static void AddImage(ImageToAdd img, SKCanvas canvas, Size2D targetSize, int dpi = ImageConstants.DEFAULT_DPI)
    {
        SKBitmap source;

        if (img.Image.HasStream())
        {
            source = SKBitmap.Decode(img.Image.Stream!);
        }
        else
        {
            using var ms = new MemoryStream(img.Image.GetBytes()!);
            source = SKBitmap.Decode(ms);
        }

        // Change opacity
        using var opacityImage = SkiaUtility.ChangeOpacity(source, img.Opacity);

        // Rotate if needed
        using var rotatedImage = Math.Abs(img.Rotation) > double.Epsilon
            ? SkiaUtility.Rotate(opacityImage, (float)img.Rotation)
            : opacityImage.Copy();

        // Calculate target width/height
        var imgWidth = SizeUtility.GetPixels(img.Width, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgHeight = SizeUtility.GetPixels(img.Height, img.DimensionUnit, (int)targetSize.Height, dpi);
        var imgLeft = SizeUtility.GetPixels(img.Left, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgRight = SizeUtility.GetPixels(img.Right, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgTop = SizeUtility.GetPixels(img.Top, img.DimensionUnit, (int)targetSize.Height, dpi);
        var imgBottom = SizeUtility.GetPixels(img.Bottom, img.DimensionUnit, (int)targetSize.Height, dpi);

        int width = img.Width > 0 ? imgWidth : rotatedImage.Width;
        int height = img.Height > 0 ? imgHeight : rotatedImage.Height;

        if (width > targetSize.Width)
        {
            width = (int)(targetSize.Width * 0.95);
        }
        if (height > targetSize.Height)
        {
            height = (int)(targetSize.Height * 0.95);
        }

        using var resizedImage = (width != rotatedImage.Width || height != rotatedImage.Height)
            ? SkiaUtility.Resize(rotatedImage, new SKSize(width, height), 100)
            : rotatedImage.Copy();

        // Position
        double left = 0;
        if (img.Position.HasFlag(ImagePosition.HCenter))
        {
            left = (targetSize.Width / 2) - (resizedImage.Width / 2f);
        }
        else if (img.Position.HasFlag(ImagePosition.Right))
        {
            left = targetSize.Width - resizedImage.Width - img.Margin;
        }
        else if (Math.Abs(left) < double.Epsilon && img.Right > 0)
        {
            left = targetSize.Width - resizedImage.Width - img.Margin;
        }
        else
        {
            left += img.Margin;
        }

        double top = 0;
        if (img.Position.HasFlag(ImagePosition.VCenter))
        {
            top = (targetSize.Height / 2) - (resizedImage.Height / 2f);
        }
        else if (img.Position.HasFlag(ImagePosition.Bottom))
        {
            top = targetSize.Height - resizedImage.Height - img.Margin;
        }
        else if (Math.Abs(top) < double.Epsilon && img.Bottom > 0)
        {
            top = targetSize.Height - resizedImage.Height - img.Margin;
        }
        else
        {
            top += img.Margin;
        }

        left += imgLeft - imgRight;
        top += imgTop - imgBottom;

        canvas.DrawBitmap(resizedImage, (float)left, (float)top);
    }
}
