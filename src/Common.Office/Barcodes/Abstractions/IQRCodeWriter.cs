using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IQRCodeWriter
{
    IImageFile Create(QRCodeInput input);
}