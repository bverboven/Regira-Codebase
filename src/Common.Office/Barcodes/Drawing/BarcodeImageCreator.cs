using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Drawing;
public class BarcodeImageCreator(IBarcodeWriter barcodeWriter) : ImageCreatorBase<BarcodeInput>
{
    public override IImageFile Create(BarcodeInput input)
        => barcodeWriter.Create(input);
}