namespace Regira.Media.Drawing.Models;

public class TextImageOptions
{
    public const int DEFAULT_FONT_SIZE = 15;
    public const string DEFAULT_FONT_NAME = "Arial";
    public const string DEFAULT_TEXT_COLOR = "#000000FF";
    public const string DEFAULT_BACKGROUND_COLOR = "#FFFFFFFF";

    public string? FontName { get; set; }
    public int? FontSize { get; set; }
    public Color? TextColor { get; set; }
    public Color? BackgroundColor { get; set; }
    public int? Padding { get; set; }
    [Obsolete("Use Padding property instead.", true)]
    public int Margin
    {
        get => Padding ?? 0;
        set => Padding = value;
    }
}