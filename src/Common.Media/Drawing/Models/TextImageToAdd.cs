using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Models;

public record TextImageToAdd : IImageToAddOptions
{
    public string Text { get; set; } = null!;
    public TextImageOptions TextOptions { get; set; } = null!;

    public LengthUnit DimensionUnit { get; set; } = LengthUnit.Points;
    public Size2D? Size { get; set; }
    public float Margin { get; set; } = DrawImageDefaults.Margin;
    public ImagePosition PositionType { get; set; } = ImagePosition.Absolute;
    public Position2D? Position { get; set; } = DrawImageDefaults.Position;
    public float Rotation { get; set; } = DrawImageDefaults.Rotation;
    public float Opacity { get; set; } = DrawImageDefaults.Opacity;
}