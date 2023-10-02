using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using Regira.Office.Models;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Printer;
using Spire.Pdf;
using Spire.Pdf.Print;

namespace Regira.Office.PDF.Spire;

public class PdfPrinter : IPdfPrinter
{
    public string DefaultPrinter => new PrinterSettings().PrinterName;

    public IEnumerable<string> List()
    {
        return PrinterSettings.InstalledPrinters.Cast<string>();
    }
    public void Print(PdfPrinterInput input)
    {
        var pdfStream = input.PdfStream;
        var printerName = input.PrinterName;
        var pageSize = GetPageSize(input.PageSize);
        var orientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), input.PageOrientation.ToString());

        using PdfDocument doc = new PdfDocument(pdfStream)
        {
            PageSettings = {
                Size = pageSize,
                Orientation = orientation
            }
        };
        // Not supported in .Net Standard -> .Net Core library
        // This property is removed in Spire.PDF v6.10...
        doc.PrintSettings.SelectSinglePageLayout(PdfSinglePageScalingMode.FitSize, true);
        if (!string.IsNullOrWhiteSpace(printerName))
        {
            doc.PrintSettings.PrinterName = printerName;
        }
        doc.Print();
    }

    private SizeF GetPageSize(PageSize size)
    {
        var type = typeof(PdfPageSize);
        var sizeName = size.ToString();
        var typeProp = type.GetProperty(sizeName, BindingFlags.Static);
        if (typeProp == null)
        {
            return PdfPageSize.A4;
        }
        var value = typeProp.GetValue(null, BindingFlags.Static, null, null, null)!;
        // ReSharper disable once PossibleNullReferenceException
        return (SizeF)value;
    }
}