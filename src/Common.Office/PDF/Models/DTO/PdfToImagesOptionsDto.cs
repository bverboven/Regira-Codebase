using Regira.Media.Drawing.Enums;

namespace Regira.Office.PDF.Models.DTO;

public class PdfToImagesOptionsDto
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public ImageFormat? Format { get; set; }
}