namespace Regira.Media.Drawing.Models.DTO;

public class ImageToAddDto
{
    public byte[] Image { get; set; } = null!;
    public ImageToAddOptionsDto? DrawOptions { get; set; }
}