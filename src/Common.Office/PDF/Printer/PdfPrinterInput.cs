using Regira.Office.Models;

namespace Regira.Office.PDF.Printer;

public class PdfPrinterInput
{
    public Stream? PdfStream { get; set; }
    public PageSize PageSize { get; set; } = PageSize.A4;
    public PageOrientation PageOrientation { get; set; }
    public string? PrinterName { get; set; }
}