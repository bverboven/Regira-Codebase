using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Dimensions;
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
    /// <returns>
    /// A <see cref="ImagePoint"/> representing the calculated X and Y coordinates for the image's imgOffset.
    /// </returns>
    public static ImagePoint GetCoordinate(ImageLayerOptions options, ImageSize targetSize, ImageSize imageSize)
    {
        var inputPosition = options.Offset ?? new ImageEdgeOffset();
        int? imgLeft = inputPosition.Left;
        int? imgRight = inputPosition.Right;
        int? imgTop = inputPosition.Top;
        int? imgBottom = inputPosition.Bottom;
        int imgMargin = options.Margin;

        int left = 0;
        if (options.Position.HasFlag(ImagePosition.HCenter))
        {
            left = (int)(targetSize.Width / 2f - imageSize.Width / 2f);
        }
        else if (options.Position.HasFlag(ImagePosition.Right) || (options.Position == ImagePosition.Absolute && inputPosition.Right.HasValue))
        {
            left = targetSize.Width - imageSize.Width - imgMargin;
        }
        else
        {
            left += imgMargin;
        }

        int top = 0;
        if (options.Position.HasFlag(ImagePosition.VCenter))
        {
            top = (int)(targetSize.Height / 2f - imageSize.Height / 2f);
        }
        else if (options.Position.HasFlag(ImagePosition.Bottom) || (options.Position == ImagePosition.Absolute && inputPosition.Bottom.HasValue))
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


    /// <summary>
    /// Calculates the size of a rectangular area defined by two points.
    /// </summary>
    /// <param name="topLeft">
    /// The top-left point of the rectangle.
    /// </param>
    /// <param name="bottomRight">
    /// The bottom-right point of the rectangle.
    /// </param>
    /// <returns>
    /// A <see cref="ImageSize"/> representing the width and height of the rectangle.
    /// </returns>
    public static ImageSize CalculateSize(ImagePoint topLeft, ImagePoint bottomRight)
        => new(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
    /// <summary>
    /// Calculates the scaled size of a source dimension to fit within a target dimension while maintaining the aspect ratio.
    /// </summary>
    /// <param name="source">The original size to be scaled, represented as a <see cref="ImageSize"/>.</param>
    /// <param name="target">
    /// The target size within which the source size should fit, represented as a <see cref="ImageSize"/>.
    /// If either width or height of the target is zero, the scaling will be based on the non-zero dimension.
    /// </param>
    /// <returns>
    /// A new <see cref="ImageSize"/> representing the scaled size of the source dimension that fits within the target dimension.
    /// </returns>
    public static ImageSize CalculateSize(ImageSize source, ImageSize target)
    {
        if (target is { Width: 0, Height: 0 } || source.Equals(target))
        {
            return new ImageSize(target.Width, target.Height);
        }

        double widthFactor = 1, heightFactor = 1, factor = 1;
        if (target.Width > 0)
        {
            widthFactor = (double)target.Width / source.Width;
            if (target.Height == 0)
            {
                factor = widthFactor;
            }
        }
        if (target.Height > 0)
        {
            heightFactor = (double)target.Height / source.Height;
            if (target.Width == 0)
            {
                factor = heightFactor;
            }
        }

        if (Math.Abs(factor - 1) < double.Epsilon)
        {
            factor = Math.Min(widthFactor, heightFactor);
        }

        return new ImageSize((int)(source.Width * factor), (int)(source.Height * factor));
    }

    /// <summary>
    /// Converts a <see cref="ImageEdgeOffset"/> to a pair of points representing the top-left and bottom-right corners
    /// within a given total size.
    /// </summary>
    /// <param name="imgOffset">
    /// The <see cref="ImageEdgeOffset"/> specifying the imgOffset with optional top, left, bottom, and right offsets.
    /// </param>
    /// <param name="totalSize">
    /// The total size as a <see cref="ImageSize"/> within which the points are calculated.
    /// </param>
    /// <returns>
    /// A tuple containing the top-left and bottom-right points as <see cref="ImagePoint"/>.
    /// </returns>
    public static (ImagePoint TopLeft, ImagePoint BottomRight) ToPoints(ImageEdgeOffset imgOffset, ImageSize totalSize)
    {
        var topLeft = new ImagePoint(imgOffset.Left ?? 0, imgOffset.Top ?? 0);
        var bottomRight = new ImagePoint(totalSize.Width, totalSize.Height) - new ImagePoint(imgOffset.Right ?? 0, imgOffset.Bottom ?? 0);
        return (topLeft, bottomRight);
    }
    /// <summary>
    /// Converts a given imgOffset and total size into a point and size representation.
    /// </summary>
    /// <param name="imgOffset">
    /// The imgOffset, defined by its top, left, bottom, and right boundaries.
    /// </param>
    /// <param name="totalSize">
    /// The total size of the area, used to calculate the resulting point and size.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="ImagePoint"/> representing the top-left point of the imgOffset.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ImageSize"/> representing the dimensions of the imgOffset.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    public static (ImagePoint TopLeft, ImageSize Size) ToPointSize(ImageEdgeOffset imgOffset, ImageSize totalSize)
    {
        var points = ToPoints(imgOffset, totalSize);
        var calculatedSize = CalculateSize(points.TopLeft, points.BottomRight);
        return (points.TopLeft, calculatedSize);
    }
    /// <summary>
    /// Converts the specified top-left and bottom-right points into a <see cref="ImageEdgeOffset"/> object
    /// relative to the given total size.
    /// </summary>
    /// <param name="topLeft">The top-left point of the imgOffset.</param>
    /// <param name="bottomRight">The bottom-right point of the imgOffset.</param>
    /// <param name="totalSize">The total size used to calculate the relative imgOffset.</param>
    /// <returns>
    /// A <see cref="ImageEdgeOffset"/> object representing the relative imgOffset defined by the given points
    /// and total size.
    /// </returns>
    public static ImageEdgeOffset ToPosition(ImagePoint topLeft, ImagePoint bottomRight, ImageSize totalSize)
    {
        var br = new ImagePoint(totalSize.Width, totalSize.Height) - new ImagePoint(bottomRight.X, bottomRight.Y);
        return new ImageEdgeOffset(topLeft.Y, topLeft.X, br.Y, br.X);
    }

    public static ImagePoint CalculatePoint(Point2D point, LengthUnit unit, ImageSize targetSize, int? dpi = null)
    {
        dpi ??= ImageLayerDefaults.Dpi;

        var x = DimensionsUtility.GetPixels(point.X, unit, targetSize.Width, dpi.Value);
        var y = DimensionsUtility.GetPixels(point.Y, unit, targetSize.Height, dpi.Value);

        return new ImagePoint(x, y);
    }
    public static ImageEdgeOffset CalculateEdgeOffset(Position2D offset, LengthUnit unit, ImageSize targetSize, int? dpi = null)
    {
        dpi ??= ImageLayerDefaults.Dpi;

        int? top = offset.Top.HasValue
            ? DimensionsUtility.GetPixels(offset.Top.Value, unit, targetSize.Height, dpi.Value)
            : null;
        int? left = offset.Left.HasValue
            ? DimensionsUtility.GetPixels(offset.Left.Value, unit, targetSize.Width, dpi.Value)
            : null;
        int? bottom = offset.Bottom.HasValue
            ? DimensionsUtility.GetPixels(offset.Bottom.Value, unit, targetSize.Height, dpi.Value)
            : null;
        int? right = offset.Right.HasValue
            ? DimensionsUtility.GetPixels(offset.Right.Value, unit, targetSize.Width, dpi.Value)
            : null;

        return new ImageEdgeOffset(top, left, bottom, right);
    }
    public static ImageSize? CalculateSize(Size2D? size, LengthUnit unit, ImageSize targetSize, int? dpi = null)
    {
        if (size is null or { Width: 0, Height: 0 })
        {
            return null;
        }

        dpi ??= ImageLayerDefaults.Dpi;

        var width = DimensionsUtility.GetPixels(size.Value.Width, unit, targetSize.Width, dpi.Value);
        var height = DimensionsUtility.GetPixels(size.Value.Height, unit, targetSize.Height, dpi.Value);

        return new ImageSize(width, height);
    }
}