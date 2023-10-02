using Regira.Drawing.Abstractions;
using Regira.IO.Abstractions;
using Regira.Office.Models;
using Regira.Office.Word.Models;

namespace Regira.Office.Word.Abstractions;

public interface IWordManager
{
    IMemoryFile Create(WordTemplateInput input);
    IMemoryFile Merge(params WordTemplateInput[] inputs);
    IMemoryFile Convert(WordTemplateInput input, FileFormat format);
    IMemoryFile Convert(WordTemplateInput input, ConversionOptions options);

    string GetText(WordTemplateInput input);
    IEnumerable<WordImage> GetImages(WordTemplateInput input);

    /// <summary>
    /// Converts a document to a collection of images (1 image per page)
    /// </summary>
    IEnumerable<IImageFile> ToImages(WordTemplateInput input);
}