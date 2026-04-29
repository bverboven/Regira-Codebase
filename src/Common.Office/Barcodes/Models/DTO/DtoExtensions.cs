namespace Regira.Office.Barcodes.Models.DTO;

public static class DtoExtensions
{
    public static BarcodeInputDto ToBarcodeInputDto(this BarcodeInput input)
        => new()
        {
            Content = input.Content,
            Format = input.Format,
            Width = input.Size.Width > 0 ? input.Size.Width : null,
            Height = input.Size.Height > 0 ? input.Size.Height : null,
            Color = input.Color.Hex,
            BackgroundColor = input.BackgroundColor.Hex
        };

    public static QRCodeInputDto ToQRCodeInputDto(this QRCodeInput input)
        => new()
        {
            Content = input.Content,
            Size = input.Size.Width > 0 ? input.Size.Width : 200,
            Color = input.Color.Hex
        };
}