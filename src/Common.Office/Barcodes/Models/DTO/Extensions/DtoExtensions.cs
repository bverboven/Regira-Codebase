using Regira.Media.Drawing.Dimensions;
using Regira.Office.Barcodes.Defaults;

namespace Regira.Office.Barcodes.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static BarcodeInput ToBarcodeInput(this BarcodeOptionsDto dto)
        => new()
        {
            Content = dto.Content,
            Format = dto.Format ?? BarcodeDefaults.Format,
            Size = new ImageSize(dto.Width ?? BarcodeDefaults.Size.Width, dto.Height ?? BarcodeDefaults.Size.Height),
            Color = dto.Color,
            BackgroundColor = dto.BackgroundColor
        };
}