using Regira.Dimensions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models;

public class CanvasImageOptions
{
    public Size2D Size { get; set; }
    public Color? BackgroundColor { get; set; }
    public ImageFormat? ImageFormat { get; set; }

    public static implicit operator CanvasImageOptions(Size2D size) => new() { Size = size };
}