using Regira.IO.Abstractions;
using Regira.Office.Word.Models;

namespace Regira.Office.Word.Abstractions;

public interface IWordMerger
{
    Task<IMemoryFile> Merge(params WordTemplateInput[] inputs);
}