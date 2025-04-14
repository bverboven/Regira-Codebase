using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelReader
{
    IEnumerable<ExcelSheet> Read(IBinaryFile input, string[]? headers = null);
}
public interface IExcelReader<T>
    where T : class
{
    IEnumerable<ExcelSheet<T>> Read(IBinaryFile input, string[]? headers = null);
}