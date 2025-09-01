namespace Regira.Media.Drawing.Models.DTO;

public class LabelImageLayerDto
{
    public class LabelOptionsDto
    {
        public string? FontName { get; set; }
        public int? FontSize { get; set; }
        public string? TextColor { get; set; }
        public string? BackgroundColor { get; set; }
        public int? Padding { get; set; }
    }

    public string Text { get; set; } = null!;
    public LabelOptionsDto? LabelOptions { get; set; }
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}