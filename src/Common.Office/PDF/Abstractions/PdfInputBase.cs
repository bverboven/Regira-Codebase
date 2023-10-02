using Regira.Office.Models;
using Regira.Utilities;

namespace Regira.Office.PDF.Abstractions;

public abstract class PdfInputBase
{
    public int DPI { get; set; }
    public PageOrientation Orientation { get; set; }
    public PageSize Format { get; set; }
    /// <summary>
    /// Width of margins in points
    /// </summary>
    public Margins Margins { get; set; }

    protected PdfInputBase(int dpi = DimensionsUtility.DPI.DEFAULT)
    {
        DPI = dpi;
        Orientation = PageOrientation.Portrait;
        Format = PageSize.A4;
        Margins = DimensionsUtility.MmToPt(10f, dpi);
    }
}