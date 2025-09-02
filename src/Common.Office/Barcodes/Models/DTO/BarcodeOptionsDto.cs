using Regira.Dimensions;

namespace Regira.Office.Barcodes.Models.DTO;

public record BarcodeOptionsDto
{
    public BarcodeFormat? Format { get; set; }
    public string Content { get; set; } = null!;
    public LengthUnit? DimensionUnit { get; set; }
    public float? Width { get; set; }
    public float? Height { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
}