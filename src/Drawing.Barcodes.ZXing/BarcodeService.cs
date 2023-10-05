﻿using Regira.Drawing.Abstractions;
using Regira.Drawing.Barcodes.Abstractions;
using Regira.Drawing.Barcodes.Exceptions;
using Regira.Drawing.Barcodes.Models;
using Regira.Drawing.SkiaSharp.Utilities;
using Regira.IO.Extensions;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;
using BarcodeFormat = Regira.Drawing.Barcodes.Models.BarcodeFormat;
using ZXingFormat = ZXing.BarcodeFormat;

namespace Regira.Drawing.Barcodes.ZXing;

public class BarcodeService : IBarcodeService
{
    public BarcodeReadResult? Read(IImageFile barcode, BarcodeFormat? format = null)
    {
        var possibleFormats = format.HasValue ? new[] { Convert(format.Value) } : null;
        var reader = new BarcodeReader()
        {
            Options =
            {
                CharacterSet = "UTF-8",
                PossibleFormats = possibleFormats
            }
        };// equivalent to new BarcodeReader<SKBitmap>(null, (image) => new SKBitmapLuminanceSource(image), null);

        using var barCodeImage = SKBitmap.Decode(barcode.GetBytes());
        var luminence = new SKBitmapLuminanceSource(barCodeImage);

        var result = reader.Decode(barCodeImage);
        var content = result?.Text;

        if (result?.Text != null)
        {
            return new BarcodeReadResult
            {
                Format = Convert(result.BarcodeFormat),
                Contents = new[] { content! }
            };
        }

        // try again (with a performance penalty)
        reader = new BarcodeReader
        {
            AutoRotate = true,
            Options =
            {
                TryInverted = true,
                TryHarder = true,
                CharacterSet = "UTF-8",
                PossibleFormats = possibleFormats
            }
        };
        result = reader.Decode(luminence);
        if (result != null)
        {
            content = result.Text;
            return new BarcodeReadResult
            {
                Format = Convert(result.BarcodeFormat),
                Contents = new[] { content }
            };
        }

        return null;
    }
    public IImageFile Create(BarcodeInput input)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = Convert(input.Format),
            Options = new EncodingOptions
            {
                Width = (int)input.Size.Width,
                Height = (int)input.Size.Height,
                Margin = 0,
                PureBarcode = true,
                GS1Format = true,
            },
            Renderer = new SKBitmapRenderer
            {
                Foreground = SKColor.Parse(input.Color),
                Background = SKColor.Parse("#FFFFFF")
            }
        };
        try
        {
            using var img = writer.Write(input.Content);
            using var resizedImg = SkiaUtility.Resize(img, input.Size.ToSkiaSize());

            return resizedImg.ToImageFile(SKEncodedImageFormat.Jpeg);
        }
        catch (Exception ex)
        {
            throw new InputException(ex.Message, ex);
        }
    }

    ZXingFormat Convert(BarcodeFormat format)
    {
        switch (format)
        {
            case BarcodeFormat.Aztec:
                return ZXingFormat.AZTEC;
            case BarcodeFormat.CodaBar:
                return ZXingFormat.CODABAR;
            case BarcodeFormat.Code39:
                return ZXingFormat.CODE_39;
            case BarcodeFormat.Code93:
                return ZXingFormat.CODE_93;
            case BarcodeFormat.Code128:
                return ZXingFormat.CODE_128;
            case BarcodeFormat.DataMatrix:
                return ZXingFormat.DATA_MATRIX;
            case BarcodeFormat.Ean8:
                return ZXingFormat.EAN_8;
            case BarcodeFormat.Ean13:
                return ZXingFormat.EAN_13;
            case BarcodeFormat.Itf:
                return ZXingFormat.ITF;
            case BarcodeFormat.Pdf417:
                return ZXingFormat.PDF_417;
            case BarcodeFormat.QRCode:
                return ZXingFormat.QR_CODE;
            case BarcodeFormat.Upca:
                return ZXingFormat.UPC_A;
            case BarcodeFormat.Upce:
                return ZXingFormat.UPC_E;
            case BarcodeFormat.Any:
            case BarcodeFormat.UnKnown:
                return ZXingFormat.All_1D;
            default:
                throw new NotSupportedException($"Format {format} is not supported");

        }
    }
    BarcodeFormat Convert(ZXingFormat format)
    {
        switch (format)
        {
            case ZXingFormat.AZTEC:
                return BarcodeFormat.Aztec;
            case ZXingFormat.CODABAR:
                return BarcodeFormat.CodaBar;
            case ZXingFormat.CODE_39:
                return BarcodeFormat.Code39;
            case ZXingFormat.CODE_93:
                return BarcodeFormat.Code93;
            case ZXingFormat.CODE_128:
                return BarcodeFormat.Code128;
            case ZXingFormat.DATA_MATRIX:
                return BarcodeFormat.DataMatrix;
            case ZXingFormat.EAN_8:
                return BarcodeFormat.Ean8;
            case ZXingFormat.EAN_13:
                return BarcodeFormat.Ean13;
            case ZXingFormat.ITF:
                return BarcodeFormat.Itf;
            case ZXingFormat.PDF_417:
                return BarcodeFormat.Pdf417;
            case ZXingFormat.QR_CODE:
                return BarcodeFormat.QRCode;
            case ZXingFormat.UPC_A:
                return BarcodeFormat.Upca;
            case ZXingFormat.UPC_E:
                return BarcodeFormat.Upce;
            default:
                return BarcodeFormat.UnKnown;

        }
    }
}