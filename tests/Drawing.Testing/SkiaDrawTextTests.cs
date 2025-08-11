using Regira.Drawing.SkiaSharp.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Enums;
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
    }

    [Test]
    public async Task Add_Text_With_Options()
    {
        var input = "Hello World!";
        using var testImage = SkiaUtility.CreateTextImage(input, new TextImageOptions
        {
            BackgroundColor = "#FFFF00",
            TextColor = "#0000FF",
            FontName = "Arial",
        });

        using var imgFile = testImage.ToImageFile();
        await imgFile.SaveAs(Path.Combine(_outputDir, "hello-world_options.png"));
        Assert.That(imgFile.Format, Is.EqualTo(ImageFormat.Png));

        var content = await ReadImageText(testImage);

        Assert.That(content, Is.EqualTo(input));
    }
    
    
    protected Task<string?> ReadImageText(SKBitmap img)
    {
        using var imageFile = img.ToImageFile();
        var ocrManager = new OcrManager();
        return ocrManager.Read(imageFile);
    }
}