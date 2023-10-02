using Regira.Office.PDF.Printer;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfPrinter
{
    string DefaultPrinter { get; }

    IEnumerable<string> List();
    void Print(PdfPrinterInput input);
}