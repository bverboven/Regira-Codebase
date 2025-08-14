using Regira.Drawing.SkiaSharp.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Office.OCR.PaddleOCR;
using Regira.Utilities;
using SkiaSharp;

namespace Drawing.Testing;

[TestFixture]
public class SkiaDrawTextTests
{
    private readonly string _outputDir;
    public SkiaDrawTextTests()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _outputDir = Path.Combine(assetsDir, "Output", typeof(SkiaUtility).Assembly.GetName().Name!.Split('.').Last());
        Directory.CreateDirectory(_outputDir);
    }

    [Test]
    public async Task Add_Text_No_Params()
    {
        var input = "Hello World!";
        using var testImage = SkiaUtility.CreateTextImage(input);
        using var imgFile = testImage.ToImageFile();

        await imgFile.SaveAs(Path.Combine(_outputDir, "hello-world.png"));
        Assert.That(imgFile.Format, Is.EqualTo(ImageFormat.Png));

        var content = await ReadImageText(testImage);

        Assert.That(content, Is.EqualTo(input));
        AssertColor(SKColor.Parse("#FFFFFF"), testImage.GetPixel(1, 1));
    }

    [Test]
    public async Task Add_Text_With_Options()
    {
        var input = "Hello World!";
        using var testImage = SkiaUtility.CreateTextImage(input, new TextImageOptions
        {
            FontSize = 25,
            FontName = "Arial",
            TextColor = "#00F",
            BackgroundColor = "#FFFF0099",
        });

        using var imgFile = testImage.ToImageFile();
        await imgFile.SaveAs(Path.Combine(_outputDir, "hello-world_options.png"));
        Assert.That(imgFile.Format, Is.EqualTo(ImageFormat.Png));

        var content = await ReadImageText(testImage);

        Assert.That(content, Is.EqualTo(input));
        AssertColor(SKColor.Parse("#FFFF00"), testImage.GetPixel(1, 1));
    }


    protected Task<string?> ReadImageText(SKBitmap img)
    {
        using var imageFile = img.ToImageFile();
        var ocrManager = new OcrManager();
        return ocrManager.Read(imageFile);
    }
    protected void AssertColor(SKColor expected, SKColor actual)
    {
        Assert.That(Math.Abs(actual.Red - expected.Red), Is.LessThan(10));
        Assert.That(Math.Abs(actual.Green - expected.Green), Is.LessThan(10));
        Assert.That(Math.Abs(actual.Blue - expected.Blue), Is.LessThan(10));
    }
}