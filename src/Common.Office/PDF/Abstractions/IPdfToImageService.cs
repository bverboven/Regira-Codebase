using Regira.IO.Abstractions;
using Regira.Media.Drawing.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfToImageService
{
    IEnumerable<IImageFile> ToImages(IBinaryFile pdf, PdfImageOptions? options = null);
}
public interface IPdfToImageAsyncService
{
    IAsyncEnumerable<IImageFile> ToImagesAsync(IBinaryFile pdf, PdfImageOptions? options = null);
}
