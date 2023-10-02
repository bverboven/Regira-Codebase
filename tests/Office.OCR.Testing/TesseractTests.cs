using Regira.IO.Models;
using Regira.Office.OCR.Tesseract;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Office.OCR.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TesseractTests
{
    private readonly string _assetsDir;
    public TesseractTests()
    {
        var assetsDir = Path.Combine(AppContext.BaseDirectory, "../../../Assets");
        _assetsDir = new DirectoryInfo(assetsDir).FullName;
    }

    [Test]
    public async Task Read_English()
    {
        var path = Path.Combine(_assetsDir, "poem-en.jpg");
        var img = new BinaryFileItem
        {
            Bytes = await File.ReadAllBytesAsync(path)
        };
        var mgr = new OcrManager(new OcrManager.Options { Language = "eng" });
        var content = (await mgr.Read(img))?.ReplaceLineEndings();
        Assert.IsNotNull(content);

        var contentLines = content?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var expected = @"MOTHER'S DAY POEM
Your arms were always open
when I needed a hug.
Your heart understood
when I needed a friend.
Your gentle eyes were stern
when I needed a lesson.
Your strength and love has guided me
and gave me wings to fly.".Split(Environment.NewLine);

        CollectionAssert.IsNotEmpty(contentLines);
        Assert.AreEqual(string.Join(Environment.NewLine, expected), string.Join(Environment.NewLine, contentLines!));
    }

    [Test]
    public async Task Read_Dutch()
    {
        var path = Path.Combine(_assetsDir, "poem-nl.jpg");
        var img = new BinaryFileItem
        {
            Bytes = await File.ReadAllBytesAsync(path)
        };
        var mgr = new OcrManager(new OcrManager.Options { Language = "nld" });
        var content = (await mgr.Read(img))?.ReplaceLineEndings();
        Assert.IsNotNull(content);

        var contentLines = content?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var expected = @"je gaf mij het leven
ving mijn tranen
deelde mijn lach
daarom wil ik stilstaan
bij alles wat je bent en doet
ik zet jou in het zonnetje
op deze mooie dag
little universe".Split(Environment.NewLine);

        CollectionAssert.IsNotEmpty(contentLines);
        Assert.AreEqual(string.Join(Environment.NewLine, expected), string.Join(Environment.NewLine, contentLines!));
    }
}