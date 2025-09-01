using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;
using SkiaSharp;

namespace Regira.Drawing.SkiaSharp.Utilities;

public static class DrawUtility
{
    public static SKBitmap Draw(IEnumerable<ImageLayer> imageLayers, SKBitmap? target = null, int? dpi = null)
    {
        var images = imageLayers.ToList();
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

    public static SKBitmap CreateSizedCanvas(IEnumerable<ImageLayer> imageLayers)
    {
        var images = imageLayers.ToList();
        var size = new SKSize(
            (int)images.Max(x => x.Options?.Size?.Width ?? (x.Source.Size?.Width ?? 0)),
            (int)images.Max(x => x.Options?.Size?.Height ?? (x.Source.Size?.Height ?? 0))
        );
        return SkiaUtility.Create(size, ImageDefaults.BackgroundColor.ToSkiaColor());
    }

    public static void AddImage(ImageLayer imageLayer, SKCanvas canvas, Size2D targetSize, int? dpi = null)
    {
        dpi ??= DrawImageDefaults.Dpi;

        var img = imageLayer.Source;
        var options = imageLayer.Options ?? new ImageLayerOptions();

        var source = img.ToBitmap();

        // Change opacity
        using var opacityImage = SkiaUtility.ChangeOpacity(source, options.Opacity);

        // Resize if needed
        using var resizedImage = options.Size is { Width: > 0, Height: > 0 }
            ? SkiaUtility.ResizeFixed(opacityImage, options.Size.Value.ToSkiaSize(), 100)
            : options.Size?.Width > 0 || options.Size?.Height > 0
                ? SkiaUtility.Resize(opacityImage, new SKSize(
                    DimensionsUtility.GetPixels(options.Size.Value.Width, options.DimensionUnit, (int)targetSize.Width, dpi.Value),
                    DimensionsUtility.GetPixels(options.Size.Value.Height, options.DimensionUnit, (int)targetSize.Height, dpi.Value)
                    ))
                : opacityImage;

        // Rotate if needed
        using var rotatedImage = Math.Abs(options.Rotation) > float.Epsilon
            ? SkiaUtility.Rotate(resizedImage, options.Rotation, SKColor.Empty)
            : resizedImage;

        // Position
        var coordinate = DrawImageUtility.GetCoordinate(options, targetSize, new Size2D(resizedImage.Width, resizedImage.Height), dpi);

        canvas.DrawBitmap(resizedImage, coordinate.X, coordinate.Y);
    }
}
