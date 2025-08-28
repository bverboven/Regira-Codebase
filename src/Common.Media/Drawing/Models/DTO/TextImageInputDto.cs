namespace Regira.Media.Drawing.Models.DTO;

public class TextImageInputDto
{
    public class TextOptionsDto
    {
        public string? FontName { get; set; }
        public int? FontSize { get; set; }
        public string? TextColor { get; set; }
        public string? BackgroundColor { get; set; }
        public int? Padding { get; set; }
    }

    public string Text { get; set; } = null!;
    public TextOptionsDto? TextOptions { get; set; }
    public ImageToAddOptionsDto? DrawOptions { get; set; }
}