using Regira.Drawing.Barcodes.Abstractions;
using Regira.Drawing.Barcodes.Exceptions;
using Regira.Drawing.Barcodes.Models;
using Regira.Drawing.Core;
using Regira.Drawing.SkiaSharp.Services;
using Regira.Drawing.Utilities;
using Regira.IO.Extensions;
using Regira.Utilities;

namespace Drawing.Barcodes.Testing.Abstractions;

public abstract class QRCodeTestsBase
{
    protected readonly IQRCodeWriter? QRWriter;
    protected readonly IQRCodeReader? QRReader;
    protected readonly string InputDir;
    protected readonly string OutputDir;
    protected readonly ImageService ImageService;
    protected QRCodeTestsBase(IQRCodeService service, string outputFolderName)
        : this(service, service, outputFolderName)
    {
    }
    protected QRCodeTestsBase(IQRCodeWriter? writer, IQRCodeReader? reader, string outputFolderName)
    {
        QRWriter = writer;
        QRReader = reader;
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        InputDir = Path.Combine(assetsDir, "Input", "QR");
        OutputDir = Path.Combine(assetsDir, "Output", "QR", outputFolderName);
        Directory.CreateDirectory(OutputDir);

        ImageService = new ImageService();
    }


    public virtual async Task Create_QRCode(string input, string outputName)
    {
        if (QRWriter == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        using var barCodeImg = QRWriter.Create(input);
        await barCodeImg.SaveAs(Path.Combine(OutputDir, $"{outputName}.jpg"));
        Assert.IsNotNull(barCodeImg.GetBytes());
        Assert.IsTrue(barCodeImg.GetLength() > 0);
    }
    public virtual async Task Check_Dimensions(string content, int size)
    {
        if (QRWriter == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        var input = new QRCodeInput
        {
            Content = content,
            Size = size
        };
        using var barCodeImg = QRWriter.Create(input);
        await barCodeImg.SaveAs(Path.Combine(OutputDir, $"barcode-{input.Size.Width}x{input.Size.Height}-zxing.jpg"));
        Assert.IsNotNull(barCodeImg.GetBytes());
        Assert.IsTrue(barCodeImg.GetLength() > 0);
        Assert.AreEqual(input.Size.Width, barCodeImg.Size?.Width);
    }
    public virtual void TooLong_Expect_InputException()
    {
        if (QRWriter == null)
        {
            Assert.Ignore("Writing not supported");
            return;
        }

        var input = Convert.ToBase64String(Enumerable.Range(0, 2500).Select((_, i) => (byte)(i % 8)).ToArray());
        Assert.Throws<InputException>(() =>
        {
            QRWriter.Create(input);
        });
    }

    public virtual void Read_QRCode(string filename, string expectedContent)
    {
        if (QRReader == null)
        {
            Assert.Ignore("Reading not supported");
            return;
        }

        var inputImg = new ImageFile().Load(Path.Combine(InputDir, filename));

        var result = QRReader.Read(inputImg);
        var content = string.Join(Environment.NewLine, result?.Contents!);
        Assert.AreEqual(expectedContent, content);
    }

    public virtual Task Create_And_Read_QRCode()
    {
        if (QRReader == null)
        {
            Assert.Ignore("Reading not supported");
            return Task.CompletedTask;
        }

        var input = "This is a test";
        var inputBytes = QRWriter?.Create(input);
        var content = QRReader.Read(inputBytes!)?.Contents?.FirstOrDefault();
        Assert.AreEqual(input, content);
        return Task.CompletedTask;
    }
}