#pragma warning disable CA1416
using System.Drawing;
using System.Drawing.Imaging;
using Regira.Drawing.GDI.Utilities;
using Regira.Media.Drawing.Core;
using Regira.Office.OCR.PaddleOCR;
using Regira.Utilities;

namespace Drawing.Testing;

[TestFixture]
public class GDIDrawTextTests
{
    private readonly string _outputDir;
    public GDIDrawTextTests()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _outputDir = Path.Combine(assetsDir, "Output", typeof(GdiUtility).Assembly.GetName().Name!.Split('.').Last());
        Directory.CreateDirectory(_outputDir);
    }

    [Test]
    public async Task Add_Text_No_Params()
    {
        var input = "Hello World!";
        using var testImage = GdiUtility.CreateTextImage(input);

        testImage.Save(Path.Combine(_outputDir, "hello-world.png"));
        Assert.That(testImage.RawFormat, Is.EqualTo(ImageFormat.Png));

        var content = await ReadImageText(testImage);

        Assert.That(content, Is.EqualTo(input));
        AssertColor(Color.FromArgb(255, 255, 255, 255), ((Bitmap)testImage).GetPixel(0, 0));
        AssertColor(Color.FromArgb(255, 0, 0, 0), ((Bitmap)testImage).GetPixel(5, 4));
    }

    [Test]
    public async Task Add_Text_With_Options()
    {
        var input = "Hello World!";
        using var testImage = GdiUtility.CreateTextImage(input, new TextImageOptions
        {
            BackgroundColor = "#FFFF00",
            TextColor = "#0000FF",
            FontName = "Arial",
        });

        testImage.Save(Path.Combine(_outputDir, "hello-world_options.png"));
        Assert.That(testImage.RawFormat, Is.EqualTo(ImageFormat.Png));

        var content = await ReadImageText(testImage);

        Assert.That(content, Is.EqualTo(input));
        AssertColor(Color.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(0, 0));
        AssertColor(Color.FromArgb(255, 0, 0, 255), ((Bitmap)testImage).GetPixel(5, 4));
    }

    protected Task<string?> ReadImageText(Image img)
    {
        using var imageFile = img.ToImageFile(ImageFormat.Png);
        var ocrManager = new OcrManager();
        return ocrManager.Read(imageFile);
    }
    protected void AssertColor(Color expected, Color actual)
    {
        Assert.That(Math.Abs(actual.R - expected.R), Is.LessThan(10));
        Assert.That(Math.Abs(actual.G - expected.G), Is.LessThan(10));
        Assert.That(Math.Abs(actual.B - expected.B), Is.LessThan(10));
    }
}
#pragma warning restore CA1416