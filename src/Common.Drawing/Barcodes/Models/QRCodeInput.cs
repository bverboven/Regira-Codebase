namespace Regira.Drawing.Barcodes.Models;

public class QRCodeInput : BarcodeInput
{
    public QRCodeInput()
    {
        Format = BarcodeFormat.QRCode;
    }

    public static implicit operator QRCodeInput(string content) => new() { Content = content };
    public static implicit operator string?(QRCodeInput? input) => input?.Content;
}