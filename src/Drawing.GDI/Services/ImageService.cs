using Regira.Dimensions;
using Regira.Drawing.GDI.Helpers;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using System.Drawing;
using Color = Regira.Media.Drawing.Models.Color;

namespace Regira.Drawing.GDI.Services;

public class ImageService : IImageService
{
    public IImageFile? Parse(Stream? stream)
    {
        if (stream == null)
        {
            return null;
        }

        using var img = Image.FromStream(stream);
        return img.ToImageFile(img.RawFormat);
    }
    public IImageFile? Parse(byte[]? bytes)
    {
        if (bytes == null)
        {
            return null;
        }

        using var stream = new MemoryStream(bytes);
        return Parse(stream);
    }
    public IImageFile? Parse(IMemoryFile file) => file.HasStream() ? Parse(file.Stream) : Parse(file.GetBytes());

    public ImageFormat GetFormat(IImageFile input)
    {
        using var img = input.ToBitmap();
        return img.RawFormat.ToImageFormat();
    }
    public IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat)
    {
        using var img = input.ToBitmap();
        using var convertedImg = GdiUtility.ChangeFormat(img, targetFormat.ToGdiImageFormat());
        return convertedImg.ToImageFile(targetFormat.ToGdiImageFormat());
    }

    public IImageFile CropRectangle(IImageFile input, int[] rect)
    {
        using var img = input.ToBitmap();
        using var cropped = GdiUtility.CropRectangle(img, new Rectangle(rect[0], rect[1], rect[2], rect[3]));
        return cropped.ToImageFile(img.RawFormat);
    }

    public Size2D GetDimensions(IImageFile input)
    {
        using var img = input.ToBitmap();
        return new Size2D(img.Width, img.Height);
    }
    public IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100)
    {
        using var img = input.ToBitmap();
        using var resized = GdiUtility.Resize(img, new Size((int)wantedSize.Width, (int)wantedSize.Height));
        return resized.ToImageFile(img.RawFormat);
    }
    public IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100)
    {
        using var img = input.ToBitmap();
        using var resized = GdiUtility.ResizeFixed(img, new Size((int)size.Width, (int)size.Height));
        return resized.ToImageFile(img.RawFormat);
    }

    public IImageFile Rotate(IImageFile input, double angle, string? background = null)
    {
        using var img = input.ToBitmap();
        using var rotated = GdiUtility.Rotate(img, angle, background);
        return rotated.ToImageFile(img.RawFormat);
    }
    public IImageFile FlipHorizontal(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var flipped = GdiUtility.FlipHorizontal(img);
        return flipped.ToImageFile(img.RawFormat);
    }
    public IImageFile FlipVertical(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var flipped = GdiUtility.FlipVertical(img);
        return flipped.ToImageFile(img.RawFormat);
    }

    public Color GetPixelColor(IImageFile input, int x, int y)
    {
        using var img = input.ToBitmap();
        return GdiUtility.GetPixelColor(img, x, y).ToColor();
    }
    public IImageFile MakeTransparent(IImageFile input, int[]? rgb = null)
    {
        using var img = input.ToBitmap();
        using var target = GdiUtility.MakeTransparent(img, rgb);
        return target.ToImageFile(System.Drawing.Imaging.ImageFormat.Png);
    }
    /// <summary>
    /// Removes alpha value from pixels
    /// </summary>
    /// <param name="input"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public IImageFile RemoveAlpha(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var target = GdiUtility.ChangeOpacity(img, 1);
        return target.ToImageFile(img.RawFormat);
    }

    public IImageFile Create(int width, int height, Color? backgroundColor = null, ImageFormat? format = null)
    {
        using var img = GdiUtility.Create(width, height, backgroundColor?.ToGdiColor(), format?.ToGdiImageFormat());
        return img.ToImageFile(img.RawFormat);
    }
    public IImageFile CreateTextImage(string input, TextImageOptions? options = null)
    {
        using var img = GdiUtility.CreateTextImage(input, options);
        return img.ToImageFile(img.RawFormat);
    }
    public IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int dpi = 150)
    {
        var imagesCollection = imagesToAdd.ToArray();
        using var targetImage = target?.ToBitmap() ?? DrawUtility.CreateSizedCanvas(imagesCollection);
        DrawUtility.Draw(imagesCollection, targetImage, dpi);
        return targetImage.ToImageFile(targetImage.RawFormat);
    }
}