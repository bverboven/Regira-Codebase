using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.Abstractions;

public interface IImageToAddOptions
{
    double Width { get; set; }
    double Height { get; set; }
    double Left { get; set; }
    double Top { get; set; }
    double Right { get; set; }
    double Bottom { get; set; }
    double Margin { get; set; }
    LengthUnit DimensionUnit { get; set; }
    ImagePosition Position { get; set; }
    double Rotation { get; set; }
    double Opacity { get; set; }
}