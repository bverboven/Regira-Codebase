using Regira.IO.Abstractions;

namespace Regira.IO.Models;

public class TextFileItem : BinaryFileItem, ITextFile
{
    public string? Contents { get; set; }
}