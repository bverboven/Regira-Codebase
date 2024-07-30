using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using Regira.Collections;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Office.MimeTypes;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Regira.Office.PDF.DocNET;

public class PdfManager(IImageService? imageService = null) : IPdfService
{
    public int GetPageCount(IBinaryFile pdf)
    {
        using var docReader = DocLib.Instance.GetDocReader(pdf.GetBytes(), new PageDimensions());
        return docReader.GetPageCount();
    }


    public IEnumerable<IMemoryFile> Split(IBinaryFile pdf, IEnumerable<PdfSplitRange> ranges)
    {
        foreach (var range in ranges)
        {
            var splitBytes = DocLib.Instance.Split(pdf.GetBytes(), range.Start - 1, (range.End ?? GetPageCount(pdf)) - 1);
            var file = splitBytes.ToMemoryFile(ContentTypes.PDF);
            yield return file;
        }
    }
    public IMemoryFile Merge(IEnumerable<string> pdfPaths)
    {
        var list = pdfPaths.ToArray();
        var maxBatchSize = 10;
        var batchSize = (int)Math.Ceiling(list.Length / (double)maxBatchSize);
        var partlyMergedPdfs = new List<byte[]>(batchSize);
        for (var i = 0; i < batchSize; i++)
        {
            var pdfBatch = list
                .Skip(i * maxBatchSize)
                .Take(maxBatchSize)
                .Select(File.ReadAllBytes)
                .ToArray();
            var merged = DocLib.Instance.Merge(pdfBatch);
            partlyMergedPdfs.Add(merged);
        }

        var mergedBytes = DocLib.Instance.Merge(partlyMergedPdfs.ToArray());
        return mergedBytes.ToMemoryFile(ContentTypes.PDF);
    }
    public IMemoryFile? Merge(IEnumerable<IBinaryFile> items)
    {
        byte[]? resultBytes = null;
        foreach (var pdf in items)
        {
            var bytes = pdf.GetBytes();
            resultBytes = resultBytes == null ? bytes : DocLib.Instance.Merge(resultBytes, bytes);
        }

        var ms = resultBytes != null
            ? new MemoryStream(resultBytes)
            : null;
        return ms?.ToMemoryFile(ContentTypes.PDF);
    }
    public IMemoryFile? RemovePages(IBinaryFile pdf, IEnumerable<int> pages)
    {
        var pagesToRemove = pages.ToArray();

        var pageCount = GetPageCount(pdf);

        var ranges = new List<PdfSplitRange>();
        var firstPage = pagesToRemove.First();
        if (firstPage > 1)
        {
            ranges.Add(new PdfSplitRange { Start = 1 });
        }
        foreach (var page in pagesToRemove)
        {
            var prev = ranges.LastOrDefault();
            if (page == prev?.Start)
            {
                prev.Start = page + 1;
            }
            else
            {
                if (prev?.End.HasValue == false)
                {
                    prev.End = page - 1;
                }
                if (page < pageCount - 1)
                {
                    ranges.Add(new PdfSplitRange
                    {
                        Start = page + 1
                    });
                }
            }
        }

        var splitPdfs = Split(pdf, ranges)
            .Select(f => f.ToBinaryFile())
            .ToArray();
        var merged = Merge(splitPdfs);
        splitPdfs.Dispose();
        return merged;
    }


    public string GetText(IBinaryFile pdf)
    {
        var texts = GetTextPerPage(pdf)
            .Select(line => line.Trim());
        return string.Join(Environment.NewLine, texts);
    }
    public IList<string> GetTextPerPage(IBinaryFile pdf)
    {
        var pageDim = new PageDimensions();
        using var docReader = DocLib.Instance.GetDocReader(pdf.GetBytes(), pageDim);

        var pageCount = docReader.GetPageCount();
        var pageTexts = new List<string>(pageCount);
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var pageReader = docReader.GetPageReader(pageIndex);
            var pageText = pageReader.GetText();
            pageTexts.Add(pageText);
        }

        return pageTexts;
    }
    public IMemoryFile? RemoveEmptyPages(IBinaryFile pdf)
    {
        var texts = GetTextPerPage(pdf);
        var emptyPages = texts
            .Select((x, i) => new { page = i + 1, isEmpty = string.IsNullOrWhiteSpace(x) })
            .Where(x => x.isEmpty)
            .Select(x => x.page)
            .ToArray();

        return emptyPages.Any()
            ? RemovePages(pdf, emptyPages)
            : pdf;
    }


    public IMemoryFile ImagesToPdf(ImagesInput input)
    {
        if (imageService == null)
        {
            throw new NullReferenceException($"{nameof(IImageService)} is not initialized");
        }

        var maxDim = new[]
        {
            (int)(input.MaxDimensions.Width - (input.Margins.Left + input.Margins.Right)),
            (int)(input.MaxDimensions.Height - (input.Margins.Top + input.Margins.Bottom))
        };
        var jpegImages = input.Images
            .Select(imgBytes =>
                {
                    var imageFile = imageService.Parse(imgBytes)!;
                    var resized = imageService.Resize(imageFile, maxDim);
                    return new JpegImage
                    {
                        Bytes = resized.Bytes,
                        Width = (int)(resized.Size?.Width ?? 0),
                        Height = (int)(resized.Size?.Height ?? 0)
                    };
                }
            )
            .ToArray();

        var pdfBytes = DocLib.Instance.JpegToPdf(jpegImages);
        return pdfBytes.ToMemoryFile(ContentTypes.PDF);
    }
    public IEnumerable<IImageFile> ToImages(IBinaryFile pdf, PdfImageOptions? options = null)
    {
        var pageDimensions = new PageDimensions(((int?)options?.Size?.Width) ?? 1080, (int?)options?.Size?.Height ?? 1920);
        using var docReader = DocLib.Instance.GetDocReader(pdf.GetBytes(), pageDimensions);
        var pageCount = docReader.GetPageCount();
        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var pr = docReader.GetPageReader(pageIndex);
            var width = pr.GetPageWidth();
            var height = pr.GetPageHeight();
            var imgBytes = pr.GetImage();

            using var img = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            AddBytes(img, imgBytes);
            yield return img.ToImageFile((options?.Format ?? Media.Drawing.Enums.ImageFormat.Jpeg).ToGdiImageFormat());
        }
    }


    private static void AddBytes(Bitmap bmp, byte[] rawBytes)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

        var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
        var pNative = bmpData.Scan0;

        Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
        bmp.UnlockBits(bmpData);
    }
}