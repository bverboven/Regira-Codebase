using Regira.IO.Abstractions;

namespace Regira.Office.OCR.Abstractions;

public interface IOcrService
{
    Task<string?> Read(IMemoryFile imgFile, string? lang = null);
}
