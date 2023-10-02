namespace Regira.Office.Excel;

public class ExcelSheet : ExcelSheet<object>
{
}

public class ExcelSheet<T>
{
    public string? Name { get; set; }
    public ICollection<T>? Data { get; set; }
}