using Regira.Media.Drawing.Dimensions;

namespace Regira.Office.Barcodes.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static BarcodeInput ToBarcodeInput(this BarcodeOptionsDto dto)
        => new()
        {
            Format = dto.Format ?? BarcodeFormat.Code128,
            Content = dto.Content,
            Size = new ImageSize((int)(dto.Width ?? 400), (int)(dto.Height ?? 50)),
            Color = dto.Color ?? "#000000",
            BackgroundColor = dto.BackgroundColor ?? "#FFFFFF"
        };
    public static QRCodeInput ToQRCodeInput(this BarcodeOptionsDto dto)
        => new()
        {
            Format = BarcodeFormat.QRCode,
            Content = dto.Content,
            Size = new ImageSize((int)(dto.Width ?? 400), (int)(dto.Height ?? 400)),
            Color = dto.Color ?? "#000000",
            BackgroundColor = dto.BackgroundColor ?? "#FFFFFF"
        };
}