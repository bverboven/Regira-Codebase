using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Abstractions;

public interface IQRCodeReader
{
    BarcodeReadResult? Read(IImageFile qrCode);
}