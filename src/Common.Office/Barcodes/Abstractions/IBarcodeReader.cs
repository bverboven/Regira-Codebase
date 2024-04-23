using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IBarcodeReader
{
    BarcodeReadResult? Read(IImageFile img, BarcodeFormat? format = null);
}