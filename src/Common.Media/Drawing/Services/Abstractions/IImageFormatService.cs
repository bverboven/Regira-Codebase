using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides methods for image format detection and conversion.
/// </summary>
public interface IImageFormatService
{
    /// <summary>
    /// Determines the <see cref="ImageFormat"/> of the given <see cref="IImageFile"/>.
    /// </summary>
    /// <param name="input">The image file to inspect.</param>
    /// <returns>The detected image format.</returns>
    ImageFormat GetFormat(IImageFile input);
    /// <summary>
    /// Converts an image to a different format.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="targetFormat">The desired image format.</param>
    /// <returns>The converted image file.</returns>
    IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat);
}