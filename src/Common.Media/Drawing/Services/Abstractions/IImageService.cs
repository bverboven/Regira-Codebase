using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

public interface IImageService
{
    IImageFile? Parse(Stream stream);
    IImageFile? Parse(byte[] bytes);
    IImageFile? Parse(byte[] rawBytes, Size2D size, ImageFormat? format = null);
    IImageFile? Parse(IMemoryFile file);

    ImageFormat GetFormat(IImageFile input);
    IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat);

    IImageFile CropRectangle(IImageFile input, Position2D rect);
    Size2D GetDimensions(IImageFile input);
    IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100);
    IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100);
    IImageFile Rotate(IImageFile input, float angle, Color? background = null);

    IImageFile FlipHorizontal(IImageFile input);
    IImageFile FlipVertical(IImageFile input);

    Color GetPixelColor(IImageFile input, int x, int y);
    IImageFile MakeTransparent(IImageFile input, Color? color = null);
    IImageFile RemoveAlpha(IImageFile input);

    IImageFile Create(Size2D size, Color? backgroundColor = null, ImageFormat? format = null);
    IImageFile CreateTextImage(TextImageOptions? options = null);
    IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int? dpi = null);
}