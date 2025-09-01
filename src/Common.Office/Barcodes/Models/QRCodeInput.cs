using Regira.Media.Drawing.Dimensions;

namespace Regira.Office.Barcodes.Models;

public class QRCodeInput : BarcodeInput
{
    public QRCodeInput()
    {
        Format = BarcodeFormat.QRCode;
        Size = new ImageSize(400, 400);
    }

    public static implicit operator QRCodeInput(string content) => new() { Content = content };
    public static implicit operator string?(QRCodeInput? input) => input?.Content;
}