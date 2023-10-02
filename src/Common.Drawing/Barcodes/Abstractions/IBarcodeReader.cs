using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Abstractions;

public interface IBarcodeReader
{
    BarcodeReadResult? Read(IImageFile img, BarcodeFormat? format = null);
}