using Regira.Media.Drawing.Constants;

namespace Regira.Media.Drawing.Models;

public class LabelImageOptions
{
    public string Text { get; set; } = null!;
    public string? FontName { get; set; } = LabelImageDefaults.FontName;
    public int? FontSize { get; set; } = LabelImageDefaults.FontSize;
    public Color? TextColor { get; set; } = LabelImageDefaults.TextColor;
    public Color? BackgroundColor { get; set; } = LabelImageDefaults.BackgroundColor;
    public int? Padding { get; set; } = LabelImageDefaults.Padding;

    public static implicit operator LabelImageOptions(string content) => new() { Text = content };
}