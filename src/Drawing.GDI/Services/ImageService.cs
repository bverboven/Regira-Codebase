using Regira.Dimensions;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Utilities;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Color = Regira.Media.Drawing.Models.Color;
using ImageFormat = Regira.Media.Drawing.Enums.ImageFormat;
#pragma warning disable CA1416

namespace Regira.Drawing.GDI.Services;

/// <summary>
/// Provides a set of services for manipulating and processing images, including parsing, resizing, cropping, 
/// format conversion, and other image-related operations. This class implements the <see cref="IImageService"/> interface.
/// </summary>
/// <remarks>
/// This service supports various image formats defined in the <see cref="ImageFormat"/> enumeration and 
/// works with image files represented by the <see cref="IImageFile"/> interface. It also provides utility 
/// methods for creating, drawing, and modifying images.
/// </remarks>
public class ImageService : IImageService
{
    /// <inheritdoc/>
    public IImageFile? Parse(Stream? stream)
    {
        if (stream == null)
        {
            return null;
        }

        using var img = Image.FromStream(stream);
        return img.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile? Parse(byte[]? bytes)
    {
        if (bytes == null)
        {
            return null;
        }

        using var stream = new MemoryStream(bytes);
        return Parse(stream);
    }
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public IImageFile? Parse(IMemoryFile file) => file.HasStream() ? Parse(file.Stream) : Parse(file.GetBytes());

    /// <inheritdoc/>
    public ImageFormat GetFormat(IImageFile input)
    {
        using var img = input.ToBitmap();
        return img.RawFormat.ToImageFormat();
    }
    /// <inheritdoc/>
    public IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat)
    {
        using var img = input.ToBitmap();
        using var convertedImg = GdiUtility.ChangeFormat(img, targetFormat.ToGdiImageFormat());
        return convertedImg.ToImageFile(targetFormat.ToGdiImageFormat());
    }

    /// <inheritdoc/>
    public IImageFile CropRectangle(IImageFile input, Position2D rect)
    {
        using var img = input.ToBitmap();
        var (coordinate, size) = DimensionsUtility.ToPointSize(rect, new Size2D(img.Width, img.Height));
        var gdiRectangle = new Rectangle((int)coordinate.X, (int)coordinate.Y, (int)size.Width, (int)size.Height);
        using var cropped = GdiUtility.CropRectangle(img, gdiRectangle);
        return cropped.ToImageFile(img.RawFormat);
    }

    /// <inheritdoc/>
    public Size2D GetDimensions(IImageFile input)
    {
        using var img = input.ToBitmap();
        return new Size2D(img.Width, img.Height);
    }
    /// <inheritdoc/>
    public IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100)
    {
        using var img = input.ToBitmap();
        using var resized = GdiUtility.Resize(img, new Size((int)wantedSize.Width, (int)wantedSize.Height));
        return resized.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100)
    {
        using var img = input.ToBitmap();
        using var resized = GdiUtility.ResizeFixed(img, new Size((int)size.Width, (int)size.Height));
        return resized.ToImageFile(img.RawFormat);
    }

    /// <inheritdoc/>
    public IImageFile Rotate(IImageFile input, float angle, Color? background = null)
    {
        using var img = input.ToBitmap();
        using var rotated = GdiUtility.Rotate(img, angle, background?.ToGdiColor());
        return rotated.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile FlipHorizontal(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var flipped = GdiUtility.FlipHorizontal(img);
        return flipped.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile FlipVertical(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var flipped = GdiUtility.FlipVertical(img);
        return flipped.ToImageFile(img.RawFormat);
    }

    /// <inheritdoc/>
    public Color GetPixelColor(IImageFile input, int x, int y)
    {
        using var img = input.ToBitmap();
        return GdiUtility.GetPixelColor(img, x, y).ToColor();
    }
    /// <inheritdoc/>
    public IImageFile MakeTransparent(IImageFile input, Color? color = null)
    {
        color ??= new Color(245, 245, 245);
        using var img = input.ToBitmap();
        using var target = GdiUtility.MakeTransparent(img, color.Value.ToGdiColor());
        return target.ToImageFile(System.Drawing.Imaging.ImageFormat.Png);
    }
    /// <inheritdoc/>
    public IImageFile MakeOpaque(IImageFile input)
    {
        using var img = input.ToBitmap();
        using var target = GdiUtility.ChangeOpacity(img, 1);
        return target.ToImageFile(img.RawFormat);
    }

    /// <inheritdoc/>
    public IImageFile Create(Size2D size, Color? backgroundColor = null, ImageFormat? format = null)
    {
        using var img = GdiUtility.Create(size.ToGdiSize(), (backgroundColor ?? ImageDefaults.BackgroundColor).ToGdiColor(), (format ?? ImageDefaults.Format).ToGdiImageFormat());
        return img.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile CreateTextImage(LabelImageOptions? options = null)
    {
        using var img = GdiUtility.CreateTextImage(options);
        return img.ToImageFile(img.RawFormat);
    }
    /// <inheritdoc/>
    public IImageFile Draw(IEnumerable<ImageLayer> imageLayers, IImageFile? target = null, int? dpi = null)
    {
        dpi ??= DrawImageDefaults.Dpi;
        var imagesCollection = imageLayers.ToArray();
        using var targetImage = target?.ToBitmap() ?? DrawUtility.CreateSizedCanvas(imagesCollection);
        DrawUtility.Draw(imagesCollection, targetImage, dpi.Value);
        return targetImage.ToImageFile(targetImage.RawFormat);
    }
}
