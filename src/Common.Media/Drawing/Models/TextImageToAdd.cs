using Regira.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Models;

public record TextImageToAdd : IImageToAddOptions
{
    public string Text { get; set; } = null!;
    public TextImageOptions TextOptions { get; set; } = null!;

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