using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.Abstractions;

public interface IImageToAddOptions
{
    LengthUnit DimensionUnit { get; set; }
    Size2D? Size { get; set; }
    double Margin { get; set; }
    ImagePosition PositionType { get; set; }
    Position2D? Position { get; set; }
    double Rotation { get; set; }
    double Opacity { get; set; }
}