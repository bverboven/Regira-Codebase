using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.DTO;

public class ImageToAddDto
{
    public byte[] Image { get; set; } = null!;

    public LengthUnit? DimensionUnit { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Margin { get; set; }
    public ImagePosition? PositionType { get; set; }
    public double? Top { get; set; }
    public double? Left { get; set; }
    public double? Bottom { get; set; }
    public double? Right { get; set; }
    public double? Rotation { get; set; }
    public double? Opacity { get; set; }
}