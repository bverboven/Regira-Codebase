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
    /// <param name="srcDPI">The source DPI of the measurement.</param>
    /// <param name="targetDPI">The target DPI to which the measurement should be adjusted.</param>
    /// <returns>
    /// A <see cref="Size2D"/> representing the adjusted dimensions at the target DPI.
    /// </returns>
    public static Size2D ModifyDPI(Size2D points, int srcDPI, int targetDPI)
    {
        var factor = (float)srcDPI / targetDPI;
        return points / factor;
    }
}