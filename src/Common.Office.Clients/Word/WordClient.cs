using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.Clients.Abstractions;
using Regira.Office.Clients.Models;
using Regira.Office.Models;
using Regira.Office.Word.Abstractions;
using Regira.Office.Word.Models;

namespace Regira.Office.Clients.Word;

public class WordClient(HttpClient client) : OfficeClientBase(client),
    IWordCreator, IWordConverter, IWordMerger, IWordTextExtractor
{
    private const string CreatePath = "/word/create";
    private const string ConvertPath = "/word/convert";
    private const string MergePath = "/word/merge";
    private const string TextPath = "/word/txt";

    public async Task<IMemoryFile> Create(WordTemplateInput input, CancellationToken cancellationToken = default)
    {
        var dto = MapToDocumentModel(input);
        return await PostJsonForFileAsync(CreatePath, dto, cancellationToken);
    }

    public async Task<IMemoryFile> Convert(WordTemplateInput input, FileFormat format, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.", nameof(input))), "file", "template.docx");
        return await PostMultipartForFileAsync($"{ConvertPath}?OutputFormat={format}", content, cancellationToken);
    }

    public async Task<IMemoryFile> Convert(WordTemplateInput input, ConversionOptions options, CancellationToken cancellationToken = default)
    {
        var url = BuildConvertUrl(options);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.", nameof(input))), "file", "template.docx");
        return await PostMultipartForFileAsync(url, content, cancellationToken);
    }

    public async Task<IMemoryFile> Merge(IEnumerable<WordTemplateInput> inputs, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        foreach (var input in inputs)
            content.Add(new ByteArrayContent(input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.")), "files", "document.docx");
        return await PostMultipartForFileAsync(MergePath, content, cancellationToken);
    }

    public async Task<string> GetText(WordTemplateInput input, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.", nameof(input))), "file", "document.docx");
        return await PostMultipartForTextAsync(TextPath, content, cancellationToken) ?? string.Empty;
    }

    private static WordDocumentModel MapToDocumentModel(WordTemplateInput input) => new()
    {
        TemplateBytes = input.Template.GetBytes() ?? throw new ArgumentException("Template has no content.", nameof(input)),
        GlobalParameters = input.GlobalParameters,
        CollectionParameters = input.CollectionParameters,
        Images = input.Images?.Select(img => new WordImageModel
        {
            Name = img.Name,
            Bytes = img.File?.GetBytes() ?? []
        }).ToList(),
        Headers = input.Headers?.Select(h => new WordHeaderFooterModel { Template = MapToDocumentModel(h.Template), Type = h.Type }).ToList(),
        Footers = input.Footers?.Select(f => new WordHeaderFooterModel { Template = MapToDocumentModel(f.Template), Type = f.Type }).ToList()
    };

    private static string BuildConvertUrl(ConversionOptions options)
    {
        var parts = new List<string> { $"OutputFormat={options.OutputFormat}" };
        if (options.Settings?.PageSize != null) parts.Add($"PageSize={options.Settings.PageSize}");
        if (options.AutoScaleTables) parts.Add("AutoScaleTables=true");
        if (options.AutoScalePictures) parts.Add("AutoScalePictures=true");
        return $"{ConvertPath}?{string.Join("&", parts)}";
    }
}
