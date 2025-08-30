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
        var size = new SKSize(
            (int)images.Max(x => x.Options?.Size?.Width ?? (x.Source.Size?.Width ?? 0)),
            (int)images.Max(x => x.Options?.Size?.Height ?? (x.Source.Size?.Height ?? 0))
        );
        return SkiaUtility.Create(size, ImageDefaults.BackgroundColor.ToSkiaColor());
    }

    public static void AddImage(ImageToAdd imageToAdd, SKCanvas canvas, Size2D targetSize, int? dpi = null)
    {
        var img = imageToAdd.Source;
        var options = imageToAdd.Options ?? new ImageToAddOptions();

        var source = img.ToBitmap();

        // Change opacity
        using var opacityImage = SkiaUtility.ChangeOpacity(source, options.Opacity);

        // Resize if needed
        using var resizedImage = options.Size is { Width: > 0, Height: > 0 }
            ? SkiaUtility.ResizeFixed(opacityImage, options.Size.Value.ToSkiaSize(), 100)
            : options.Size?.Width > 0 || options.Size?.Height > 0
                ? SkiaUtility.Resize(opacityImage, new SKSize(
                    SizeUtility.GetPixels(options.Size.Value.Width, options.DimensionUnit, (int)targetSize.Width, dpi),
                    SizeUtility.GetPixels(options.Size.Value.Height, options.DimensionUnit, (int)targetSize.Height, dpi)
                    ))
                : opacityImage;

        // Rotate if needed
        using var rotatedImage = Math.Abs(options.Rotation) > float.Epsilon
            ? SkiaUtility.Rotate(resizedImage, options.Rotation, SKColor.Empty)
            : resizedImage;

        // Position
        var inputPosition = options.Position ?? new Position2D();
        int? imgLeft = inputPosition.Left.HasValue ? SizeUtility.GetPixels(inputPosition.Left.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgRight = inputPosition.Right.HasValue ? SizeUtility.GetPixels(inputPosition.Right.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgTop = inputPosition.Top.HasValue ? SizeUtility.GetPixels(inputPosition.Top.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? SizeUtility.GetPixels(inputPosition.Bottom.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;

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
