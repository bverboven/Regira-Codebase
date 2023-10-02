using QRCoder;
using QRCoder.Exceptions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Abstractions;
using Regira.Drawing.Barcodes.Exceptions;
using Regira.Drawing.Barcodes.Models;
using Regira.Drawing.GDI.Utilities;
using Regira.Utilities;
using System.Drawing;
using System.Drawing.Imaging;

namespace Regira.Drawing.Barcodes.QRCoder;

public class QRCodeWriter : IQRCodeWriter
{
    public IImageFile Create(QRCodeInput input)
    {
        var generator = new QRCodeGenerator();
        QRCodeData qrData;
        try
        {
            qrData = generator.CreateQrCode(input.Content, QRCodeGenerator.ECCLevel.Q);
        }
        catch (DataTooLongException ex)
        {
            throw new InputException(ex.Message, ex);
        }
        var qrCode = new QRCode(qrData);
        var img = qrCode.GetGraphic(10, ColorUtility.FromHex(input.Color), Color.White, true);
        var width = input.Size.Width;
        var height = input.Size.Height;
        using var resizedImg = GdiUtility.Resize(img, new Size((int)width, (int)height));
        return resizedImg.ToImageFile(ImageFormat.Jpeg);
    }
}