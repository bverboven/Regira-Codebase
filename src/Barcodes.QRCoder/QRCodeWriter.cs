using QRCoder;
using QRCoder.Exceptions;
using Regira.Drawing.GDI.Utilities;
using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Exceptions;
using Regira.Office.Barcodes.Models;
using Regira.Utilities;
using System.Drawing;
using System.Drawing.Imaging;

namespace Regira.Office.Barcodes.QRCoder;

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