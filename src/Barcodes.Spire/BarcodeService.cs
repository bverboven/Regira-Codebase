using Regira.Drawing.GDI.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Models;
using Spire.Barcode;
using System.Drawing;
using System.Drawing.Imaging;
using SpireBarcodeType = Spire.Barcode.BarCodeType;

namespace Regira.Office.Barcodes.Spire;

public class BarcodeService : IBarcodeReader, IBarcodeWriter
{
    public BarcodeReadResult Read(IImageFile imageBytes, BarcodeFormat? format = null)
    {
        using var imgStream = imageBytes.GetStream();
        if (!format.HasValue)
        {
            var contents = BarcodeScanner.Scan(imgStream, false);
            return new BarcodeReadResult
            {
                Contents = contents
            };
        }

        {
            var spireFormat = Convert(format.Value);
            var contents = BarcodeScanner.Scan(imgStream, spireFormat, false);
            return new BarcodeReadResult
            {
                Contents = contents
            };
        }
    }

    public IImageFile Create(BarcodeInput input)
    {
        var settings = new BarcodeSettings
        {
            Data = input.Content,
            Type = Convert(input.Format),
            ForeColor = input.Color.ToGdiColor(),
            BackColor = Color.White,
            ShowText = false,
            ShowTopText = false,
            ShowTextOnBottom = false,
            ImageWidth = input.Size.Width,
            ImageHeight = input.Size.Height
        };
        var generator = new BarCodeGenerator(settings);
        using var image = generator.GenerateImage();
        var width = input.Size.Width;
        var height = input.Size.Height;
        using var resizedImg = GdiUtility.ResizeFixed(image, new Size((int)width, (int)height));
        return resizedImg.ToImageFile(ImageFormat.Jpeg);
    }

    SpireBarcodeType Convert(BarcodeFormat format)
    {
        Enum.TryParse<SpireBarcodeType>(format.ToString(), out var type);
        return type;
    }
}