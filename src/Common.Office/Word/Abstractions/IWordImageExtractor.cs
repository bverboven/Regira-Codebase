using Regira.Office.Word.Models;

namespace Regira.Office.Word.Abstractions;

public interface IWordImageExtractor
{
    IEnumerable<WordImage> GetImages(WordTemplateInput input);
}