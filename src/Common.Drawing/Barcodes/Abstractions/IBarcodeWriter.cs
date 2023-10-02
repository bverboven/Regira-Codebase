using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Abstractions;

public interface IBarcodeWriter
{
    IImageFile Create(BarcodeInput input);
}