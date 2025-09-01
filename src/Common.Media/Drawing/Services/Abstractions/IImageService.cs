using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

/// <summary>
/// Provides an abstraction for image processing services, including parsing, format conversion,
/// cropping, resizing, rotating, flipping, color manipulation, and drawing operations.
/// </summary>
public interface IImageService
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
    IImageFile? Parse(byte[] rawBytes, Size2D size, ImageFormat? format = null);
    /// <summary>
    /// Parses an image from an <see cref="IMemoryFile"/> by using its stream or byte content.
    /// </summary>
    /// <param name="file">The memory file containing image data.</param>
    /// <returns>The parsed image file, or null if parsing fails.</returns>
    IImageFile? Parse(IMemoryFile file);

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

    /// <summary>
    /// Crops the input image to the specified rectangle.
    /// </summary>
    /// <param name="input">The source image file.</param>
    /// <param name="rect">The rectangle to crop, specified as a <see cref="Position2D"/>.</param>
    /// <returns>The cropped image file.</returns>
    IImageFile CropRectangle(IImageFile input, Position2D rect);

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
    /// <param name="imagesToAdd">The collection of images to add.</param>
    /// <param name="target">The target image file (optional).</param>
    /// <param name="dpi">The DPI to use for drawing (optional).</param>
    /// <returns>The resulting image file with all images drawn.</returns>
    IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int? dpi = null);
}