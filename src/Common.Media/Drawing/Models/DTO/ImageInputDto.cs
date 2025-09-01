namespace Regira.Media.Drawing.Models.DTO;

public class ImageInputDto
{
    public byte[] Image { get; set; } = null!;
    public ImageInputOptionsDto? DrawOptions { get; set; }
}