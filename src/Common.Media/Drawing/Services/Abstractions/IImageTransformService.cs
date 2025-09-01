using Regira.Dimensions;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides methods for image transformation (crop, resize, rotate, flip).
/// </summary>
public interface IImageTransformService
{
    /// <summary>
    /// Gets the dimensions (width and height) of the input image.
    /// </summary>
    /// <param name="input">The image file.</param>
    /// <returns>The size of the image as a <see cref="Size2D"/>.</returns>
    Size2D GetDimensions(IImageFile input);
    /// <summary>
    /// Resizes the input image to the specified size, with optional quality.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="wantedSize">The desired size.</param>
    /// <param name="quality">The quality of the resize operation (default 100).</param>
    /// <returns>The resized image file.</returns>
    IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100);
    /// <summary>
    /// Resizes the input image to the exact specified size, ignoring aspect ratio, with optional quality.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="size">The target size.</param>
    /// <param name="quality">The quality of the resize operation (default 100).</param>
    /// <returns>The resized image file.</returns>
    IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100);

    /// <summary>
    /// Crops the input image to the specified rectangle.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="rect">The rectangle to crop, specified as a <see cref="Position2D"/>.</param>
    /// <returns>The cropped image file.</returns>
    IImageFile CropRectangle(IImageFile input, Position2D rect);
    
    /// <summary>
    /// Rotates the input image by the specified angle, with an optional background color.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="angle">The rotation angle in degrees.</param>
    /// <param name="background">The background color to use for empty areas (optional).</param>
    /// <returns>The rotated image file.</returns>
    IImageFile Rotate(IImageFile input, float angle, Color? background = null);
    /// <summary>
    /// Flips the input image horizontally.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <returns>The horizontally flipped image file.</returns>
    IImageFile FlipHorizontal(IImageFile input);
    /// <summary>
    /// Flips the input image vertically.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <returns>The vertically flipped image file.</returns>
    IImageFile FlipVertical(IImageFile input);
}