using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.DTO;

public class ImageToAddDto
{
    public byte[] Image { get; set; } = null!;

    public LengthUnit? DimensionUnit { get; set; }
    public float? Width { get; set; }
    public float? Height { get; set; }
    public float? Margin { get; set; }
    public ImagePosition? PositionType { get; set; }
    public float? Top { get; set; }
    public float? Left { get; set; }
    public float? Bottom { get; set; }
    public float? Right { get; set; }
    public float? Rotation { get; set; }
    public float? Opacity { get; set; }
}