using PDFtoPrinter;
using Regira.IO.Extensions;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Printer;
using System.Drawing.Printing;

namespace Regira.Office.PDF.PDFtoPrinter;

public class PdfPrinter : IPdfPrinter
{
    public string DefaultPrinter => new PrinterSettings().PrinterName;

    public IEnumerable<string> List()
    {
        return PrinterSettings.InstalledPrinters.Cast<string>();
    }

    public void Print(PdfPrinterInput input)
    {
        var printer = new PDFtoPrinterPrinter();
        var tempPath = Path.GetTempFileName();
        using (var fs = File.Create(tempPath))
        {
            using var pdfStream = input.PdfFile.GetStream()!;
            pdfStream.CopyTo(fs);
        }
        try
        {
            var printTimeout = new TimeSpan(0, 30, 0);
            printer.Print(new PrintingOptions(input.PrinterName, tempPath), printTimeout);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}