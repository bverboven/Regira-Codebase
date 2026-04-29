namespace Regira.Office.Excel.Models.DTO;

public static class DtoExtensions
{
    public static ExcelSheetInputDto ToExcelSheetInputDto(this ExcelSheet input)
        => new()
        {
            Name = input.Name,
            Data = input.Data?.Select(item => item as IDictionary<string, object?> ?? new Dictionary<string, object?> { ["value"] = item }).ToList() ?? []
        };
}