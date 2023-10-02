using Regira.Dimensions;
using Regira.Drawing.Enums;
using Regira.IO.Abstractions;

namespace Regira.Drawing.Abstractions;

public interface IImageService
{
    IImageFile? Parse(Stream? stream);
    IImageFile? Parse(byte[]? bytes);
    IImageFile? Parse(IMemoryFile file);

    ImageFormat GetFormat(IImageFile input);
    IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat);

    IImageFile CropRectangle(IImageFile input, int[] rect);
    IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100);
    IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100);
    IImageFile Rotate(IImageFile input, double angle, string? background = null);

    IImageFile FlipHorizontal(IImageFile input);
    IImageFile FlipVertical(IImageFile input);

    IImageFile MakeTransparent(IImageFile input, int[]? rgb = null);
    IImageFile RemoveAlpha(IImageFile input);
}