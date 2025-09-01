using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Constants;

public static class DrawImageDefaults
{
    public static LengthUnit DimensionUnit { get; set; } = LengthUnit.Points;
    public static float Margin { get; set; } = 0;
    public static ImagePosition PositionType { get; set; } = ImagePosition.Absolute;
    public static Position2D Position { get; set; } = new (0, 0);
    public static float Rotation { get; set; } = 0;
    public static float Opacity { get; set; } = 1;
    public static int Dpi { get; set; } = 150;
}