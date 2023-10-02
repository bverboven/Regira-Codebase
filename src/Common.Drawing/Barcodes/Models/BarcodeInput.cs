using Regira.Drawing.Barcodes.Abstractions;

namespace Regira.Drawing.Barcodes.Models;

public class BarcodeInput : BarcodeInputBase
{
    public BarcodeInput()
    {
        Format = BarcodeFormat.Code39;
    }

    public static implicit operator BarcodeInput(string content) => new() { Content = content };
    public static implicit operator string?(BarcodeInput? input) => input?.Content;
}