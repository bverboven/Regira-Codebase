namespace Regira.Media.Drawing.Models.DTO;

public class ImageLayerDto
{
    public byte[] Image { get; set; } = null!;
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}