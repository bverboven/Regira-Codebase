using Regira.IO.Abstractions;
using Regira.Office.PDF.Models;

namespace Regira.Office.PDF.Abstractions;

public interface IImagesToPdfService
{
    IMemoryFile? ImagesToPdf(ImagesInput input);
}