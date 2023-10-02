using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Abstractions;

public interface IQRCodeWriter
{
    IImageFile Create(QRCodeInput input);
}