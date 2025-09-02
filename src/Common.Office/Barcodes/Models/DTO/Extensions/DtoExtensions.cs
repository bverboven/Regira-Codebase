using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Dimensions;
using Regira.Media.Drawing.Services;

namespace Regira.Office.Barcodes.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static BarcodeInput ToBarcodeInput(this BarcodeOptionsDto dto, ImageSize targetSize, int? dpi = null)
        => new()
        {
            Format = dto.Format ?? BarcodeFormat.Code128,
            Content = dto.Content,
            Size = new Size2D(dto.Width ?? 400, dto.Height ?? 50).ToImageSize(dto.DimensionUnit ?? ImageLayerDefaults.DimensionUnit, targetSize, dpi ?? ImageLayerDefaults.Dpi)!.Value,
            Color = dto.Color ?? "#000000",
            BackgroundColor = dto.BackgroundColor ?? "#FFFFFF"
        };
    public static QRCodeInput ToQRCodeInput(this BarcodeOptionsDto dto, ImageSize targetSize, int? dpi = null)
        => new()
        {
            Format = BarcodeFormat.QRCode,
            Content = dto.Content,
            Size = new Size2D(dto.Width ?? 400, dto.Height ?? 50).ToImageSize(dto.DimensionUnit ?? ImageLayerDefaults.DimensionUnit, targetSize, dpi ?? ImageLayerDefaults.Dpi)!.Value,
            Color = dto.Color ?? "#000000",
            BackgroundColor = dto.BackgroundColor ?? "#FFFFFF"
        };
}