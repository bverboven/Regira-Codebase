using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;
using Regira.Utilities;
using SelectPdf;

namespace Regira.Office.PDF.SelectPdf;

public class PdfManager : IHtmlToPdfService
{
    public class Options
    {
        public Action<HtmlInput, string?>? OnPrint { get; set; }
    }

    public event Action<HtmlInput, string?>? OnPrint;
    public PdfManager(Options? options = null)
    {
        OnPrint = options?.OnPrint;
    }


    public IMemoryFile Create(HtmlInput template)
    {
        var doc = GetPdfDocument(template);
        var ms = new MemoryStream();
        doc.Save(ms);
        doc.Close();
        return ms.ToMemoryFile();
    }

    protected PdfDocument GetPdfDocument(HtmlInput template)
    {
        var pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), template.Format.ToString(), true);
        var pdfOrientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), template.Orientation.ToString(), true);
        var margins = template.Margins;
        var htmlString = template.HtmlContent;

        var converter = new HtmlToPdf
        {
            Options =
            {
                //converter.Options.CssMediaType = HtmlToPdfCssMediaType.Print;
                PdfPageSize = pageSize,
                PdfPageOrientation = pdfOrientation,
                //}
                //    }
                //        converter.Options.WebPageHeight = (int)(DimensionsUtility.MmToPt(PageSizes.Mm.A4.Width) - (margins.Top + margins.Bottom));
                //        converter.Options.WebPageWidth = (int)(DimensionsUtility.MmToPt(PageSizes.Mm.A4.Height) - (margins.Right + margins.Left));
                //    {
                //    else
                //    }
                //        converter.Options.WebPageHeight = (int)(DimensionsUtility.MmToPt(PageSizes.Mm.A4.Height) - (margins.Top + margins.Bottom));
                //        converter.Options.WebPageWidth = (int)(DimensionsUtility.MmToPt(PageSizes.Mm.A4.Width) - (margins.Right + margins.Left));
                //    {
                //    if (template.Orientation == PageOrientation.Portrait)
                //{
                //if (pageSize == PdfPageSize.A4)
                //converter.Options.WebPageFixedSize = true;
                MarginTop = (int)margins.Top,
                MarginRight = (int)margins.Right,
                MarginBottom = (int)margins.Bottom,
                MarginLeft = (int)margins.Left
            }
            //converter.Options.EmbedFonts = true;
        };

        // header
        if (template.HeaderHtmlContent != null)
        {
            var header = new PdfHtmlSection(template.HeaderHtmlContent, null);
            converter.Header.Add(header);
            converter.Options.DisplayHeader = true;
            if (template.HeaderHeight.HasValue)
            {
                converter.Header.Height = MillimetersToPoints(template.HeaderHeight.Value);
            }
        }
        // footer
        if (template.FooterHtmlContent != null)
        {
            var footer = new PdfHtmlSection(template.FooterHtmlContent, null);
            converter.Footer.Add(footer);
            converter.Options.DisplayFooter = true;
            if (template.FooterHeight.HasValue)
            {
                converter.Footer.Height = MillimetersToPoints(template.FooterHeight.Value);
            }
        }

        OnPrint?.Invoke(template, htmlString);

        var doc = converter.ConvertHtmlString(htmlString);
        //var font = doc.AddFont(PdfStandardFont.TimesRoman);
        //font.Size = 14;
        return doc;
    }

    static int MillimetersToPoints(float mm)
    {
        return (int)DimensionsUtility.MmToPt(mm);
    }
}