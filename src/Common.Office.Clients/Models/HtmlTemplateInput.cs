namespace Regira.Office.Clients.Models;

public class HtmlTemplateInput
{
    public string Template { get; set; } = null!;
    public IDictionary<string, object?>? Parameters { get; set; }
}