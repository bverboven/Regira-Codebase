using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models;

public class ImageToAddOptions
{
    public LengthUnit DimensionUnit { get; set; } = DrawImageDefaults.DimensionUnit;
    public Size2D? Size { get; set; }
    public float Margin { get; set; } = DrawImageDefaults.Margin;
    public ImagePosition PositionType { get; set; } = DrawImageDefaults.PositionType;
    public Position2D? Position { get; set; } = DrawImageDefaults.Position;
    public float Rotation { get; set; } = DrawImageDefaults.Rotation;
    public float Opacity { get; set; } = DrawImageDefaults.Opacity;
}