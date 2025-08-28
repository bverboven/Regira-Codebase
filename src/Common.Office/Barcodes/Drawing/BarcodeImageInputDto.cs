using Regira.Media.Drawing.Models.DTO;
using Regira.Office.Barcodes.Models.DTO;

namespace Regira.Office.Barcodes.Drawing;

public class BarcodeImageInputDto
{
    public BarcodeInputDto Barcode { get; set; } = null!;
    public ImageToAddOptionsDto? DrawOptions { get; set; }
}