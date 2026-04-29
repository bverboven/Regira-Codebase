namespace Regira.Office.Clients.Models;

public class QRCodeInput
{
    public string Content { get; set; } = null!;
    public int Size { get; set; } = 200;
    public string Color { get; set; } = "#000000";
}