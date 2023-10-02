using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Abstractions;
using Regira.Drawing.Barcodes.Models;
using BarcodeFormat = Regira.Drawing.Barcodes.Models.BarcodeFormat;

namespace Regira.Drawing.Barcodes.ZXing;

public class QRCodeService : IQRCodeService
{
    private readonly BarcodeService _barcodeService;
    public QRCodeService()
    {
        _barcodeService = new BarcodeService();
    }


    public IImageFile Create(QRCodeInput input) => _barcodeService.Create(input);
    public BarcodeReadResult? Read(IImageFile qrCode) => _barcodeService.Read(qrCode, BarcodeFormat.QRCode);
}