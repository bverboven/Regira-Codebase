namespace Regira.Office.Word.Models;

public class WordHeaderFooterInput
{
    public WordTemplateInput Template { get; set; } = null!;
    public HeaderFooterType Type { get; set; } = HeaderFooterType.Default;
}