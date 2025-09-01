using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models;

public class ImageLayerOptions
{
    public LengthUnit DimensionUnit { get; set; } = ImageLayerDefaults.DimensionUnit;
    public Size2D? Size { get; set; }
    public float Margin { get; set; } = ImageLayerDefaults.Margin;
    public ImagePosition PositionType { get; set; } = ImageLayerDefaults.PositionType;
    public Position2D? Position { get; set; } = ImageLayerDefaults.Position;
    public float Rotation { get; set; } = ImageLayerDefaults.Rotation;
    public float Opacity { get; set; } = ImageLayerDefaults.Opacity;
}