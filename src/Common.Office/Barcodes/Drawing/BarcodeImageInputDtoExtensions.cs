using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Models.DTO.Extensions;
using Regira.Office.Barcodes.Models;
using Regira.Office.Barcodes.Models.DTO.Extensions;

namespace Regira.Office.Barcodes.Drawing;

public static class BarcodeImageInputDtoExtensions
{
    public static IImageToAdd ToImageToAdd(this BarcodeImageInputDto input)
    {
        return new ImageToAdd<BarcodeInput>
        {
            Source = input.Barcode.ToBarcodeInput(),
            Options = input.DrawOptions?.ToImageToAddOptions()
        };
    }
}