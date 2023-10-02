using Regira.Dimensions;

namespace Regira.Utilities;

public static class DimensionsUtility
{
    public static class DPI
    {
        public const int WIN_PPI = 96;
        public const int MAC_PPI = 72;

        public const int DEFAULT = WIN_PPI;
    }

    public const float CM_PER_INCH = 2.54f;
    public const float MM_PER_INCH = CM_PER_INCH * 10;

    // to Inches
    public static float MmToIn(float mm)
    {
        return mm / MM_PER_INCH;
    }
    public static Size2D MmToIn(Size2D mm)
    {
        return mm / MM_PER_INCH;
    }
    // from Inches
    public static float InToMm(float inches)
    {
        return inches * MM_PER_INCH;
    }
    public static Size2D InToMm(Size2D inches)
    {
        return inches * MM_PER_INCH;
    }

    // to Points
    public static float MmToPt(float mm, int dpi = DPI.DEFAULT)
    {
        var inch = MmToIn(mm);
        return InToPt(inch, dpi);
    }
    public static Size2D MmToPt(Size2D mm, int dpi = DPI.DEFAULT)
    {
        var inch = MmToIn(mm);
        return InToPt(inch, dpi);
    }
    public static float InToPt(float inches, int dpi = DPI.DEFAULT)
    {
        return inches * dpi;
    }
    public static Size2D InToPt(Size2D inches, int dpi = DPI.DEFAULT)
    {
        return inches * dpi;
    }

    // from Points
    public static float PtToMm(float points, int dpi = DPI.DEFAULT)
    {
        var inch = PtToIn(points, dpi);
        return InToMm(inch);
    }
    public static Size2D PtToMm(Size2D points, int dpi = DPI.DEFAULT)
    {
        var inch = PtToIn(points, dpi);
        return InToMm(inch);
    }
    public static float PtToIn(float points, int dpi = DPI.DEFAULT)
    {
        return points / dpi;
    }
    public static Size2D PtToIn(Size2D points, int dpi = DPI.DEFAULT)
    {
        return points / dpi;
    }

    // dpi
    public static Size2D ModifyDPI(Size2D points, int srcDPI, int targetDPI)
    {
        var factor = (float)srcDPI / targetDPI;
        return points / factor;
    }
}