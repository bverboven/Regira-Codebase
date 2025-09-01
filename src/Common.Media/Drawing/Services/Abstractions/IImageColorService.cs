using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides methods for pixel color manipulation and transparency.
/// </summary>
public interface IImageColorService
{
    /// <summary>
    /// Gets the color of the pixel at the specified coordinates in the input image.
    /// </summary>
    /// <param name="input">The image file.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The color of the specified pixel.</returns>
    Color GetPixelColor(IImageFile input, int x, int y);
    /// <summary>
    /// Makes the specified color in the input image transparent.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="color">The color to make transparent (optional, defaults to light gray).</param>
    /// <returns>The image file with transparency applied.</returns>
    IImageFile MakeTransparent(IImageFile input, Color? color = null);
    /// <summary>
    /// Removes the alpha channel from the input image, replacing transparent pixels with a solid color (typically white).
    /// </summary>
    /// <param name="input">The source image file to process.</param>
    /// <returns>The image file with alpha transparency removed.</returns>
    IImageFile MakeOpaque(IImageFile input);
}