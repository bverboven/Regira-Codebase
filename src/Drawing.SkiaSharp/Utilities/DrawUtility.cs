using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class DrawUtility
{
    public static SKBitmap Draw(IEnumerable<ImageToAdd> imagesToAdd, SKBitmap? target = null, int? dpi = null)
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
            (int)images.Max(x => x.Options?.Size?.Width ?? (x.Source.Size?.Width ?? 0)),
            (int)images.Max(x => x.Options?.Size?.Height ?? (x.Source.Size?.Height ?? 0))
        );
        return new SKBitmap(size.Width, size.Height);
    }

    public static void AddImage(ImageToAdd imageToAdd, SKCanvas canvas, Size2D targetSize, int? dpi = null)
    {
        var img = imageToAdd.Source;
        var options = imageToAdd.Options ?? new ImageToAddOptions();

        var source = img.ToBitmap();

        // Change opacity
        using var opacityImage = SkiaUtility.ChangeOpacity(source, options.Opacity);

        // Calculate target width/height
        var inputSize = options.Size ?? new Size2D();
        var inputPosition = options.Position ?? new Position2D();
        var imgWidth = SizeUtility.GetPixels(inputSize.Width, options.DimensionUnit, (int)targetSize.Width, dpi);
        var imgHeight = SizeUtility.GetPixels(inputSize.Height, options.DimensionUnit, (int)targetSize.Height, dpi);
        int? imgLeft = inputPosition.Left.HasValue ? SizeUtility.GetPixels(inputPosition.Left.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgRight = inputPosition.Right.HasValue ? SizeUtility.GetPixels(inputPosition.Right.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgTop = inputPosition.Top.HasValue ? SizeUtility.GetPixels(inputPosition.Top.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? SizeUtility.GetPixels(inputPosition.Bottom.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;

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
        using var rotatedImage = Math.Abs(options.Rotation) > float.Epsilon
            ? SkiaUtility.Rotate(resizedImage, options.Rotation, SKColor.Empty)
            : resizedImage.Copy();

        // Position
        float left = 0;
        if (options.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = (targetSize.Width / 2) - (resizedImage.Width / 2f);
        }
        else if (options.PositionType.HasFlag(ImagePosition.Right) || Math.Abs(left) < float.Epsilon && inputPosition.Right.HasValue)
        {
            left = targetSize.Width - resizedImage.Width - options.Margin;
        }
        else
        {
            left += options.Margin;
        }

        float top = 0;
        if (options.PositionType.HasFlag(ImagePosition.VCenter))
        {
            top = (targetSize.Height / 2) - (resizedImage.Height / 2f);
        }
        else if (options.PositionType.HasFlag(ImagePosition.Bottom) || Math.Abs(top) < float.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - resizedImage.Height - options.Margin;
        }
        else
        {
            top += options.Margin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        canvas.DrawBitmap(resizedImage, left, top);
    }
}
