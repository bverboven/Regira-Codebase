using Regira.IO.Abstractions;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfTextExtractor
{
    string GetText(IMemoryFile pdf);
}