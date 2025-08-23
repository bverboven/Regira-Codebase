using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;
using System.Drawing;
#pragma warning disable CA1416

namespace Regira.Drawing.GDI.Utilities;

public static class DrawUtility
{
    public static Image Draw(IEnumerable<ImageToAdd> imagesToAdd, Image? target = null, int dpi = ImageConstants.DEFAULT_DPI)
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
            (int)images.Max(x => x.Size?.Width ?? (x.Image.Size?.Width ?? 0)),
            (int)images.Max(x => x.Size?.Height ?? (x.Image.Size?.Height ?? 0))
        );
        return new Bitmap(size.Width, size.Height);
    }
    public static void AddImage(ImageToAdd img, Graphics g1, Size2D targetSize, int dpi = ImageConstants.DEFAULT_DPI)
    {
        Image source;
        if (img.Image.HasStream())
        {
            source = Image.FromStream(img.Image.Stream!);
        }
        else
        {
            using var ms = new MemoryStream(img.Image.GetBytes()!);
            source = Image.FromStream(ms);
        }


        using var newImg = GdiUtility.ChangeOpacity(source, img.Opacity);

        var inputSize = img.Size ?? new Size2D();
        var inputPosition = img.Position ?? new Position2D();
        var imgWidth = SizeUtility.GetPixels(inputSize.Width, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgHeight = SizeUtility.GetPixels(inputSize.Height, img.DimensionUnit, (int)targetSize.Height, dpi);
        int? imgLeft = inputPosition.Left.HasValue ? SizeUtility.GetPixels(inputPosition.Left.Value, img.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgRight = inputPosition.Right.HasValue ? SizeUtility.GetPixels(inputPosition.Right.Value, img.DimensionUnit, (int)targetSize.Width, dpi) : null;
        int? imgTop = inputPosition.Top.HasValue ? SizeUtility.GetPixels(inputPosition.Top.Value, img.DimensionUnit, (int)targetSize.Height, dpi) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? SizeUtility.GetPixels(inputPosition.Bottom.Value, img.DimensionUnit, (int)targetSize.Height, dpi) : null;

        //Dimensions
        var width = 0;
        if (inputSize.Width > 0)
        {
            width = imgWidth;
        }
        var height = 0;
        if (inputSize.Height > 0)
        {
            height = imgHeight;
        }

        if ((inputSize.Width > 0 ? inputSize.Width : newImg.Width) > targetSize.Width)
        {
            width = (int)(targetSize.Width * 0.95);
        }
        if ((inputSize.Height > 0 ? inputSize.Height : newImg.Height) > targetSize.Height)
        {
            height = (int)(targetSize.Height * 0.95);
        }

        // Resize?
        using var resizedImage = width != 0 && newImg.Width != width || height != 0 && newImg.Height != height ? GdiUtility.Resize(newImg, new Size(width, height)) : new Bitmap(newImg);

        // Rotate?
        using var rotatedImage = Math.Abs(img.Rotation) > double.Epsilon ? GdiUtility.Rotate(resizedImage, img.Rotation) : new Bitmap(resizedImage);

        // Position
        double left = 0;
        if (img.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = targetSize.Width / 2 - resizedImage.Width / 2f;
        }
        else if (img.PositionType.HasFlag(ImagePosition.Right))
        {
            left = targetSize.Width - resizedImage.Width - img.Margin;
        }
        else if (Math.Abs(left) < double.Epsilon && inputPosition.Right.HasValue)
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
            top = targetSize.Height / 2 - resizedImage.Height / 2f;
        }
        else if (img.PositionType.HasFlag(ImagePosition.Bottom))
        {
            top = targetSize.Height - resizedImage.Height - img.Margin;
        }
        else if (Math.Abs(top) < double.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - resizedImage.Height - img.Margin;
        }
        else
        {
            top += img.Margin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        g1.DrawImage(resizedImage, (int)left, (int)top);
    }
}