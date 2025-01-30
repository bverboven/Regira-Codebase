using System.Drawing;
using Regira.Dimensions;
using Regira.Drawing.GDI.Abstractions;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;

namespace Regira.Drawing.GDI.Helpers;

internal class DrawHelper : IImageHelper
{
    public Image Draw(IEnumerable<ImageToAdd> imagesToAdd, Image? target = null, int dpi = ImageConstants.DEFAULT_DPI)
    {
        var images = imagesToAdd.AsList();
        if (target == null)
        {
            target = CalculateTarget(images);
        }
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

    public Image CalculateTarget(IEnumerable<ImageToAdd> imagesToAdd)
    {
        var images = imagesToAdd.AsList();
        var size = new Size(
            (int)images.Max(x => x.Width > 0 ? x.Width : x.Image?.Size?.Width ?? Image.FromFile(x.Path!).Width),
            (int)images.Max(x => x.Height > 0 ? x.Height : x.Image?.Size?.Height ?? Image.FromFile(x.Path!).Height)
        );
        return new Bitmap(size.Width, size.Height);
    }
    public void AddImage(ImageToAdd img, Graphics g1, Size2D targetSize, int dpi = ImageConstants.DEFAULT_DPI)
    {
        Image source;
        if (img.Image?.HasStream() == true)
        {
            source = Image.FromStream(img.Image.Stream!);
        }
        else
        {
            using var ms = new MemoryStream(img.Image?.GetBytes()!);
            source = Image.FromStream(ms);
        }

        using var newImg = GdiUtility.ChangeOpacity(source, img.Opacity);
        using var rotatedImage = Math.Abs(img.Rotation) > double.Epsilon ? GdiUtility.Rotate(newImg, img.Rotation) : new Bitmap(newImg);
        var imgWidth = SizeUtility.GetPixels(img.Width, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgHeight = SizeUtility.GetPixels(img.Height, img.DimensionUnit, (int)targetSize.Height, dpi);
        var imgLeft = SizeUtility.GetPixels(img.Left, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgRight = SizeUtility.GetPixels(img.Right, img.DimensionUnit, (int)targetSize.Width, dpi);
        var imgTop = SizeUtility.GetPixels(img.Top, img.DimensionUnit, (int)targetSize.Height, dpi);
        var imgBottom = SizeUtility.GetPixels(img.Bottom, img.DimensionUnit, (int)targetSize.Height, dpi);

        //Dimensions
        var width = 0;
        if (img.Width > 0)
        {
            width = imgWidth;
        }
        var height = 0;
        if (img.Height > 0)
        {
            height = imgHeight;
        }

        if ((img.Width > 0 ? img.Width : rotatedImage.Width) > targetSize.Width)
        {
            width = (int)(targetSize.Width * 0.95);
        }
        if ((img.Height > 0 ? img.Height : rotatedImage.Height) > targetSize.Height)
        {
            height = (int)(targetSize.Height * 0.95);
        }

        using var resizedImage = rotatedImage.Width != width || rotatedImage.Height != height ? GdiUtility.Resize(rotatedImage, new Size(width, height)) : new Bitmap(rotatedImage);
        //Position
        double left = 0;
        if (((int)img.Position & (int)ImagePosition.HCenter) == (int)ImagePosition.HCenter)
        {
            left = (targetSize.Width / 2) - (resizedImage.Width / 2f);
        }
        else if (((int)img.Position & (int)ImagePosition.Right) == (int)ImagePosition.Right)
        {
            left = targetSize.Width - resizedImage.Width;
        }
        else if (Math.Abs(left) < double.Epsilon && img.Right > 0)
        {
            left = targetSize.Width - resizedImage.Width;
        }

        double top = 0;
        if (((int)img.Position & (int)ImagePosition.VCenter) == (int)ImagePosition.VCenter)
        {
            top = (targetSize.Height / 2) - (resizedImage.Height / 2f);
        }
        else if (((int)img.Position & (int)ImagePosition.Bottom) == (int)ImagePosition.Bottom)
        {
            top = targetSize.Height - resizedImage.Height;
        }
        else if (Math.Abs(top) < double.Epsilon && img.Bottom > 0)
        {
            top = targetSize.Height - resizedImage.Height;
        }

        left += imgLeft - imgRight;
        top += imgTop - imgBottom;

        g1.DrawImage(resizedImage, (int)left, (int)top);
    }
}