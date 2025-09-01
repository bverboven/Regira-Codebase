using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.DTO;

public class CanvasImageLayerDto
{
    public class CanvasOptionsDto
    {
        public string? BackgroundColor { get; set; }
        public ImageFormat? ImageFormat { get; set; }
    }

    public float Width { get; set; }
    public float Height { get; set; }
    public LengthUnit? DimensionUnit { get; set; }
    public int? Dpi { get; set; }
    public CanvasOptionsDto? CanvasOptions { get; set; }
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}