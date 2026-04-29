using Regira.Office.Word.Models;

namespace Regira.Office.Clients.Models;

public class WordHeaderFooterModel
{
    public WordDocumentModel Template { get; set; } = null!;
    public HeaderFooterType Type { get; set; }
}
public class WordDocumentModel
{
    public byte[] TemplateBytes { get; set; } = null!;
    public IDictionary<string, object>? GlobalParameters { get; set; }
    public IDictionary<string, ICollection<IDictionary<string, object>>>? CollectionParameters { get; set; }
    public ICollection<WordImageModel>? Images { get; set; }
    public IDictionary<string, WordDocumentModel>? DocumentParameters { get; set; }
    public ICollection<WordHeaderFooterModel>? Headers { get; set; }
    public ICollection<WordHeaderFooterModel>? Footers { get; set; }
}