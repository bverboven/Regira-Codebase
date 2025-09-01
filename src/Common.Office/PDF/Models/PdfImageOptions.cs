using Regira.Media.Drawing.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Office.PDF.Models;

public class PdfImageOptions
{
    public ImageSize? Size = null;
    public int Dpi { get; set; } = 96;
    public ImageFormat Format { get; set; } = ImageFormat.Jpeg;
}