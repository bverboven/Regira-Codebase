using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Abstractions;
using Regira.Drawing.Barcodes.Models;

namespace Regira.Drawing.Barcodes.Spire;

public class QRCodeService : IQRCodeService
{
    private readonly BarcodeService _service;
    public QRCodeService()
    {
        _service = new BarcodeService();
    }

    public IImageFile Create(QRCodeInput input) => _service.Create(input);
    public BarcodeReadResult Read(IImageFile qrCode) => _service.Read(qrCode);
}