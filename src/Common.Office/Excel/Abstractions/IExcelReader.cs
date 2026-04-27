using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelReader
{
    Task<IEnumerable<ExcelSheet>> Read(IBinaryFile input, string[]? headers = null, CancellationToken cancellationToken = default);
}
public interface IExcelReader<T>
    where T : class
{
    Task<IEnumerable<ExcelSheet<T>>> Read(IBinaryFile input, string[]? headers = null, CancellationToken cancellationToken = default);
}