namespace Regira.Drawing.Barcodes.Models;

public class BarcodeReadResult
{
    public BarcodeFormat? Format { get; set; }
    public string[]? Contents { get; set; }
}