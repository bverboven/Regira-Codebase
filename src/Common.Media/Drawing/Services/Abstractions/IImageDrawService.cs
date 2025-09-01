using Regira.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides methods for creating and drawing images.
/// </summary>
public interface IImageDrawService
{
    /// <summary>
    /// Creates a new blank image with the specified size, background color, and format.
    /// </summary>
    /// <param name="size">The size of the image.</param>
    /// <param name="backgroundColor">The background color (optional).</param>
    /// <param name="format">The image format (optional).</param>
    /// <returns>The created image file.</returns>
    IImageFile Create(Size2D size, Color? backgroundColor = null, ImageFormat? format = null);
    /// <summary>
    /// Creates an image containing text, using the specified label options.
    /// </summary>
    /// <param name="options">The label image options (optional).</param>
    /// <returns>The created text image file.</returns>
    IImageFile CreateTextImage(LabelImageOptions? options = null);
    /// <summary>
    /// Draws multiple images onto a target image or a new canvas, with optional DPI.
    /// </summary>
    /// <param name="items">The collection of images to add.</param>
    /// <param name="target">The target image file (optional).</param>
    /// <param name="dpi">The DPI to use for drawing (optional, needed for conversion of dimension units).</param>
    /// <returns>The resulting image file with all images drawn.</returns>
    IImageFile Draw(IEnumerable<ImageToAdd> items, IImageFile? target = null, int? dpi = null);
}