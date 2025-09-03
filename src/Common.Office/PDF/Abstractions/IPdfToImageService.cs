using Regira.IO.Abstractions;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Abstractions;

public interface IPdfToImageService
{
    IEnumerable<IImageFile> ToImages(IMemoryFile pdf, PdfToImagesOptions? options = null);
}
public interface IPdfToImageAsyncService
{
    IAsyncEnumerable<IImageFile> ToImagesAsync(IMemoryFile pdf, PdfToImagesOptions? options = null);
}
