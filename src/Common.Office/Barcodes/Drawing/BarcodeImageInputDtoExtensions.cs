using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Models.DTO.Extensions;
using Regira.Office.Barcodes.Models;
using Regira.Office.Barcodes.Models.DTO.Extensions;

namespace Regira.Office.Barcodes.Drawing;

public static class BarcodeImageLayerDtoExtensions
{
    public static IImageLayer ToImageLayer(this BarcodeImageLayerDto input)
    {
        return new ImageLayer<BarcodeInput>
        {
            Source = input.BarcodeOptions.ToBarcodeInput(),
            Options = input.DrawOptions?.ToImageLayerOptions()
        };
    }
}