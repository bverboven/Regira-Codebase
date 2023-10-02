using Regira.IO.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Abstractions;

public interface IHtmlToPdfService
{
    IMemoryFile Create(HtmlInput template);
}

[Obsolete("Use IHtmlToPdfService instead", true)]
public interface IPrintService
{
    event Action<HtmlInput, string> OnPrint;

    Stream Create(HtmlInput template);
}