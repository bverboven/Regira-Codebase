using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IBarcodeReader
{
    BarcodeReadResult? Read(IImageFile img, BarcodeFormat? format = null);
}