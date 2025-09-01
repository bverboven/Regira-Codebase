using Regira.Media.Drawing.Models.DTO;
using Regira.Office.Barcodes.Models.DTO;

namespace Regira.Office.Barcodes.Drawing;

public class BarcodeImageLayerDto
{
    public BarcodeOptionsDto BarcodeOptions { get; set; } = null!;
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}