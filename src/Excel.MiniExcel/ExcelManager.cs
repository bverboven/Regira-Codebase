using MiniExcelLibs;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.Excel.Abstractions;
using Regira.Office.MimeTypes;

namespace Regira.Office.Excel.MiniExcel;

public class ExcelManager(ExcelManager.Options? options = null) : IExcelManager
{
    private readonly Options _options = options ?? new Options();
    public class Options
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd hh:mm:ss";
    }


    public IEnumerable<ExcelSheet> Read(IBinaryFile input, string[]? headers = null)
    {
        using var ms = input.GetStream();
        var sheetNames = ms.GetSheetNames();

        foreach (var sheetName in sheetNames)
        {
            var rows = ms.Query(sheetName: sheetName)
                .Cast<IDictionary<string, object>>();
            // ReSharper disable once PossibleMultipleEnumeration
            var miniHeaders = rows.First();
            yield return new ExcelSheet
            {
                Name = sheetName,
                // ReSharper disable once PossibleMultipleEnumeration
                Data = rows
                    .Skip(1)
                    .Select(d => d.Keys.ToDictionary(key => miniHeaders[key].ToString()!, key => d[key]) as object)
                    .ToList()
            };
        }
    }

    public IMemoryFile Create(IEnumerable<ExcelSheet> sheets)
    {
        var ms = new MemoryStream();
        var sheetIndex = 0;
        var miniSheets = new Dictionary<string, object>();
        foreach (var sheet in sheets)
        {
            var sheetName = sheet.Name ?? $"Sheet-{++sheetIndex}";
            miniSheets.Add(sheetName, sheet.Data ?? []);
        }

        ms.SaveAs(miniSheets);

        return ms.ToMemoryFile(ContentTypes.XLSX);
    }
}

public class ExcelManager<T> : IExcelManager<T>
    where T : class, new()
{
    public IEnumerable<ExcelSheet<T>> Read(IBinaryFile input, string[]? headers = null)
    {
        using var ms = input.GetStream();
        var sheetNames = ms.GetSheetNames();

        foreach (var sheetName in sheetNames)
        {
            var rows = ms.Query<T>(sheetName: sheetName);
            yield return new ExcelSheet<T>
            {
                Name = sheetName,
                Data = rows.ToList()
            };
        }
    }
    public IMemoryFile Create(IEnumerable<ExcelSheet<T>> sheets)
    {
        var ms = new MemoryStream();
        var sheetIndex = 0;
        var miniSheets = new Dictionary<string, object>();
        foreach (var sheet in sheets)
        {
            var sheetName = sheet.Name ?? $"Sheet-{++sheetIndex}";
            miniSheets.Add(sheetName, sheet.Data ?? []);
        }

        ms.SaveAs(miniSheets);

        return ms.ToMemoryFile(ContentTypes.XLSX);
    }
}