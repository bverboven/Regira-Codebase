using Regira.IO.Abstractions;

namespace Regira.Office.Excel.Abstractions;

public interface IExcelReader
{
    IEnumerable<ExcelSheet> Read(IBinaryFile input, string[]? headers = null);
}