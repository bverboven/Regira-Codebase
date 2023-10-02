using Regira.IO.Abstractions;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfMerger
{
    IMemoryFile? Merge(IEnumerable<IBinaryFile> items);
}