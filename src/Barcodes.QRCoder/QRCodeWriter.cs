using QRCoder;
using QRCoder.Exceptions;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Utilities;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Exceptions;
using Regira.Office.Barcodes.Models;
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

        var qrCode = new PngByteQRCode(qrData);
        var img = qrCode.GetGraphic(10, true).ToBinaryFile().ToImageFile();
        var width = input.Size.Width;
        var height = input.Size.Height;
        using var resizedImg = GdiUtility.Resize(img.ToBitmap(), new Size((int)width, (int)height));
        return resizedImg.ToImageFile(ImageFormat.Jpeg);
    }
}