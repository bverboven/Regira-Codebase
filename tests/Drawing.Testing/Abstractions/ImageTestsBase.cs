using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.Enums;
using Regira.IO.Extensions;
using Regira.Utilities;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Drawing.Testing.Abstractions;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public abstract class ImageTestsBase
{
    protected readonly IImageService _imageService;
    protected readonly string _inputDir;
    protected readonly string _outputDir;
    protected ImageTestsBase(IImageService imageService)
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _inputDir = Path.Combine(assetsDir, "Input");
        _outputDir = Path.Combine(assetsDir, "Output", imageService.GetType().Assembly.GetName().Name!.Split('.').Last());
        Directory.CreateDirectory(_outputDir);
        _imageService = imageService;
    }


    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    public async Task To_Png(string filename)
    {
        using var image = await ReadImage(filename);
        var pngImg = _imageService.ChangeFormat(image, ImageFormat.Png);

        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}.png", pngImg);
    }

    [TestCase("img-5.png")]
    [TestCase("img-6.png")]
    public async Task To_Jpeg(string filename)
    {
        using var image = await ReadImage(filename);
        var pngImg = _imageService.ChangeFormat(image, ImageFormat.Jpeg);

        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}.jpg", pngImg);
    }

    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    public async Task Resize(string filename)
    {
        using var image = await ReadImage(filename);
        var original = image.Size;
        var wantedSize = new Size2D(1024, 1024);
        using var resized = _imageService.Resize(image, wantedSize, 60);

        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}-resized.jpg", resized);

        Assert.AreEqual(image.Size!.Value.Width, original!.Value.Width);
        Assert.AreEqual(image.Size!.Value.Height, original.Value.Height);
        Assert.IsTrue(resized.Size!.Value.Width <= wantedSize.Width);
        Assert.IsTrue(resized.Size!.Value.Height <= wantedSize.Height);
    }

    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    public async Task ResizeFixed(string filename)
    {
        using var image = await ReadImage(filename);
        var original = image.Size;
        var targetSize = new Size2D(1024, 1024);
        using var resized = _imageService.ResizeFixed(image, targetSize);

        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}-resized-fixed.jpg", resized);

        Assert.AreEqual(image.Size!.Value.Width, original!.Value.Width);
        Assert.AreEqual(image.Size!.Value.Height, original.Value.Height);
        Assert.AreEqual(resized.Size!.Value.Width, targetSize.Width);
        Assert.AreEqual(resized.Size!.Value.Height, targetSize.Height);
    }

    [Test]
    public async Task RotateImage90Right()
    {
        using var image = await ReadImage("img-1.jpg");
        using var rotatedR = _imageService.Rotate(image, 90);
        Assert.AreEqual(image.Size!.Value.Width, rotatedR.Size!.Value.Height);
        Assert.AreEqual(image.Size!.Value.Height, rotatedR.Size!.Value.Width);

        //var target90R = Path.Combine(_assetsDir, "Output", "img-1-90right.jpg");
        //rotatedR.Save(target90R);
        await WriteOutput("img-1-90right.jpg", rotatedR);
    }
    [Test]
    public async Task RotateImage90Left()
    {
        using var image = await ReadImage("img-1.jpg");
        using var rotatedL = _imageService.Rotate(image, -90);
        Assert.AreEqual(image.Size!.Value.Width, rotatedL.Size!.Value.Height);
        Assert.AreEqual(image.Size!.Value.Height, rotatedL.Size!.Value.Width);

        //var target90L = Path.Combine(_assetsDir, "Output", "img-1-90left.jpg");
        //rotatedL.Save(target90L);
        await WriteOutput("img-1-90left.jpg", rotatedL);
    }

    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    public async Task CropRect(string filename)
    {
        var src = Path.Combine(_inputDir, filename);
        using var image = await ReadImage(src);

        var halfWidth = (int)image.Size!.Value.Width / 2;
        var halfHeight = (int)image.Size!.Value.Height / 2;

        var rectangles = new Dictionary<string, int[]>()
        {
            ["topleft"] = new[] { 0, 0, halfWidth, halfHeight },
            ["topright"] = new[] { halfWidth, 0, halfWidth, halfHeight },
            ["bottomleft"] = new[] { 0, halfHeight, halfWidth, halfHeight },
            ["bottomright"] = new[] { halfWidth, halfHeight, halfWidth, halfHeight }
        };

        foreach (var rectangle in rectangles)
        {
            using var cropped = _imageService.CropRectangle(image, rectangle.Value);
            await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}-{rectangle.Key}{Path.GetExtension(filename)}", cropped);
        }
    }

    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    [TestCase("img-5.png")]
    [TestCase("img-6.png")]
    [TestCase("thumbs-up.jpg")]
    public async Task MakeTransparent(string filename)
    {
        var src = Path.Combine(_inputDir, filename);
        using var image = await ReadImage(src);

        using var rgbImg = _imageService.MakeTransparent(image, new[] { 225, 225, 225 });
        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}-transparent.png", rgbImg);
    }
    
    [TestCase("img-1.jpg")]
    [TestCase("img-2.jpg")]
    [TestCase("img-3.jpg")]
    [TestCase("img-4.jpg")]
    [TestCase("img-5.png")]
    [TestCase("img-6.png")]
    [TestCase("thumbs-up.jpg")]
    public async Task RemoveAlpha(string filename)
    {
        var src = Path.Combine(_inputDir, filename);
        using var image = await ReadImage(src);

        using var rgbImg = _imageService.RemoveAlpha(image);
        await WriteOutput($"{Path.GetFileNameWithoutExtension(filename)}-rgb.png", rgbImg);
    }


    protected async Task<IImageFile> ReadImage(string filename)
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(_inputDir, filename));
        return _imageService.Parse(bytes)!;
    }
    protected Task WriteOutput(string filename, IImageFile img)
        => File.WriteAllBytesAsync(Path.Combine(_outputDir, filename), img.GetBytes()!);
}