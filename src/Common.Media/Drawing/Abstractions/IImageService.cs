using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;

namespace Regira.Media.Drawing.Abstractions;

public interface IImageService
{
    IImageFile? Parse(Stream? stream);
    IImageFile? Parse(byte[]? bytes);
    IImageFile? Parse(IMemoryFile file);

    ImageFormat GetFormat(IImageFile input);
    IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat);

    IImageFile CropRectangle(IImageFile input, int[] rect);
    Size2D GetDimensions(IImageFile input);
    IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100);
    IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100);
    IImageFile Rotate(IImageFile input, double angle, string? background = null);

    IImageFile FlipHorizontal(IImageFile input);
    IImageFile FlipVertical(IImageFile input);

    Color GetPixelColor(IImageFile input, int x, int y);
    IImageFile MakeTransparent(IImageFile input, int[]? rgb = null);
    IImageFile RemoveAlpha(IImageFile input);

    IImageFile Create(int width, int height, Color? backgroundColor = null, ImageFormat? format = null);
    IImageFile CreateTextImage(string input, TextImageOptions? options = null);
    IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int dpi = ImageConstants.DEFAULT_DPI);
}