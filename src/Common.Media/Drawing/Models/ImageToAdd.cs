using Regira.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Models;

public record ImageToAdd : IImageToAddOptions
{
    public IImageFile Image { get; set; } = null!;
    public LengthUnit DimensionUnit { get; set; } = LengthUnit.Points;
    public Size2D? Size { get; set; }
    public double Margin { get; set; }
    public ImagePosition PositionType { get; set; } = ImagePosition.Absolute;
    public Position2D? Position { get; set; }
    public double Rotation { get; set; } = 0;
    public double Opacity { get; set; } = 1;
}