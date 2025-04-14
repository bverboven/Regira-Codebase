using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelWriter
{
    IMemoryFile Create(IEnumerable<ExcelSheet> sheets);
}
public interface IExcelWriter<T>
    where T : class
{
    IMemoryFile Create(IEnumerable<ExcelSheet<T>> sheets);
}