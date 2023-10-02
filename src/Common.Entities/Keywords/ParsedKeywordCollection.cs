namespace Regira.Entities.Keywords;

public class ParsedKeywordCollection : List<QKeyword>
{
    public string? Input { get; }
    public ParsedKeywordCollection(IEnumerable<QKeyword> items, string? input)
        : base(items)
    {
        Input = input;
    }
}