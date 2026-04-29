namespace Regira.Office.Barcodes.Models.DTO;

public class QRCodeInputDto
{
    public string Content { get; set; } = null!;
    public int Size { get; set; } = 200;
    public string Color { get; set; } = "#000000";
}