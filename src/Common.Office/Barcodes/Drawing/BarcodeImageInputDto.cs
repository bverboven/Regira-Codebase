using Regira.Media.Drawing.Models.DTO;
using Regira.Office.Barcodes.Models.DTO;

namespace Regira.Office.Barcodes.Drawing;

public class BarcodeImageInputDto
{
    public BarcodeOptionsDto Barcode { get; set; } = null!;
    public ImageInputOptionsDto? DrawOptions { get; set; }
}