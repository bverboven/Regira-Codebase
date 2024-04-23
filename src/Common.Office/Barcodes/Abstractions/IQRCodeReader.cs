using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IQRCodeReader
{
    BarcodeReadResult? Read(IImageFile qrCode);
}