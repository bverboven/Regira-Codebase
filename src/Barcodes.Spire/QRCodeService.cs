using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Models;

namespace Regira.Office.Barcodes.Spire;

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