using Regira.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;
using System.Drawing;

#pragma warning disable CA1416

namespace Regira.Drawing.GDI.Utilities;

public static class DrawUtility
{
    public static Image Draw(IEnumerable<ImageToAdd> imagesToAdd, Image? target = null, int? dpi = null)
    {
        var images = imagesToAdd.AsList();
        target ??= CreateSizedCanvas(images);

        if (!images.Any())
        {
            return target;
        }

        using var g = GdiUtility.GetGraphics(target);
        foreach (var img in images)
        {
            AddImage(img, g, new Size2D(target.Width, target.Height), dpi);
        }

        return target;
    }

    public static Image CreateSizedCanvas(IEnumerable<ImageToAdd> imagesToAdd)
    {
        var images = imagesToAdd.AsList();
        var size = new Size(
            (int)images.Max(x => x.Options?.Size?.Width ?? (x.Source.Size?.Width ?? 0)),
            (int)images.Max(x => x.Options?.Size?.Height ?? (x.Source.Size?.Height ?? 0))
        );
        return GdiUtility.Create(size);
    }
    public static void AddImage(ImageToAdd imageToAdd, Graphics g1, Size2D targetSize, int? dpi = null)
    {
        var img = imageToAdd.Source;
        var options = imageToAdd.Options ?? new ImageToAddOptions();

        var source = img.ToBitmap();

        // Change opacity
        using var opacityImage = GdiUtility.ChangeOpacity(source, options.Opacity);
        
        // Resize if needed
        using var resizedImage = options.Size is { Width: > 0, Height: > 0 }
            ? GdiUtility.ResizeFixed(opacityImage, options.Size.Value.ToGdiSize(), 100)
            : options.Size?.Width > 0 || options.Size?.Height > 0
                ? GdiUtility.Resize(opacityImage, new Size(
                    SizeUtility.GetPixels(options.Size.Value.Width, options.DimensionUnit, (int)targetSize.Width, dpi),
                    SizeUtility.GetPixels(options.Size.Value.Height, options.DimensionUnit, (int)targetSize.Height, dpi)
                ))
                : opacityImage;

        // Rotate?
        using var rotatedImage = Math.Abs(options.Rotation) > float.Epsilon ? GdiUtility.Rotate(resizedImage, options.Rotation, null) : new Bitmap(resizedImage);

        // Position
        var inputPosition = options.Position ?? new Position2D();
        int? imgLeft = inputPosition.Left.HasValue ? SizeUtility.GetPixels(inputPosition.Left.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgRight = inputPosition.Right.HasValue ? SizeUtility.GetPixels(inputPosition.Right.Value, options.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgTop = inputPosition.Top.HasValue ? SizeUtility.GetPixels(inputPosition.Top.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? SizeUtility.GetPixels(inputPosition.Bottom.Value, options.DimensionUnit, (int)targetSize.Height, dpi) : null;
        
        float left = 0;
        if (options.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = targetSize.Width / 2 - resizedImage.Width / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Right))
        {
            left = targetSize.Width - resizedImage.Width - options.Margin;
        }
        else if (Math.Abs(left) < float.Epsilon && inputPosition.Right.HasValue)
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
            top = targetSize.Height / 2 - resizedImage.Height / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Bottom))
        {
            top = targetSize.Height - resizedImage.Height - options.Margin;
        }
        else if (Math.Abs(top) < float.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - resizedImage.Height - options.Margin;
        }
        else
        {
            top += options.Margin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        g1.DrawImage(resizedImage, (int)left, (int)top);
    }
}