using Regira.Dimensions;
using Regira.Drawing.Enums;

namespace Regira.Office.PDF.Models;

public class PdfImageOptions
{
    public Size2D? Size = null;
    public int Dpi { get; set; } = 96;
    public ImageFormat Format { get; set; } = ImageFormat.Jpeg;
}