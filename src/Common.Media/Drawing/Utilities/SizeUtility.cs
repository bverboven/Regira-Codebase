using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Utilities;

namespace Regira.Media.Drawing.Utilities;

public static class SizeUtility
{
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
    /// Defaults to the DPI value defined in <see cref="PrintDefaults.Dpi"/> if not provided.
    /// </param>
    /// <returns>The equivalent dimension in pixels as an integer.</returns>
    public static int GetPixels(float dimension, LengthUnit unit, int targetDimension = 0, int? targetDpi = null)
    {
        targetDpi ??= PrintDefaults.Dpi;

        switch (unit)
        {
            case LengthUnit.Percent:
                return (int)(targetDimension * (dimension / 100));
            case LengthUnit.Millimeters:
                return (int)DimensionsUtility.MmToPt(dimension, targetDpi.Value);
            case LengthUnit.Inches:
                return (int)DimensionsUtility.InToPt(dimension, targetDpi.Value);
            default: //case LengthUnit.Point:
                return (int)dimension;
        }
    }
}