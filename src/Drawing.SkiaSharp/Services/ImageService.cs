﻿using Regira.Dimensions;
using Regira.Drawing.SkiaSharp.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Enums;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Services;

public class ImageService : IImageService
{
    /// <summary>
    /// Creates an IImageFile with its content and size
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>Image with Stream, Width &amp; Height</returns>
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
    /// <summary>
    /// Creates an IImageFile with its content and size
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>Image with Bytes, Width &amp; Height</returns>
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
    public IImageFile? Parse(IMemoryFile file) => file.HasStream() ? Parse(file.Stream) : Parse(file.GetBytes());

    public ImageFormat GetFormat(IImageFile input)
    {
        if (input.Stream != null && input.Stream != Stream.Null)
        {
            return SkiaUtility.GetFormat(input.Stream).ToImageFormat();
        }

        var bytes = input.GetBytes();
        if (bytes == null)
        {
            throw new Exception("Could not get contents of image");
        }

        using var ms = new MemoryStream(bytes);
        return SkiaUtility.GetFormat(ms).ToImageFormat();
    }
    public IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat)
    {
        var skiaFormat = targetFormat.ToSkiaFormat();
        using var convertedBitmap = SkiaUtility.ChangeFormat(input.ToBitmap(), skiaFormat);
        return convertedBitmap.ToImageFile(skiaFormat);
    }

    public IImageFile CropRectangle(IImageFile input, int[] rect)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var croppedBitmap = SkiaUtility.CropRectangle(sourceBitmap, new SKRect(rect[0], rect[1], rect[0] + rect[2], rect[1] + rect[3]));
        return croppedBitmap.ToImageFile(format.ToSkiaFormat());
    }

    public IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 80)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var scaledBitmap = SkiaUtility.Resize(sourceBitmap, new SKSize(wantedSize.Width, wantedSize.Height), quality);
        return scaledBitmap.ToImageFile(format.ToSkiaFormat());
    }
    public IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 80)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var scaledBitmap = SkiaUtility.ResizeFixed(sourceBitmap, new SKSize(size.Width, size.Height), quality);
        return scaledBitmap.ToImageFile(format.ToSkiaFormat());
    }

    public IImageFile Rotate(IImageFile input, double angle, string? background = null)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var rotatedBitmap = SkiaUtility.Rotate(sourceBitmap, angle, background);
        return rotatedBitmap.ToImageFile(format.ToSkiaFormat());
    }

    public IImageFile FlipHorizontal(IImageFile input)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var flippedBitmap = SkiaUtility.FlipHorizontal(sourceBitmap);
        return flippedBitmap.ToImageFile(format.ToSkiaFormat());
    }
    public IImageFile FlipVertical(IImageFile input)
    {
        var format = GetFormat(input);
        using var sourceBitmap = input.ToBitmap();
        using var flippedBitmap = SkiaUtility.FlipVertical(sourceBitmap);
        return flippedBitmap.ToImageFile(format.ToSkiaFormat());
    }

    public IImageFile MakeTransparent(IImageFile input, int[]? rgb = null)
    {
        using var sourceBitmap = input.ToBitmap();
        var transparentBitmap = SkiaUtility.MakeTransparent(sourceBitmap, rgb);
        // return as PNG image
        return transparentBitmap.ToImageFile();
    }
    public IImageFile RemoveAlpha(IImageFile input)
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
}