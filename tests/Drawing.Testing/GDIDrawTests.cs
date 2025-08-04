using Regira.Drawing.GDI.Helpers;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;
using System.Drawing;
using System.Drawing.Imaging;

namespace Drawing.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class GDIDrawTests
{
    protected readonly string _inputDir;
    protected readonly string _outputDir;
    public GDIDrawTests()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _inputDir = Path.Combine(assetsDir, "Input");
        _outputDir = Path.Combine(assetsDir, "Output", typeof(GdiUtility).Assembly.GetName().Name!.Split('.').Last());
        Directory.CreateDirectory(_outputDir);
    }

    [SetUp]
    public async Task Setup()
    {
        var transparentPath = Path.Combine(_inputDir, "transparent-400x300.jpg");
        if (!File.Exists(transparentPath))
        {
            var img = GdiUtility.Create(400, 300, null, ImageFormat.Png);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(transparentPath);
        }
        var whitePath = Path.Combine(_inputDir, "white-400x300.jpg");
        if (!File.Exists(whitePath))
        {
            var img = GdiUtility.Create(400, 300, "#FFFFFF", ImageFormat.Jpeg);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(whitePath);
        }
        var yellowPath = Path.Combine(_inputDir, "yellow-200x150.jpg");
        if (!File.Exists(yellowPath))
        {
            var img = GdiUtility.Create(200, 150, "#FFFF00", ImageFormat.Jpeg);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(yellowPath);
        }
        var redPath = Path.Combine(_inputDir, "red-150x100.jpg");
        if (!File.Exists(redPath))
        {
            var img = GdiUtility.Create(150, 100, "#FF0000", ImageFormat.Jpeg);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(redPath);
        }
        var greenPath = Path.Combine(_inputDir, "green-50x100.jpg");
        if (!File.Exists(greenPath))
        {
            var img = GdiUtility.Create(50, 100, "#00FF00", ImageFormat.Jpeg);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(greenPath);
        }
        var bluePath = Path.Combine(_inputDir, "blue-50x50.jpg");
        if (!File.Exists(bluePath))
        {
            var img = GdiUtility.Create(50, 50, "#0000FF", ImageFormat.Jpeg);
            await img.ToImageFile(ImageFormat.Jpeg).SaveAs(bluePath);
        }
    }

    [Test]
    public async Task AddImage()
    {

        using var target = await ReadImage("white-400x300.jpg");
        var builder = new ImageBuilder();
        builder.SetTarget(target);
        builder.Add(new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg")
        });

        var resultImg = builder.Build();
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image.jpg"));
        var testImage = resultImg.ToBitmap();
        
        var yellowPx = ((Bitmap)testImage).GetPixel(10, 10);
        Assert.That(yellowPx.R, Is.EqualTo(255));
        Assert.That(yellowPx.G, Is.EqualTo(255));
        Assert.That(yellowPx.B, Is.EqualTo(1));// Blue seems to be 1 instead of 0

        var whitePx = ((Bitmap)testImage).GetPixel(210, 160);
        Assert.That(whitePx.R, Is.EqualTo(255));
        Assert.That(whitePx.G, Is.EqualTo(255));
        Assert.That(whitePx.B, Is.EqualTo(255));
    }


    protected async Task<IImageFile> ReadImage(string filename)
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(_inputDir, filename));
        return bytes.ToBinaryFile().ToImageFile();
    }
}