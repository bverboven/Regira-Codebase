using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Utilities;

namespace Regira.Media.Drawing.Utilities;

public static class DrawImageUtility
{
    /// <summary>
    /// Creates an <see cref="IImageFile"/> instance using the first <see cref="IImageCreator"/> 
    /// in the provided <paramref name="services"/> collection that can handle the specified <paramref name="input"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the input object used to create the <see cref="IImageFile"/>. Must be a reference type.
    /// </typeparam>
    /// <param name="services">
    /// A collection of <see cref="IImageCreator"/> instances used to create the <see cref="IImageFile"/>.
    /// </param>
    /// <param name="input">
    /// The input object of type <typeparamref name="T"/> used to create the <see cref="IImageFile"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IImageFile"/> instance if a suitable <see cref="IImageCreator"/> is found and creation succeeds; otherwise, <c>null</c>.
    /// </returns>
    public static IImageFile? Create<T>(this IEnumerable<IImageCreator> services, T input)
        where T : class
        => services.FirstOrDefault(s => s.CanCreate(input))?.Create(input);
    /// <summary>
    /// Retrieves an <see cref="IImageFile"/> instance from the provided <paramref name="item"/> or creates one using the available <paramref name="services"/>.
    /// </summary>
    /// <param name="services">
    /// A collection of <see cref="IImageCreator"/> instances used to create an <see cref="IImageFile"/> if it cannot be directly retrieved from the <paramref name="item"/>.
    /// </param>
    /// <param name="item">
    /// The <see cref="IImageLayer"/> instance containing the source object to retrieve or create the <see cref="IImageFile"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IImageFile"/> instance if successfully retrieved or created; otherwise, <c>null</c>.
    /// </returns>
    public static IImageFile? GetImageFile(this IEnumerable<IImageCreator> services, IImageLayer item)
        => item.Source as IImageFile ?? services.Create(item.Source);

    /// <summary>
    /// Calculates the coordinates for positioning an image within a target area based on the provided options, target size, image size, and DPI.
    /// </summary>
    /// <param name="options">
    /// The options specifying how the image should be positioned, including margins, alignment, and dimension units.
    /// </param>
    /// <param name="targetSize">
    /// The size of the target area where the image will be placed.
    /// </param>
    /// <param name="imageSize">
    /// The size of the image to be positioned.
    /// </param>
    /// <param name="dpi">
    /// The dots per inch (DPI) value used for scaling dimensions. If null, a default DPI value is used.
    /// </param>
    /// <returns>
    /// A <see cref="Point2D"/> representing the calculated X and Y coordinates for the image's position.
    /// </returns>
    public static Point2D GetCoordinate(ImageLayerOptions options, Size2D targetSize, Size2D imageSize, int? dpi)
    {
        dpi ??= DrawImageDefaults.Dpi;

        var inputPosition = options.Position ?? new Position2D();
        int? imgLeft = inputPosition.Left.HasValue ? DimensionsUtility.GetPixels(inputPosition.Left.Value, options.DimensionUnit, (int)targetSize.Width, dpi.Value) : null;
        int? imgRight = inputPosition.Right.HasValue ? DimensionsUtility.GetPixels(inputPosition.Right.Value, options.DimensionUnit, (int)targetSize.Width, dpi.Value) : null;
        int? imgTop = inputPosition.Top.HasValue ? DimensionsUtility.GetPixels(inputPosition.Top.Value, options.DimensionUnit, (int)targetSize.Height, dpi.Value) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? DimensionsUtility.GetPixels(inputPosition.Bottom.Value, options.DimensionUnit, (int)targetSize.Height, dpi.Value) : null;
        int imgMargin = options.Margin > 0 ? DimensionsUtility.GetPixels(options.Margin, options.DimensionUnit, (int)Math.Max(targetSize.Width, targetSize.Height), dpi.Value) : 0;

        float left = 0;
        if (options.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = targetSize.Width / 2 - imageSize.Width / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Right) || Math.Abs(left) < float.Epsilon && inputPosition.Right.HasValue)
        {
            left = targetSize.Width - imageSize.Width - imgMargin;
        }
        else
        {
            left += imgMargin;
        }

        float top = 0;
        if (options.PositionType.HasFlag(ImagePosition.VCenter))
        {
            top = targetSize.Height / 2 - imageSize.Height / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Bottom) || Math.Abs(top) < float.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - imageSize.Height - imgMargin;
        }
        else
        {
            top += imgMargin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        return new[] { left, top };
    }
}