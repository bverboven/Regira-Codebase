using Regira.Dimensions;
using Regira.Media.Drawing.Models;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public abstract class BarcodeInputBase
{
    public BarcodeFormat Format { get; set; }
    public string Content { get; set; } = null!;
    public Size2D Size { get; set; } = new(400, 100);
    public Color Color { get; set; } = "#000000";
    public Color BackgroundColor { get; set; } = "#FFFFFF";
}