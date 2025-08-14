using Regira.Dimensions;
using Regira.Media.Drawing.Models;
using Regira.Utilities;

namespace Regira.Media.Drawing.Utilities;

public static class SizeUtility
{
    /// <summary>
    /// Calculates new dimensions while respecting ratio
    /// </summary>
    /// <returns></returns>
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

    public static Size2D CalculateSize(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        return CalculateSize(new[] { sourceWidth, sourceHeight }, new[] { targetWidth, targetHeight });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dimension"></param>
    /// <param name="unit"></param>
    /// <param name="targetDimension">Only used for relative dimensions (such as percentage)</param>
    /// <param name="targetDpi"></param>
    /// <returns></returns>
    public static int GetPixels(double dimension, LengthUnit unit, int targetDimension = 0, int targetDpi = ImageConstants.DEFAULT_DPI)
    {
        switch (unit)
        {
            case LengthUnit.Percent:
                return (int)(targetDimension * (dimension / 100));
            case LengthUnit.Millimeters:
                return (int)DimensionsUtility.MmToPt((float)dimension, targetDpi);
            case LengthUnit.Inches:
                return (int)DimensionsUtility.InToPt((float)dimension, targetDpi);
            default: //case LengthUnit.Point:
                return (int)dimension;
        }
    }
}