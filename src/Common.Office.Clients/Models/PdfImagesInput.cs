using Regira.Media.Drawing.Enums;

namespace Regira.Office.Clients.Models;

public class PdfImagesInput
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public ImageFormat? Format { get; set; }
}