using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.MimeTypes;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;
using Spire.Pdf;
using Spire.Pdf.Graphics;

namespace Regira.Office.PDF.Spire;

public class PdfManager : IPdfMerger, IPdfSplitter, IPdfToImageService, IPdfTextExtractor
{
    public int GetPageCount(IBinaryFile pdfStream)
    {
        using var doc = new PdfDocument(pdfStream.GetStream());
        return doc.Pages.Count;
    }

    public IEnumerable<IMemoryFile> Split(IBinaryFile pdf, IEnumerable<PdfSplitRange> ranges)
    {
        using var doc = new PdfDocument(pdf.GetStream());
        foreach (var range in ranges)
        {
            var split = new PdfDocument();
            for (var i = range.Start - 1; i < (range.End ?? doc.Pages.Count); i++)
            {
                var page = split.Pages.Add(doc.Pages[i].Size, new PdfMargins(0));
                doc.Pages[i].CreateTemplate().Draw(page, new PointF(0, 0));
            }

            yield return ToMemoryFile(split);
        }
    }

    public IMemoryFile Merge(IEnumerable<string> pdfPaths)
    {
        var merged = PdfDocument.MergeFiles(pdfPaths.ToArray());
        var ms = new MemoryStream();
        merged.Save(ms);
        return ms.ToMemoryFile(ContentTypes.PDF);
    }
    public IMemoryFile Merge(IEnumerable<IBinaryFile> items)
    {
        var merged = new PdfDocument();
        foreach (var pdfStream in items)
        {
            var doc = new PdfDocument(pdfStream.GetStream());
            for (var i = 0; i < doc.Pages.Count; i++)
            {
                merged.InsertPage(doc, i);
            }
        }

        return ToMemoryFile(merged);
    }

    public string GetText(IBinaryFile pdf)
    {
        var doc = new PdfDocument(pdf.GetStream());
        var text = new StringBuilder(doc.Pages.Count);
        foreach (PdfPageBase page in doc.Pages)
        {
            text.Append(page.ExtractText());
        }

        return text.ToString();
    }
    public IEnumerable<IImageFile> ToImages(IBinaryFile pdf, PdfImageOptions? options = null)
    {
        using var doc = new PdfDocument(pdf.GetStream());
        var pageCount = doc.Pages.Count;
        for (var i = 0; i < pageCount; i++)
        {
            using var image = doc.SaveAsImage(i);
            if (options?.Size.HasValue == true)
            {
                using var resized = GdiUtility.Resize(image, options.Size.Value.ToSize());
                yield return resized.ToImageFile(ImageFormat.Jpeg);
            }
            else
            {
                yield return image.ToImageFile(ImageFormat.Jpeg);
            }
        }
    }


    public IMemoryFile ToMemoryFile(PdfDocument doc)
    {
        var ms = new MemoryStream();
        doc.SaveToStream(ms);
        return ms.ToMemoryFile(ContentTypes.PDF);
    }

}