using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.MimeTypes;
using Regira.Office.Word.Models;
using Spire.Doc;
using RegiraFileFormat = Regira.Office.Models.FileFormat;

namespace Regira.Office.Word.Spire;

public class DocumentBuilder
{
    private readonly WordManager _manager;
    private WordDocumentSettings? _settings;
    private WordTemplateInput[]? _inputs;
    private ConversionOptions? _conversionOptions;
    private IEnumerable<Paragraph>? _paragraphs;
    private ICollection<WordHeaderFooterInput>? _headers;
    private ICollection<WordHeaderFooterInput>? _footers;

    public DocumentBuilder(WordManager manager)
    {
        _manager = manager;
    }


    public DocumentBuilder Load(params WordTemplateInput[] inputs)
    {
        _inputs = inputs;
        return this;
    }
    public DocumentBuilder WithSettings(WordDocumentSettings settings)
    {
        _settings = settings;
        return this;
    }
    public DocumentBuilder WithParagraphs(IEnumerable<Paragraph> paragraphs)
    {
        _paragraphs = paragraphs;
        return this;
    }
    public DocumentBuilder AddHeader(WordHeaderFooterInput header)
    {
        _headers ??= new List<WordHeaderFooterInput>();
        _headers.Add(header);
        return this;
    }
    public DocumentBuilder AddFooter(WordHeaderFooterInput footer)
    {
        _footers ??= new List<WordHeaderFooterInput>();
        _footers.Add(footer);
        return this;
    }
    public DocumentBuilder WithConversion(ConversionOptions options)
    {
        _conversionOptions = options;
        return this;
    }


    public IMemoryFile Build()
    {
        // Create Document
        using var doc = _inputs != null
            ? _manager.MergeDocuments(_inputs)
            : new Document();

        // PageSettings
        if (_settings != null)
        {
            if (doc.Sections.Count == 0)
            {
                doc.AddSection();
            }
            foreach (Section section in doc.Sections)
            {
                section.PageSetup.PageSize = _manager.GetPageSize(_settings.PageSize);
                section.PageSetup.Orientation = _manager.GetPageOrientation(_settings.PageOrientation);
            }
        }

        // Paragraphs
        if (_paragraphs?.Any() ?? false)
        {
            _manager.AddParagraphs(doc, _paragraphs);
        }

        // Headers
        if (_headers?.Any() ?? false)
        {
            foreach (var headerInput in _headers)
            {
                using var headerDoc = _manager.CreateDocument(headerInput.Template);
                _manager.AddHeader(doc, headerDoc, headerInput.Type);
            }
        }
        // Footers
        if (_footers?.Any() ?? false)
        {
            foreach (var footerInput in _footers)
            {
                using var footerDoc = _manager.CreateDocument(footerInput.Template);
                _manager.AddFooter(doc, footerDoc, footerInput.Type);
            }
        }

        // ConversionOptions
        _conversionOptions ??= new ConversionOptions();
        var stream = _manager.ConvertDocument(doc, _conversionOptions);
        return stream.ToMemoryFile(_conversionOptions.OutputFormat == RegiraFileFormat.Doc ? ContentTypes.DOC : ContentTypes.DOCX);
    }
}