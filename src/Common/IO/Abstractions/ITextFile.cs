namespace Regira.IO.Abstractions;

public interface ITextFile : IBinaryFile
{
    string? Contents { get; set; }
}