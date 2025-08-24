using Regira.Media.Drawing.Constants;

namespace Regira.Media.Drawing.Models;

public class TextImageOptions
{
    public string? FontName { get; set; } = TextImageDefaults.FontName;
    public int? FontSize { get; set; } = TextImageDefaults.FontSize;
    public Color? TextColor { get; set; } = TextImageDefaults.TextColor;
    public Color? BackgroundColor { get; set; } = TextImageDefaults.BackgroundColor;
    public int? Padding { get; set; } = TextImageDefaults.Padding;
}