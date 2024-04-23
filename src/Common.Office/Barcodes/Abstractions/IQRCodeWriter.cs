using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IQRCodeWriter
{
    IImageFile Create(QRCodeInput input);
}