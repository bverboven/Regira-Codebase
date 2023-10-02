using Regira.Dimensions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Abstractions;

public abstract class BarcodeInputBase
{
    public BarcodeFormat Format { get; set; }
    public string? Content { get; set; }
    public Size2D Size { get; set; } = new(400, 100);
    public string Color { get; set; } = "#000000";
}