using Regira.Dimensions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public abstract class BarcodeInputBase
{
    public BarcodeFormat Format { get; set; }
    public string? Content { get; set; }
    public Size2D Size { get; set; } = new(400, 100);
    public string Color { get; set; } = "#000000";
}