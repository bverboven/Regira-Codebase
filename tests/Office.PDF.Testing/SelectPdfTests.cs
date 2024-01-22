using NUnit.Framework.Legacy;
using Regira.IO.Extensions;
using Regira.IO.Utilities;
using Regira.Office.PDF.Models;
using Regira.Office.PDF.SelectPdf;
using Regira.Serializing.Newtonsoft.Json;
using Regira.Utilities;
using Regira.Web.HTML;

namespace Office.PDF.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SelectPdfTests
{
    private readonly string _tableContent;
    private readonly string _loremIpsumContent;
    private readonly string _regiraLogoPath;
    private readonly HtmlTemplateParser _htmlParser;
    private readonly string _inputDir;
    private readonly string _outputDir;
    public SelectPdfTests()
    {
        _htmlParser = new HtmlTemplateParser(new JsonSerializer());

        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        var assetsDir = Path.Combine(assemblyDir, "../../../", "Assets");
        _inputDir = Path.Combine(assetsDir, "Input");
        _outputDir = Path.Combine(assetsDir, "Output", "SelectPdf");
        Directory.CreateDirectory(_outputDir);
        _tableContent = File.ReadAllText(Path.Combine(assetsDir, "Input", "table.html"));
        _loremIpsumContent = File.ReadAllText(Path.Combine(assetsDir, "Input", "lorem-ipsum.html"));
        _regiraLogoPath = Path.Combine(assetsDir, "Input", "regira-logo.png");
    }

    [Test]
    public void CreatePdf()
    {
        var html = _htmlParser.Parse(_tableContent, (object?)null).Result;

        // ReSharper disable once RedundantArgumentDefaultValue
        var marginPoints = DimensionsUtility.MmToPt(10f, DimensionsUtility.DPI.DEFAULT);
        var pdfPrinter = new PdfManager();
        var template = new HtmlInput
        {
            HtmlContent = html,
            Margins = marginPoints
        };
        using var pdf = pdfPrinter.Create(template);
        using var stream = pdf.GetStream()!;
        ClassicAssert.IsNotNull(stream);
        ClassicAssert.IsTrue(stream.Length > 0);
        var outputPath = Path.Combine(_outputDir, "pdf-table.pdf");
        File.WriteAllBytes(outputPath, FileUtility.GetBytes(stream)!);
    }
    [Test]
    public void FillParameters()
    {
        var date = DateTime.Now;
        var logoBytes = File.ReadAllBytes(_regiraLogoPath);
        var logoBase64 = $"data:image/png;base64,{FileUtility.GetBase64String(logoBytes)}";
        var parameters = new
        {
            title = "Test Title",
            date = date.ToString("dd/MM/yyyy"),
            lijnen = Enumerable.Repeat((object?)null, 25).Select((_, i) => new { title = $"Item {char.ConvertFromUtf32(i + 65)}" }).ToList(),
            logo = logoBase64
        };

        var html = _htmlParser.Parse(_loremIpsumContent, parameters).Result;

        var template = new HtmlInput
        {
            HtmlContent = html
        };

        var pdfPrinter = new PdfManager();
        using var pdf = pdfPrinter.Create(template);
        using var stream = pdf.GetStream()!;
        ClassicAssert.IsNotNull(stream);
        ClassicAssert.IsTrue(stream.Length > 0);
        var outputPath = Path.Combine(_outputDir, "pdf-with-parameters.pdf");
        File.WriteAllBytes(outputPath, FileUtility.GetBytes(stream)!);
    }
    [Test]
    public void With_Header()
    {
        var date = DateTime.Now;
        var logoBytes = File.ReadAllBytes(_regiraLogoPath);
        var logoBase64 = $"data:image/png;base64,{FileUtility.GetBase64String(logoBytes)}";
        var parameters = new
        {
            title = "Test Title",
            date = date.ToString("dd/MM/yyyy"),
            lijnen = Enumerable.Repeat((object?)null, 25).Select((_, i) => new { title = $"Item {char.ConvertFromUtf32(i + 65)}" }).ToList(),
            logo = logoBase64
        };

        var html = _htmlParser.Parse(_loremIpsumContent, parameters).Result;
        var headerHtml = _htmlParser.Parse(File.ReadAllText(Path.Combine(_inputDir, "header.html")), parameters).Result;

        var template = new HtmlInput
        {
            HtmlContent = html,
            HeaderHtmlContent = headerHtml,
            HeaderHeight = 10
        };

        var pdfPrinter = new PdfManager();
        using var pdf = pdfPrinter.Create(template);
        using var stream = pdf.GetStream()!;
        ClassicAssert.IsNotNull(stream);
        ClassicAssert.IsTrue(stream.Length > 0);
        var outputPath = Path.Combine(_outputDir, "pdf-with-header.pdf");
        File.WriteAllBytes(outputPath, FileUtility.GetBytes(stream)!);
    }
}