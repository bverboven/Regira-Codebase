using Regira.IO.Abstractions;

namespace Regira.Office.Csv.Abstractions;

public interface ICsvManager : ICsvManager<IDictionary<string, object>>
{
}
public interface ICsvManager<T>
{
    Task<List<T>> Read(string input);
    Task<List<T>> Read(IBinaryFile input);

    Task<string> Write(IEnumerable<T> items);
    Task<IMemoryFile> WriteFile(IEnumerable<T> items);
}