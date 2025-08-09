using Regira.Dimensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Core;

public class ImageToAdd
{
    public string? Path { get; set; }
    public IImageFile? Image { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
    public double Margin { get; set; }
    public LengthUnit DimensionUnit { get; set; } = LengthUnit.Points;
    public ImagePosition Position { get; set; } = ImagePosition.Absolute;
    public double Rotation { get; set; } = 0;
    public double Opacity { get; set; } = 1;
}