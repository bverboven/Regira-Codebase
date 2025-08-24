using Regira.Dimensions;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Color = Regira.Media.Drawing.Models.Color;
using ImageFormat = Regira.Media.Drawing.Enums.ImageFormat;
#pragma warning disable CA1416

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
    public IImageFile Parse(byte[] rawBytes, Size2D size, ImageFormat? format = null)
    {
        using var bmp = new Bitmap((int)size.Width, (int)size.Height, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

        var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
        var pNative = bmpData.Scan0;

        Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
        bmp.UnlockBits(bmpData);
        return bmp.ToImageFile((format ?? ImageFormat.Jpeg).ToGdiImageFormat());
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

    public IImageFile CropRectangle(IImageFile input, Position2D rect)
    {
        using var img = input.ToBitmap();
        var gdiRectangle = new Rectangle(
            (int)rect.Left!,
            (int)rect.Top!,
            // Position2D expects Right and Bottom to be distances from the right/bottom edge
            (int)(img.Width - rect.Right! - rect.Left),
            (int)(img.Height - rect.Bottom! - rect.Top)
        );// int x, int y, int width, int height
        using var cropped = GdiUtility.CropRectangle(img, gdiRectangle);
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

    public IImageFile Rotate(IImageFile input, float angle, Color? background = null)
    {
        using var img = input.ToBitmap();
        using var rotated = GdiUtility.Rotate(img, angle, background?.ToGdiColor());
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
    public IImageFile MakeTransparent(IImageFile input, Color? color = null)
    {
        color ??= new Color(245, 245, 245);
        using var img = input.ToBitmap();
        using var target = GdiUtility.MakeTransparent(img, color.Value.ToGdiColor());
        return target.ToImageFile(System.Drawing.Imaging.ImageFormat.Png);
    }

    /// <summary>
    /// Removes alpha value from pixels
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public IImageFile RemoveAlpha(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var target = GdiUtility.ChangeOpacity(img, 1);
        return target.ToImageFile(img.RawFormat);
    }

    public IImageFile Create(Size2D size, Color? backgroundColor = null, ImageFormat? format = null)
    {
        using var img = GdiUtility.Create(size.ToGdiSize(), (backgroundColor ?? ImageDefaults.BackgroundColor).ToGdiColor(), (format ?? ImageDefaults.Format).ToGdiImageFormat());
        return img.ToImageFile(img.RawFormat);
    }
    public IImageFile CreateTextImage(string input, TextImageOptions? options = null)
    {
        using var img = GdiUtility.CreateTextImage(input, options);
        return img.ToImageFile(img.RawFormat);
    }
    public IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int? dpi = null)
    {
        dpi ??= PrintDefaults.Dpi;
        var imagesCollection = imagesToAdd.ToArray();
        using var targetImage = target?.ToBitmap() ?? DrawUtility.CreateSizedCanvas(imagesCollection);
        DrawUtility.Draw(imagesCollection, targetImage, dpi.Value);
        return targetImage.ToImageFile(targetImage.RawFormat);
    }
}