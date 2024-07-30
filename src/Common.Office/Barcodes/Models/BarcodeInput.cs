using Regira.Office.Barcodes.Abstractions;

namespace Regira.Office.Barcodes.Models;

public class BarcodeInput : BarcodeInputBase
{
    public BarcodeInput()
    {
        Format = BarcodeFormat.Code128;
    }

    public static implicit operator BarcodeInput(string content) => new() { Content = content };
    public static implicit operator string?(BarcodeInput? input) => input?.Content;
}