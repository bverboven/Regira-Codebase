using Regira.IO.Abstractions;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfTextService : IPdfTextExtractor
{
    IList<string> GetTextPerPage(IBinaryFile pdf);
    IMemoryFile? RemoveEmptyPages(IBinaryFile pdf);
}