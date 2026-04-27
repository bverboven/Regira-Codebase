using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelWriter
{
    Task<IMemoryFile> Create(IEnumerable<ExcelSheet> sheets, CancellationToken cancellationToken = default);
}
public interface IExcelWriter<T>
    where T : class
{
    Task<IMemoryFile> Create(IEnumerable<ExcelSheet<T>> sheets, CancellationToken cancellationToken = default);
}