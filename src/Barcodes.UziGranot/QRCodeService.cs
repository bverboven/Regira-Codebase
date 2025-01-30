using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using QRCodeDecoderLibrary;
using QRCodeEncoderLibrary;
using Regira.Drawing.GDI.Utilities;
using Regira.Media.Drawing.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Exceptions;
using Regira.Office.Barcodes.Models;
using ErrorCorrection = QRCodeEncoderLibrary.ErrorCorrection;

namespace Regira.Office.Barcodes.UziGranot;

public class QRCodeService() : IQRCodeService
{
    public IImageFile Create(QRCodeInput input)
    {
        var moduleSize = 4;

        var encoder = new QRCodeEncoder();
        encoder.ErrorCorrection = ErrorCorrection.M;
        encoder.ModuleSize = moduleSize;
        encoder.QuietZone = moduleSize * 4;
        encoder.EciAssignValue = -1;

        try
        {
            encoder.Encode(input.Content!);
        }
        catch (ApplicationException ex)
        {
            if (ex.Message == "Input data string is too long")
            {
                throw new InputException(input.Content!, ex);
            }
        }

        using var ms = new MemoryStream();
        encoder.SaveQRCodeToPngFile(ms);

        // resize image
        using var img = GdiUtility.Resize(Image.FromStream(ms), input.Size.ToSize());
        return img.ToImageFile(ImageFormat.Jpeg);
    }

    public BarcodeReadResult Read(IImageFile qrCode)
    {
        var decoder = new QRDecoder();
        using var img = GdiUtility.ToBitmap(qrCode);
        using var bitmap = new Bitmap(img);
        var data = decoder.ImageDecoder(bitmap);
        var contents = QRCodeResult(data);

        return new BarcodeReadResult
        {
            Contents = contents,
            Format = BarcodeFormat.QRCode
        };
    }


    private static string[] QRCodeResult(byte[][] dataByteArray)
    {
        // no QR code
        if (dataByteArray == null)
        {
            return [];
        }

        // image has one QR code
        if (dataByteArray.Length == 1)
        {
            return [ForDisplay(QRDecoder.ByteArrayToStr(dataByteArray.First()))];
        }

        // image has more than one QR code
        return dataByteArray.Select(data => ForDisplay(QRDecoder.ByteArrayToStr(data))).ToArray();
    }
    private static string ForDisplay(string result)
    {
        int Index;
        for (Index = 0; Index < result.Length && (result[Index] >= ' ' && result[Index] <= '~' || result[Index] >= 160); Index++)
        {
            // continue;
        }
        if (Index == result.Length)
        {
            return result;
        }

        var Display = new StringBuilder(result.Substring(0, Index));
        for (; Index < result.Length; Index++)
        {
            var OneChar = result[Index];
            if (OneChar >= ' ' && OneChar <= '~' || OneChar >= 160)
            {
                Display.Append(OneChar);
                continue;
            }

            if (OneChar == '\r')
            {
                Display.Append("\r\n");
                if (Index + 1 < result.Length && result[Index + 1] == '\n')
                {
                    Index++;
                }
                continue;
            }

            if (OneChar == '\n')
            {
                Display.Append("\r\n");
                continue;
            }

            Display.Append('¿');
        }
        return Display.ToString();
    }
}
