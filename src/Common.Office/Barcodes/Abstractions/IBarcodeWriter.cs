using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Abstractions;

public interface IBarcodeWriter
{
    IImageFile Create(BarcodeInput input);
}