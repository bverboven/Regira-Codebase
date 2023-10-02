using Moq;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.FileSystem;
using Regira.IO.Storage.Helpers;

namespace IO.Testing.FileSystem;

[TestFixture]
public class FileNameTests
{
    [TestCase("image.jpeg", "image.jpeg")]
    [TestCase("in\"v<a>li|d.jpeg", "in_v_a_li_d.jpeg")]
    [TestCase("in\"v<a>li|d\\image.jpeg", "in_v_a_li_d\\image.jpeg")]
    [TestCase("CLOCK$\\image.jpeg", "_XXX_\\image.jpeg")]
    [TestCase("COM5\\LPT5\\image.jpeg", "_XXX_\\_XXX_\\image.jpeg")]
    [TestCase("xCOM5\\xLPT5\\image.jpeg", "xCOM5\\xLPT5\\image.jpeg")]
    public void Sanitize_FileName(string input, string? expected)
    {
        var sanitized = FileNameUtility.SanitizeFilename(input);
        Assert.That(sanitized, Is.EqualTo(expected));
    }

    [Test]
    public async Task Next_Available_FileName()
    {
        var file = "folder/file.jpg";
        var fileService = new Mock<IFileService>();
        fileService.Setup(l => l.Exists(file)).ReturnsAsync(false);

        var fh = new FileNameHelper(fileService.Object);
        var identifier = await fh.NextAvailableFileName(file);

        Assert.That(identifier, Is.EqualTo(file));
    }
    [Test]
    public async Task Next_Available_FileName_Increase_Number()
    {
        var file1 = "folder/file.jpg";
        var file2 = "folder/file-(2).jpg";
        var file3 = "folder/file-(3).jpg";
        var fileService = new Mock<IFileService>();
        fileService.Setup(l => l.Exists(file1)).ReturnsAsync(true);
        fileService.Setup(l => l.Exists(file2)).ReturnsAsync(true);
        fileService.Setup(l => l.Exists(file3)).ReturnsAsync(false);

        var fh = new FileNameHelper(fileService.Object);
        var identifier = await fh.NextAvailableFileName(file1);

        Assert.That(identifier, Is.EqualTo(file3));
    }
    [Test]
    public async Task Next_Available_FileName_With_Custom_NumberPattern()
    {
        var file1 = "folder/file.jpg";
        var file2 = "folder/file 2.jpg";
        var file3 = "folder/file 3.jpg";
        var fileService = new Mock<IFileService>();
        fileService.Setup(l => l.Exists(file1)).ReturnsAsync(true);
        fileService.Setup(l => l.Exists(file2)).ReturnsAsync(true);
        fileService.Setup(l => l.Exists(file3)).ReturnsAsync(false);

        var fh = new FileNameHelper(fileService.Object, new FileNameHelper.Options { NumberPattern = @" {0}" });
        var identifier = await fh.NextAvailableFileName(file1);

        Assert.That(identifier, Is.EqualTo(file3));
    }
}