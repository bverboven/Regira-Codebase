using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelWriter
{
    IMemoryFile Create(ExcelSheet sheet);
    IMemoryFile Create(IEnumerable<ExcelSheet> sheets);
}