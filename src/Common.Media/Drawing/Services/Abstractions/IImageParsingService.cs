using Regira.IO.Abstractions;
using Regira.Media.Drawing.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides methods for parsing images from various sources.
/// </summary>
public interface IImageParsingService
{
    /// <summary>
    /// Parses an image from a stream and returns an <see cref="IImageFile"/> representation.
    /// Returns null if the stream is null or cannot be decoded.
    /// </summary>
    /// <param name="stream">The input stream containing image data.</param>
    /// <returns>The parsed image file, or null if parsing fails.</returns>
    IImageFile? Parse(Stream stream);
    /// <summary>
    /// Parses an image from a byte array and returns an <see cref="IImageFile"/> representation.
    /// Returns null if the byte array is null or cannot be decoded.
    /// </summary>
    /// <param name="bytes">The input byte array containing image data.</param>
    /// <returns>The parsed image file, or null if parsing fails.</returns>
    IImageFile? Parse(byte[] bytes);
    /// <summary>
    /// Parses an image from raw pixel bytes, size, and optional format, returning an <see cref="IImageFile"/>.
    /// </summary>
    /// <param name="rawBytes">The raw pixel data.</param>
    /// <param name="size">The dimensions of the image.</param>
    /// <param name="format">The image format (optional, defaults to JPEG).</param>
    /// <returns>The parsed image file, or null if parsing fails.</returns>
    IImageFile? Parse(byte[] rawBytes, ImageSize size, ImageFormat? format = null);
    /// <summary>
    /// Parses an image from an <see cref="IMemoryFile"/> by using its stream or byte content.
    /// </summary>
    /// <param name="file">The memory file containing image data.</param>
    /// <returns>The parsed image file, or null if parsing fails.</returns>
    IImageFile? Parse(IMemoryFile file);
}