using Regira.Dimensions;

namespace Regira.Utilities;

public static class DimensionsUtility
{
    /// <summary>
    /// Provides constants for common DPI (dots per inch) values used in display and printing contexts.
    /// </summary>
    public static class DPI
    {
        public const int WIN_PPI = 96;
        public const int MAC_PPI = 72;

        public const int DEFAULT = WIN_PPI;
    }

    public const float CM_PER_INCH = 2.54f;
    public const float MM_PER_INCH = CM_PER_INCH * 10;

    /// <summary>
    /// Converts a measurement from millimeters to inches.
    /// </summary>
    /// <param name="mm">The measurement in millimeters.</param>
    /// <returns>The equivalent measurement in inches.</returns>
    public static float MmToIn(float mm)
    {
        return mm / MM_PER_INCH;
    }
    /// <summary>
    /// Converts a two-dimensional measurement from millimeters to inches.
    /// </summary>
    /// <param name="mm">The two-dimensional measurement in millimeters, represented as a <see cref="Size2D"/>.</param>
    /// <returns>The equivalent two-dimensional measurement in inches, represented as a <see cref="Size2D"/>.</returns>
    public static Size2D MmToIn(Size2D mm)
    {
        return mm / MM_PER_INCH;
    }
    /// <summary>
    /// Converts a measurement from inches to millimeters.
    /// </summary>
    /// <param name="inches">The measurement in inches.</param>
    /// <returns>The equivalent measurement in millimeters.</returns>
    public static float InToMm(float inches)
    {
        return inches * MM_PER_INCH;
    }
    /// <summary>
    /// Converts a two-dimensional measurement from inches to millimeters.
    /// </summary>
    /// <param name="inches">
    /// The two-dimensional measurement in inches, represented as a <see cref="Size2D"/>.
    /// </param>
    /// <returns>
    /// The equivalent two-dimensional measurement in millimeters, represented as a <see cref="Size2D"/>.
    /// </returns>
    public static Size2D InToMm(Size2D inches)
    {
        return inches * MM_PER_INCH;
    }

    /// <summary>
    /// Converts a measurement from millimeters to points, considering the specified DPI (dots per inch).
    /// </summary>
    /// <param name="mm">The measurement in millimeters.</param>
    /// <param name="dpi">
    /// The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>The equivalent measurement in points.</returns>
    public static float MmToPt(float mm, int dpi = DPI.DEFAULT)
    {
        var inch = MmToIn(mm);
        return InToPt(inch, dpi);
    }
    /// <summary>
    /// Converts a two-dimensional measurement from millimeters to points, considering the specified DPI (dots per inch).
    /// </summary>
    /// <param name="mm">The two-dimensional measurement in millimeters, represented as a <see cref="Size2D"/>.</param>
    /// <param name="dpi">
    /// The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>
    /// The equivalent two-dimensional measurement in points, represented as a <see cref="Size2D"/>.
    /// </returns>
    public static Size2D MmToPt(Size2D mm, int dpi = DPI.DEFAULT)
    {
        var inch = MmToIn(mm);
        return InToPt(inch, dpi);
    }
    /// <summary>
    /// Converts a measurement from inches to points, based on the specified DPI (dots per inch).
    /// </summary>
    /// <param name="inches">The measurement in inches.</param>
    /// <param name="dpi">
    /// The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>The equivalent measurement in points.</returns>
    public static float InToPt(float inches, int dpi = DPI.DEFAULT)
    {
        return inches * dpi;
    }
    /// <summary>
    /// Converts a measurement from inches to points, considering the specified DPI (dots per inch).
    /// </summary>
    /// <param name="inches">The measurement in inches.</param>
    /// <param name="dpi">The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.</param>
    /// <returns>The equivalent measurement in points.</returns>
    public static Size2D InToPt(Size2D inches, int dpi = DPI.DEFAULT)
    {
        return inches * dpi;
    }

    /// <summary>
    /// Converts a measurement from points to millimeters, considering the specified DPI (dots per inch).
    /// </summary>
    /// <param name="points">The measurement in points.</param>
    /// <param name="dpi">
    /// The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>The equivalent measurement in millimeters.</returns>
    public static float PtToMm(float points, int dpi = DPI.DEFAULT)
    {
        var inch = PtToIn(points, dpi);
        return InToMm(inch);
    }
    /// <summary>
    /// Converts a two-dimensional measurement from points to millimeters.
    /// </summary>
    /// <param name="points">
    /// The two-dimensional measurement in points, represented as a <see cref="Size2D"/>.
    /// </param>
    /// <param name="dpi">
    /// The dots per inch (DPI) value used for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>
    /// The equivalent two-dimensional measurement in millimeters, represented as a <see cref="Size2D"/>.
    /// </returns>
    public static Size2D PtToMm(Size2D points, int dpi = DPI.DEFAULT)
    {
        var inch = PtToIn(points, dpi);
        return InToMm(inch);
    }
    /// <summary>
    /// Converts a measurement from points to inches, considering the specified DPI (dots per inch).
    /// </summary>
    /// <param name="points">The measurement in points.</param>
    /// <param name="dpi">
    /// The DPI (dots per inch) to use for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.
    /// </param>
    /// <returns>The equivalent measurement in inches.</returns>
    public static float PtToIn(float points, int dpi = DPI.DEFAULT)
    {
        return points / dpi;
    }
    /// <summary>
    /// Converts a two-dimensional measurement from points to inches.
    /// </summary>
    /// <param name="points">The two-dimensional measurement in points, represented as a <see cref="Size2D"/>.</param>
    /// <param name="dpi">The dots per inch (DPI) value used for the conversion. Defaults to <see cref="DPI.DEFAULT"/>.</param>
    /// <returns>The equivalent two-dimensional measurement in inches, represented as a <see cref="Size2D"/>.</returns>
    public static Size2D PtToIn(Size2D points, int dpi = DPI.DEFAULT)
    {
        return points / dpi;
    }

    /// <summary>
    /// Adjusts the dimensions of a two-dimensional measurement to match a target DPI (dots per inch).
    /// </summary>
    /// <param name="points">The original two-dimensional measurement, represented as a <see cref="Size2D"/>.</param>
    /// <param name="srcDpi">The source DPI of the measurement.</param>
    /// <param name="targetDpi">The target DPI to which the measurement should be adjusted.</param>
    /// <returns>
    /// A <see cref="Size2D"/> representing the adjusted dimensions at the target DPI.
    /// </returns>
    public static Size2D ModifyDpi(Size2D points, int srcDpi, int targetDpi)
    {
        var factor = (float)srcDpi / targetDpi;
        return points / factor;
    }

    /// <summary>
    /// Calculates the size of a rectangular area defined by two coordinates.
    /// </summary>
    /// <param name="topLeft">
    /// The top-left coordinate of the rectangle.
    /// </param>
    /// <param name="bottomRight">
    /// The bottom-right coordinate of the rectangle.
    /// </param>
    /// <returns>
    /// A <see cref="Size2D"/> representing the width and height of the rectangle.
    /// </returns>
    public static Size2D CalculateSize(Coordinate2D topLeft, Coordinate2D bottomRight)
        => new(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
    /// <summary>
    /// Calculates the scaled size of a source dimension to fit within a target dimension while maintaining the aspect ratio.
    /// </summary>
    /// <param name="source">The original size to be scaled, represented as a <see cref="Size2D"/>.</param>
    /// <param name="target">
    /// The target size within which the source size should fit, represented as a <see cref="Size2D"/>.
    /// If either width or height of the target is zero, the scaling will be based on the non-zero dimension.
    /// </param>
    /// <returns>
    /// A new <see cref="Size2D"/> representing the scaled size of the source dimension that fits within the target dimension.
    /// </returns>
    public static Size2D CalculateSize(Size2D source, Size2D target)
    {
        if (target is { Width: 0, Height: 0 } || source.Equals(target))
        {
            return new Size2D(target.Width, target.Height);
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

        return new Size2D((int)(source.Width * factor), (int)(source.Height * factor));
    }
    /// <summary>
    /// Converts a given dimension to pixels based on the specified unit and target parameters.
    /// </summary>
    /// <param name="dimension">The dimension value to be converted.</param>
    /// <param name="unit">The unit of measurement for the dimension (e.g., Points, Inches, Millimeters, Percent).</param>
    /// <param name="targetDimension">
    /// The target dimension (e.g., width or height) in pixels, used when the unit is <see cref="LengthUnit.Percent"/>.
    /// Defaults to 0 if not provided.
    /// </param>
    /// <param name="targetDpi">
    /// The target DPI (dots per inch) used for conversion when the unit is <see cref="LengthUnit.Millimeters"/> or <see cref="LengthUnit.Inches"/>.
    /// Defaults to the DPI value defined in <see cref="DPI.DEFAULT"/> if not provided.
    /// </param>
    /// <returns>The equivalent dimension in pixels as an integer.</returns>
    public static int GetPixels(float dimension, LengthUnit unit, int targetDimension = 0, int targetDpi = DPI.DEFAULT)
    {
        switch (unit)
        {
            case LengthUnit.Percent:
                return (int)(targetDimension * (dimension / 100));
            case LengthUnit.Millimeters:
                return (int)MmToPt(dimension, targetDpi);
            case LengthUnit.Inches:
                return (int)InToPt(dimension, targetDpi);
            default: //case LengthUnit.Point:
                return (int)dimension;
        }
    }

    /// <summary>
    /// Converts a <see cref="Position2D"/> to a pair of coordinates representing the top-left and bottom-right corners
    /// within a given total size.
    /// </summary>
    /// <param name="position">
    /// The <see cref="Position2D"/> specifying the position with optional top, left, bottom, and right offsets.
    /// </param>
    /// <param name="totalSize">
    /// The total size as a <see cref="Size2D"/> within which the coordinates are calculated.
    /// </param>
    /// <returns>
    /// A tuple containing the top-left and bottom-right coordinates as <see cref="Coordinate2D"/>.
    /// </returns>
    public static (Coordinate2D TopLeft, Coordinate2D BottomRight) ToCoordinates(Position2D position, Size2D totalSize)
    {
        var topLeft = new Coordinate2D(position.Left ?? 0, position.Top ?? 0);
        var bottomRight = new Coordinate2D(totalSize.Width, totalSize.Height) - new Coordinate2D(position.Right, position.Bottom);
        return (topLeft, bottomRight);
    }
    /// <summary>
    /// Converts a given position and total size into a coordinate and size representation.
    /// </summary>
    /// <param name="position">
    /// The position, defined by its top, left, bottom, and right boundaries.
    /// </param>
    /// <param name="totalSize">
    /// The total size of the area, used to calculate the resulting coordinate and size.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="Coordinate2D"/> representing the top-left coordinate of the position.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="Size2D"/> representing the dimensions of the position.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    public static (Coordinate2D Coordinate, Size2D Size) ToCoordinateSize(Position2D position, Size2D totalSize)
    {
        var coordinates = ToCoordinates(position, totalSize);
        var calculatedSize = CalculateSize(coordinates.TopLeft, coordinates.BottomRight);
        return (coordinates.TopLeft, calculatedSize);
    }
    /// <summary>
    /// Converts the specified top-left and bottom-right coordinates into a <see cref="Position2D"/> object
    /// relative to the given total size.
    /// </summary>
    /// <param name="topLeft">The top-left coordinate of the position.</param>
    /// <param name="bottomRight">The bottom-right coordinate of the position.</param>
    /// <param name="totalSize">The total size used to calculate the relative position.</param>
    /// <returns>
    /// A <see cref="Position2D"/> object representing the relative position defined by the given coordinates
    /// and total size.
    /// </returns>
    public static Position2D ToPosition(Coordinate2D topLeft, Coordinate2D bottomRight, Size2D totalSize)
    {
        var br = new Coordinate2D(totalSize.Width, totalSize.Height) - new Coordinate2D(bottomRight.X, bottomRight.Y);
        return new Position2D(topLeft.Y, topLeft.X, br.Y, br.X);
    }
}