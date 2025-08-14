using Regira.Drawing.GDI.Services;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;
using System.Drawing;
using GdiColor = System.Drawing.Color;
using GdiImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Drawing.Testing;

#pragma warning disable CA1416
[TestFixture]
public class GDIDrawPositionTests
{
    private readonly string _inputDir;
    private readonly string _outputDir;
    public GDIDrawPositionTests()
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
            var img = GdiUtility.Create(400, 300, null, GdiImageFormat.Png);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(transparentPath);
        }
        var whitePath = Path.Combine(_inputDir, "white-400x300.jpg");
        if (!File.Exists(whitePath))
        {
            var img = GdiUtility.Create(400, 300, "#FFFFFF", GdiImageFormat.Jpeg);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(whitePath);
        }
        var yellowPath = Path.Combine(_inputDir, "yellow-200x150.jpg");
        if (!File.Exists(yellowPath))
        {
            var img = GdiUtility.Create(200, 150, "#FFFF00", GdiImageFormat.Jpeg);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(yellowPath);
        }
        var redPath = Path.Combine(_inputDir, "red-150x100.jpg");
        if (!File.Exists(redPath))
        {
            var img = GdiUtility.Create(150, 100, "#FF0000", GdiImageFormat.Jpeg);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(redPath);
        }
        var greenPath = Path.Combine(_inputDir, "green-50x100.jpg");
        if (!File.Exists(greenPath))
        {
            var img = GdiUtility.Create(50, 100, "#00FF00", GdiImageFormat.Jpeg);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(greenPath);
        }
        var bluePath = Path.Combine(_inputDir, "blue-50x50.jpg");
        if (!File.Exists(bluePath))
        {
            var img = GdiUtility.Create(50, 50, "#0000FF", GdiImageFormat.Jpeg);
            await img.ToImageFile(GdiImageFormat.Jpeg).SaveAs(bluePath);
        }
    }

    [Test]
    public async Task AddImage_No_Params()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg")
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-no-params.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(190, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(210, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(200, 160));
    }

    // Positioned
    [Test]
    public async Task AddImage_Top_Left()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Top | ImagePosition.Left
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-top-left.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(190, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(210, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(200, 160));
    }
    [Test]
    public async Task AddImage_Bottom_Left()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Bottom | ImagePosition.Left
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-bottom-left.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(10, 160));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(190, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(210, 290));
    }
    [Test]
    public async Task AddImage_Top_Right()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.Top | ImagePosition.Right
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-top-right.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(360, 10));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(390, 90));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(340, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(360, 110));
    }
    [Test]
    public async Task AddImage_Bottom_Right()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.Bottom | ImagePosition.Right
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-bottom-right.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(360, 210));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(390, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(390, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(390, 190));
    }

    // Centered
    [Test]
    public async Task AddImage_Top_HCenter()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Top | ImagePosition.HCenter
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-top-hcenter.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(110, 10));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(290, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(200, 160));
    }
    [Test]
    public async Task AddImage_Bottom_HCenter()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Bottom | ImagePosition.HCenter
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-bottom-hcenter.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(110, 160));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(290, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(90, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(310, 290));
    }
    [Test]
    public async Task AddImage_Left_VCenter()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.VCenter | ImagePosition.Left
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-right-vcenter.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(10, 110));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(10, 190));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 90));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(60, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 290));
    }
    [Test]
    public async Task AddImage_Right_VCenter()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.VCenter | ImagePosition.Right
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-right-vcenter.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(360, 110));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(390, 190));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(390, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(340, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(390, 290));
    }
    [Test]
    public async Task AddImage_Middle()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.VCenter | ImagePosition.HCenter
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-middle.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(110, 85));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(200, 150));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(290, 175));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 200));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(90, 150));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(310, 150));
    }

    // Absolute
    [Test]
    public async Task AddImage_Absolute_Top_Left()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Absolute,
            Top = 50,
            Left = 50
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-abs-top-left.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 22), ((Bitmap)testImage).GetPixel(60, 60));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(240, 190));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(40, 40));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(260, 210));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(390, 290));
    }
    [Test]
    public async Task AddImage_Absolute_Top_Right()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Absolute,
            Top = 50,
            Right = 50
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-abs-top-right.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(160, 60));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(240, 190));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(140, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(140, 60));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(360, 60));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(360, 190));
    }
    [Test]
    public async Task AddImage_Absolute_Bottom_Left()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Absolute,
            Bottom = 50,
            Left = 50
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-abs-bottom-left.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(60, 160));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(240, 240));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(40, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(260, 260));
    }
    [Test]
    public async Task AddImage_Absolute_Bottom_Right()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Absolute,
            Bottom = 50,
            Right = 50
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-abs-bottom-right.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(160, 160));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(240, 240));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(140, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(360, 260));
    }

    // Margins
    [Test]
    public async Task AddImage_Margin10()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Margin = 10
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-margin10.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(20, 20));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(190, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(0, 0));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(220, 170));
    }
    [Test]
    public async Task AddImage_Top_Left_Margin10()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Top | ImagePosition.Left,
            Margin = 10
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-top-left-margin10.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(20, 20));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(190, 140));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(0, 0));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(220, 170));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(220, 170));
    }
    [Test]
    public async Task AddImage_Bottom_Left_Margin10()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("yellow-200x150.jpg"),
            Position = ImagePosition.Bottom | ImagePosition.Left,
            Margin = 10
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-bottom-left-margin10.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(20, 150));
        AssertColor(GdiColor.FromArgb(255, 255, 255, 0), ((Bitmap)testImage).GetPixel(200, 280));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 130));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(220, 290));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(0, 299));
    }
    [Test]
    public async Task AddImage_Top_Right_Margin10()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.Top | ImagePosition.Right,
            Margin = 10
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-top-right-margin10.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(350, 20));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(380, 100));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(330, 20));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(380, 120));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(399, 0));
    }
    [Test]
    public async Task AddImage_Bottom_Right_Margin10()
    {
        using var target = await ReadImage("white-400x300.jpg");
        var imgService = new ImageService();
        var imgToAdd = new ImageToAdd
        {
            Image = await ReadImage("green-50x100.jpg"),
            Position = ImagePosition.Bottom | ImagePosition.Right,
            Margin = 10
        };

        using var resultImg = imgService.Draw([imgToAdd], target);
        _ = await resultImg.SaveAs(Path.Combine(_outputDir, "add-image-bottom-right-margin10.jpg"));
        using var testImage = resultImg.ToBitmap();

        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(350, 200));
        AssertColor(GdiColor.FromArgb(255, 0, 255, 0), ((Bitmap)testImage).GetPixel(380, 280));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(10, 280));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(330, 10));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(380, 180));
        AssertColor(GdiColor.White, ((Bitmap)testImage).GetPixel(399, 299));
    }


    protected void AssertColor(GdiColor expected, GdiColor actual)
    {
        Assert.That(Math.Abs(actual.R - expected.R), Is.LessThan(10));
        Assert.That(Math.Abs(actual.G - expected.G), Is.LessThan(10));
        Assert.That(Math.Abs(actual.B - expected.B), Is.LessThan(10));
    }

    protected async Task<IImageFile> ReadImage(string filename)
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(_inputDir, filename));
        return bytes.ToBinaryFile().ToImageFile();
    }
}
#pragma warning restore CA1416