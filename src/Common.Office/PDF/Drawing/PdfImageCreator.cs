using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Office.PDF.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Drawing;

public class PdfImageCreator(IPdfToImageService service, IPdfSplitter splitter) : ImageCreatorBase<PdfToImageLayerOptions>
{
    public override IImageFile? Create(PdfToImageLayerOptions input)
    {
        var page = input.Page ?? 1;
        var singlePagePdf= splitter.GetPageCount(input.File)>1
            ? splitter.Split(input.File, [new PdfSplitRange { Start = page, End = page }]).Single()
            : input.File;
        return service.ToImages(singlePagePdf, input.ToPdfToImageOptions()).SingleOrDefault();
    }
}