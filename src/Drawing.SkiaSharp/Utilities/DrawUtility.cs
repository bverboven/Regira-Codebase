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
            (int)images.Max(x => x.Size?.Width ?? (x.Image.Size?.Width ?? 0)),
            (int)images.Max(x => x.Size?.Height ?? (x.Image.Size?.Height ?? 0))
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

        // Calculate target width/height
        var inputSize = img.Size ?? new Size2D();
        var inputPosition = img.Position ?? new Position2D();
        var imgWidth = SizeUtility.GetPixels(inputSize.Width, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgHeight = SizeUtility.GetPixels(inputSize.Height, img.DimensionUnit, (int)targetSize.Height, dpi);
        int? imgLeft = inputPosition.Left.HasValue ? SizeUtility.GetPixels(inputPosition.Left.Value, img.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgRight = inputPosition.Right.HasValue ? SizeUtility.GetPixels(inputPosition.Right.Value, img.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgTop = inputPosition.Top.HasValue ? SizeUtility.GetPixels(inputPosition.Top.Value, img.DimensionUnit, (int)targetSize.Height, dpi) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? SizeUtility.GetPixels(inputPosition.Bottom.Value, img.DimensionUnit, (int)targetSize.Height, dpi) : null;

        int width = inputSize.Width > 0 ? imgWidth : opacityImage.Width;
        int height = inputSize.Height > 0 ? imgHeight : opacityImage.Height;

        if (width > targetSize.Width)
        {
            width = (int)(targetSize.Width * 0.95);
        }
        if (height > targetSize.Height)
        {
            height = (int)(targetSize.Height * 0.95);
        }

        // Resize if needed
        using var resizedImage = width != opacityImage.Width || height != opacityImage.Height
            ? SkiaUtility.Resize(opacityImage, new SKSize(width, height), 100)
            : opacityImage.Copy();

        // Rotate if needed
        using var rotatedImage = Math.Abs(img.Rotation) > double.Epsilon
            ? SkiaUtility.Rotate(resizedImage, (float)img.Rotation)
            : resizedImage.Copy();

        // Position
        double left = 0;
        if (img.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = (targetSize.Width / 2) - (resizedImage.Width / 2f);
        }
        else if (img.PositionType.HasFlag(ImagePosition.Right) || Math.Abs(left) < double.Epsilon && inputPosition.Right.HasValue)
        {
            left = targetSize.Width - resizedImage.Width - img.Margin;
        }
        else
        {
            left += img.Margin;
        }

        double top = 0;
        if (img.PositionType.HasFlag(ImagePosition.VCenter))
        {
            top = (targetSize.Height / 2) - (resizedImage.Height / 2f);
        }
        else if (img.PositionType.HasFlag(ImagePosition.Bottom) || Math.Abs(top) < double.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - resizedImage.Height - img.Margin;
        }
        else
        {
            top += img.Margin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        canvas.DrawBitmap(resizedImage, (float)left, (float)top);
    }
}
