using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.Clients.Abstractions;
using Regira.Office.Clients.Models;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.Clients.PDF;

public class PdfClient(HttpClient client) : OfficeClientBase(client),
    IHtmlToPdfService, IImagesToPdfService, IPdfMerger, IPdfSplitter, IPdfTextExtractor, IPdfToImageService
{
    private const string FromHtmlPath = "/pdf/from-html";
    private const string FromImagesPath = "/pdf/from-images";
    private const string ToImagesPath = "/pdf/to-images";
    private const string MergePath = "/pdf/merge";
    private const string SplitPath = "/pdf/split";
    private const string TextPath = "/pdf/txt";
    private const string PageCountPath = "/pdf/page-count";

    public async Task<IMemoryFile> Create(HtmlInput template, CancellationToken cancellationToken = default)
    {
        var dto = new HtmlToPdfInput
        {
            HtmlContent = template.HtmlContent ?? string.Empty,
            Orientation = template.Orientation,
            Format = template.Format,
            Margins = [template.Margins.Top, template.Margins.Right, template.Margins.Bottom, template.Margins.Left],
            HeaderHtmlContent = template.HeaderHtmlContent,
            HeaderHeight = template.HeaderHeight ?? 0,
            FooterHtmlContent = template.FooterHtmlContent,
            FooterHeight = template.FooterHeight ?? 0
        };
        return await PostJsonForFileAsync(FromHtmlPath, dto, cancellationToken);
    }

    public async Task<IMemoryFile?> ImagesToPdf(ImagesInput input, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var i = 0;
        foreach (var imageBytes in input.Images)
            content.Add(new ByteArrayContent(imageBytes), "images", $"image{i++}.jpg");
        return await PostMultipartForFileAsync(FromImagesPath, content, cancellationToken);
    }

    public async Task<IMemoryFile?> Merge(IEnumerable<IMemoryFile> items, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var i = 0;
        foreach (var file in items)
            content.Add(new ByteArrayContent(file.GetBytes() ?? throw new ArgumentException("File has no content.")), "files", $"file{i++}.pdf");
        return await PostMultipartForFileAsync(MergePath, content, cancellationToken);
    }

    public async Task<IEnumerable<IMemoryFile>> Split(IMemoryFile pdf, IEnumerable<PdfSplitRange> ranges, CancellationToken cancellationToken = default)
    {
        var rangesParam = string.Join(",", ranges.Select(r => r.End.HasValue ? $"{r.Start}-{r.End}" : $"{r.Start}"));
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdf.GetBytes() ?? throw new ArgumentException("File has no content.", nameof(pdf))), "pdfFile", "file.pdf");
        var files = await PostMultipartForFilesAsync($"{SplitPath}?ranges={rangesParam}", content, cancellationToken);
        return files;
    }

    public async Task<int> GetPageCount(IMemoryFile pdf, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdf.GetBytes() ?? throw new ArgumentException("File has no content.", nameof(pdf))), "files", "file.pdf");
        var counts = await PostMultipartAsync<int[]>(PageCountPath, content, cancellationToken);
        return counts?.FirstOrDefault() ?? 0;
    }

    public async Task<string> GetText(IMemoryFile pdf, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdf.GetBytes() ?? throw new ArgumentException("File has no content.", nameof(pdf))), "files", "file.pdf");
        return await PostMultipartForTextAsync(TextPath, content, cancellationToken) ?? string.Empty;
    }

    public async Task<IList<IImageFile>> ToImages(IMemoryFile pdf, PdfToImagesOptions? options = null, CancellationToken cancellationToken = default)
    {
        var url = BuildToImagesUrl(options);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdf.GetBytes() ?? throw new ArgumentException("File has no content.", nameof(pdf))), "pdfFile", "file.pdf");
        var files = await PostMultipartForFilesAsync(url, content, cancellationToken);
        return files.Cast<IImageFile>().ToList();
    }

    private static string BuildToImagesUrl(PdfToImagesOptions? options)
    {
        if (options == null) return ToImagesPath;
        var parts = new List<string>();
        if (options.Size?.Width > 0) parts.Add($"Width={options.Size.Value.Width}");
        if (options.Size?.Height > 0) parts.Add($"Height={options.Size.Value.Height}");
        parts.Add($"Format={options.Format}");
        return parts.Count == 0 ? ToImagesPath : $"{ToImagesPath}?{string.Join("&", parts)}";
    }
}
