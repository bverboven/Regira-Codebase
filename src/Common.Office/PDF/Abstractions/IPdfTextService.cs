using Regira.IO.Abstractions;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfTextService : IPdfTextExtractor
{
    IList<string> GetTextPerPage(IMemoryFile pdf);
    IMemoryFile? RemoveEmptyPages(IMemoryFile pdf);
}