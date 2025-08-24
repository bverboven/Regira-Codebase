using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.DTO;

public class TextImageInputDto
{
    public class TextOptions
    {
        public string? FontName { get; set; }
        public int? FontSize { get; set; }
        public string? TextColor { get; set; }
        public string? BackgroundColor { get; set; }
        public int? Padding { get; set; }
    }

    public string Text { get; set; } = null!;
    public TextOptions? Options { get; set; }

    public LengthUnit? DimensionUnit { get; set; }
    public float? Width { get; set; }
    public float? Height { get; set; }
    public float? Margin { get; set; }
    public ImagePosition? PositionType { get; set; }
    public float? Left { get; set; }
    public float? Top { get; set; }
    public float? Right { get; set; }
    public float? Bottom { get; set; }
    public float? Rotation { get; set; }
    public float? Opacity { get; set; }
}