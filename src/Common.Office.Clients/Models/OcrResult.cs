namespace Regira.Office.Clients.Models;

public enum Lang
{
    En,
    De,
    Fr,
    Nl,
    Pt,
    Es,
    Sw
}
public class OcrResult
{
    public Lang? Language { get; set; }
    public string? Text { get; set; }
}