using Regira.Dimensions;
using Regira.Drawing.SkiaSharp.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Utilities;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Services;

/// <summary>
/// Provides a set of methods for handling image processing tasks, such as parsing, resizing, cropping, rotating, 
/// flipping, and changing formats. This class also supports creating new images, drawing on images, and retrieving 
/// image properties like dimensions and pixel colors.
/// </summary>
/// <remarks>
/// This service is built on top of SkiaSharp and integrates with various abstractions and utilities for image manipulation.
/// It supports multiple image formats and provides functionality for both memory-based and stream-based image operations.
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

        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap == null)
        {
            return null;
        }

        var img = new ImageFile
        {
            Stream = stream,
            Size = new[] { bitmap.Width, bitmap.Height }
        };
        return img;
    }
    /// <inheritdoc/>
    public IImageFile? Parse(byte[]? bytes)
    {
        if (bytes == null)
        {
            return null;
        }

        using var ms = new MemoryStream(bytes);
        using var bitmap = SKBitmap.Decode(ms);
        if (bitmap == null)
        {
            return null;
        }

        var img = new ImageFile
        {
            Bytes = bytes,
            Size = new[] { bitmap.Width, bitmap.Height }
        };
        return img;
    }
    /// <inheritdoc/>
    public IImageFile? Parse(byte[] rawBytes, Size2D size, ImageFormat? format = null)
    {
        format ??= ImageFormat.Jpeg;

        using var skImg = SKImage.FromPixels(
            new SKImageInfo((int)size.Width, (int)size.Height, SKColorType.Bgra8888, SKAlphaType.Premul),
            SKData.CreateCopy(rawBytes)
        );

        using var skBitmap = SKBitmap.FromImage(skImg);
        if (skBitmap == null)
        {
            return null;
        }

        using var img = SkiaUtility.ChangeFormat(skBitmap, format.Value.ToSkiaFormat());
        return img.ToImageFile(format.Value.ToSkiaFormat());
    }
    /// <inheritdoc/>
    public IImageFile? Parse(IMemoryFile file) => file.HasStream() ? Parse(file.Stream) : Parse(file.GetBytes());

    /// <inheritdoc/>
    public ImageFormat GetFormat(IImageFile input)
    {
        if (input.Stream != null && input.Stream != Stream.Null)
        {
            return ConversionUtility.GetFormat(input.Stream).ToImageFormat();
        }

        var bytes = input.GetBytes();
        if (bytes == null)
        {
            throw new Exception("Could not get contents of image");
        }

        using var ms = new MemoryStream(bytes);
        return ConversionUtility.GetFormat(ms).ToImageFormat();
    }
    /// <inheritdoc/>
    public IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat)
    {
        var skiaFormat = targetFormat.ToSkiaFormat();
        using var convertedBitmap = SkiaUtility.ChangeFormat(input.ToBitmap(), skiaFormat);
        return convertedBitmap.ToImageFile(skiaFormat);
    }

    /// <inheritdoc/>
    public IImageFile CropRectangle(IImageFile input, Position2D rect)
    {
        var format = GetFormat(input);
        using var img = input.ToBitmap();
        var (topLeft, bottomRight) = DimensionsUtility.ToPoints(rect, new Size2D(img.Width, img.Height));
        var skRect = new SKRect((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X, (int)bottomRight.Y);
        using var croppedBitmap = SkiaUtility.CropRectangle(img, skRect);
        return croppedBitmap.ToImageFile(format.ToSkiaFormat());
    }

    /// <inheritdoc/>
    public Size2D GetDimensions(IImageFile input)
    {
        using var img = input.ToBitmap();
        return new Size2D(img.Width, img.Height);
    }
    /// <inheritdoc/>
    public IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 80)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var scaledBitmap = SkiaUtility.Resize(sourceBitmap, new SKSize(wantedSize.Width, wantedSize.Height), quality);
        return scaledBitmap.ToImageFile(format.ToSkiaFormat());
    }
    /// <inheritdoc/>
    public IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 80)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var scaledBitmap = SkiaUtility.ResizeFixed(sourceBitmap, new SKSize(size.Width, size.Height), quality);
        return scaledBitmap.ToImageFile(format.ToSkiaFormat());
    }

    /// <inheritdoc/>
    public IImageFile Rotate(IImageFile input, float angle, Color? background = null)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var rotatedBitmap = SkiaUtility.Rotate(sourceBitmap, angle, (background ?? Color.Transparent).ToSkiaColor());
        return rotatedBitmap.ToImageFile(format.ToSkiaFormat());
    }
    /// <inheritdoc/>
    public IImageFile FlipHorizontal(IImageFile input)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var flippedBitmap = SkiaUtility.FlipHorizontal(sourceBitmap);
        return flippedBitmap.ToImageFile(format.ToSkiaFormat());
    }
    /// <inheritdoc/>
    public IImageFile FlipVertical(IImageFile input)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var flippedBitmap = SkiaUtility.FlipVertical(sourceBitmap);
        return flippedBitmap.ToImageFile(format.ToSkiaFormat());
    }

    /// <inheritdoc/>
    public Color GetPixelColor(IImageFile input, int x, int y)
    {
        using var image = input.ToBitmap();
        return SkiaUtility.GetPixelColor(image, x, y).ToColor();
    }
    /// <inheritdoc/>
    public IImageFile MakeTransparent(IImageFile input, Color? color = null)
    {
        color ??= new Color(245, 245, 245);
        using var sourceBitmap = input.ToBitmap();
        var transparentBitmap = SkiaUtility.MakeTransparent(sourceBitmap, color.Value.ToSkiaColor());
        // return as PNG image
        return transparentBitmap.ToImageFile();
    }
    /// <inheritdoc/>
    public IImageFile MakeOpaque(IImageFile input)
    {
        using var inputBitmap = input.ToBitmap();

        var outputBitmap = new SKBitmap(inputBitmap.Width, inputBitmap.Height);
        using (var canvas = new SKCanvas(outputBitmap))
        {
            canvas.Clear(SKColors.White);
        }

        for (int x = 0; x < inputBitmap.Width; x++)
        {
            for (int y = 0; y < inputBitmap.Height; y++)
            {
                var pixelColor = inputBitmap.GetPixel(x, y);
                // Create a new color without the alpha value
                var newColor = SkiaUtility.IsPixelTransparent(pixelColor)
                    ? SKColors.White
                    : new SKColor(pixelColor.Red, pixelColor.Green, pixelColor.Blue, 255);
                outputBitmap.SetPixel(x, y, newColor);
            }
        }

        return outputBitmap.ToImageFile();
    }

    /// <inheritdoc/>
    public IImageFile Create(Size2D size, Color? backgroundColor = null, ImageFormat? format = null)
    {
        using var img = SkiaUtility.Create(size.ToSkiaSize(), (backgroundColor ?? ImageDefaults.BackgroundColor).ToSkiaColor());
        return img.ToImageFile((format ?? ImageDefaults.Format).ToSkiaFormat());
    }
    /// <inheritdoc/>
    public IImageFile CreateTextImage(LabelImageOptions? options = null)
    {
        using var img = SkiaUtility.CreateTextImage(options);
        return img.ToImageFile();
    }
    /// <inheritdoc/>
    public IImageFile Draw(IEnumerable<ImageLayer> imageLayers, IImageFile? target = null, int? dpi = null)
    {
        dpi ??= ImageLayerDefaults.Dpi;
        var imagesCollection = imageLayers.ToArray();
        using var targetImage = target?.ToBitmap() ?? DrawUtility.CreateSizedCanvas(imagesCollection);
        DrawUtility.Draw(imagesCollection, targetImage, dpi.Value);
        return targetImage.ToImageFile();
    }
}