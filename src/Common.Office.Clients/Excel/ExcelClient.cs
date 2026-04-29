using Regira.IO.Abstractions;
using Regira.Office.Clients.Abstractions;
using Regira.Office.Clients.Models;
using Regira.Office.Excel;
using Regira.Office.Excel.Abstractions;

namespace Regira.Office.Clients.Excel;

public class ExcelClient(HttpClient client) : OfficeClientBase(client), IExcelService
{
    private const string CreatePath = "/xlsx/create";
    private const string ReadPath = "/xlsx/read";

    public async Task<IMemoryFile> Create(IEnumerable<ExcelSheet> sheets, CancellationToken cancellationToken = default)
    {
        var dto = sheets.Select(s => new ExcelSheetInputDto
        {
            Name = s.Name,
            Data = (s.Data?.Select(item => item as IDictionary<string, object?> ?? new Dictionary<string, object?> { ["value"] = item }).ToList()
                    ?? [])!
        }).ToList();
        return await PostJsonForFileAsync(CreatePath, dto, cancellationToken);
    }

    public async Task<IEnumerable<ExcelSheet>> Read(IBinaryFile input, string[]? headers = null, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(GetBytes(input)), "xlsxFile", "data.xlsx");
        return await PostMultipartAsync<IEnumerable<ExcelSheet>>(ReadPath, content, cancellationToken) ?? [];
    }

    private static byte[] GetBytes(IBinaryFile file)
        => (file as IMemoryBytesFile)?.Bytes ?? throw new ArgumentException("File must be an in-memory file with bytes.", nameof(file));
}
