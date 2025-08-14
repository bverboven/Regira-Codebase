using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Models;
using BarcodeFormat = Regira.Office.Barcodes.Models.BarcodeFormat;

namespace Regira.Office.Barcodes.ZXing;

public class QRCodeService : IQRCodeService
{
    private readonly BarcodeService _barcodeService = new();


    public IImageFile Create(QRCodeInput input) => _barcodeService.Create(input);
    public BarcodeReadResult? Read(IImageFile qrCode) => _barcodeService.Read(qrCode, BarcodeFormat.QRCode);
}