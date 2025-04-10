using Regira.IO.Abstractions;
using Regira.Office.Models;
using Regira.Office.Word.Models;

namespace Regira.Office.Word.Abstractions;

public interface IWordConverter
{
    Task<IMemoryFile> Convert(WordTemplateInput input, FileFormat format);
    Task<IMemoryFile> Convert(WordTemplateInput input, ConversionOptions options);
}