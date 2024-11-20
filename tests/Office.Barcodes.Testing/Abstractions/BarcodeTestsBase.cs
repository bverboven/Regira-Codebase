﻿using NUnit.Framework.Legacy;
using Regira.Drawing.SkiaSharp.Services;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Utilities;
using Regira.Office.Barcodes.Abstractions;
using Regira.Office.Barcodes.Exceptions;
using Regira.Office.Barcodes.Models;
using Regira.Utilities;

namespace Office.Barcodes.Testing.Abstractions;

public class BarcodeTestsBase
{
    protected readonly IBarcodeWriter? Writer;
    protected readonly IBarcodeReader? Reader;
    protected readonly string InputDir;
    protected readonly string OutputDir;
    protected readonly ImageService ImageService;
    protected BarcodeTestsBase(IBarcodeService service, string outputFolderName)
        : this(service, service, outputFolderName)
    {
    }
    protected BarcodeTestsBase(IBarcodeWriter? writer, IBarcodeReader? reader, string outputFolderName)
    {
        Writer = writer;
        Reader = reader;
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        InputDir = Path.Combine(assetsDir, "Input", "Barcodes");
        OutputDir = Path.Combine(assetsDir, "Output", outputFolderName);
        Directory.CreateDirectory(OutputDir);

        ImageService = new ImageService();
    }


    public virtual async Task Create_Barcode(string content, BarcodeFormat format, string outputName)
    {
        if (Writer == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        var input = new BarcodeInput
        {
            Content = content,
            Format = format,
        };

        using var barCodeImg = Writer.Create(input);
        await barCodeImg.SaveAs(Path.Combine(OutputDir, $"{outputName}-{input.Format.ToString().ToLowerInvariant()}.jpg"));
        ClassicAssert.IsNotNull(barCodeImg.GetBytes());
        Assert.That(barCodeImg.GetLength() > 0, Is.True);
    }
    public virtual async Task Check_Dimensions(string content, BarcodeFormat format, int[] size)
    {
        if (Writer == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        var input = new BarcodeInput
        {
            Content = content,
            Format = format,
            Size = size
        };
        using var barCodeImg = Writer.Create(input);
        await barCodeImg.SaveAs(Path.Combine(OutputDir, $"barcode-{input.Format}-{input.Size.Width}x{input.Size.Height}.jpg"));
        ClassicAssert.IsNotNull(barCodeImg.GetBytes());
        Assert.That(barCodeImg.GetLength() > 0, Is.True);
        Assert.That(barCodeImg.Size?.Width, Is.EqualTo(input.Size.Width));
        Assert.That(barCodeImg.Size?.Height, Is.EqualTo(input.Size.Height));
    }
    public virtual void TooLong_Expect_InputException()
    {
        if (Writer == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        var input = Convert.ToBase64String(Enumerable.Range(0, 5000).Select((_, i) => (byte)(i % 8)).ToArray());
        Assert.Throws<InputException>(() =>
        {
            Writer.Create(input);
        });
    }

    public virtual void Read_Barcode(string inputImg, string expectedContent)
    {
        if (Reader == null)
        {
            Assert.Ignore("Reading not supported");
            return;
        }

        var input = new ImageFile().Load(Path.Combine(InputDir, inputImg));

        var content = string.Join(Environment.NewLine, Reader.Read(input)?.Contents!);
        Assert.That(content, Is.EqualTo(expectedContent));
    }

    public virtual Task Create_And_Read_Barcode()
    {
        if (Reader == null)
        {
            Assert.Ignore("Reading not supported");
            return Task.CompletedTask;
        }

        var input = new BarcodeInput
        {
            Content = "This is a test",
            Format = BarcodeFormat.Code128
        };
        var inputBytes = Writer?.Create(input)!;
        var result = Reader.Read(inputBytes);
        Assert.That(result?.Contents?.FirstOrDefault(), Is.EqualTo(input.Content));
        return Task.CompletedTask;
    }
}