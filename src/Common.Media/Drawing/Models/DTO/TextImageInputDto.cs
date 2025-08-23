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
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Margin { get; set; }
    public ImagePosition? PositionType { get; set; }
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Right { get; set; }
    public double? Bottom { get; set; }
    public double? Rotation { get; set; }
    public double? Opacity { get; set; }
}