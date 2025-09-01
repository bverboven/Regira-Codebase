using Regira.Dimensions;

namespace Regira.Media.Drawing.Models.DTO;

public class LabelImageLayerDto
{
    public class LabelOptionsDto
    {
        public float? FontSize { get; set; }
        public float? Padding { get; set; }
        public LengthUnit? DimensionUnit { get; set; }
        public string? FontName { get; set; }
        public string? TextColor { get; set; }
        public string? BackgroundColor { get; set; }
    }

    public string Text { get; set; } = null!;
    public LabelOptionsDto? LabelOptions { get; set; }
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}