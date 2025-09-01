using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.DTO;

public class CanvasImageInputDto
{
    public class CanvasOptionsDto
    {
        public string? BackgroundColor { get; set; }
        public ImageFormat? ImageFormat { get; set; }
    }
    
    public float Width { get; set; }
    public float Height { get; set; }
    public CanvasOptionsDto? CanvasOptions { get; set; }
    public ImageInputOptionsDto? DrawOptions { get; set; }
}