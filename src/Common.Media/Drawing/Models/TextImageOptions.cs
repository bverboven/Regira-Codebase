namespace Regira.Media.Drawing.Models;

public class TextImageOptions
{
    public const int DEFAULT_FONT_SIZE = 15;
    public const string DEFAULT_FONT_NAME = "Arial";

    public string FontName { get; set; } = DEFAULT_FONT_NAME;
    public int FontSize { get; set; } = DEFAULT_FONT_SIZE;
    public Color TextColor { get; set; } = "#000000FF";
    public Color BackgroundColor { get; set; } = "#FFFFFFFF";
    public int Margin { get; set; } = 0;
}