namespace Regira.Office.OCR.Models.DTO;

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