using Regira.IO.Abstractions;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfEditor : IPdfMerger, IPdfSplitter
{
    IMemoryFile? RemovePages(IMemoryFile pdf, IEnumerable<int> pages);
}