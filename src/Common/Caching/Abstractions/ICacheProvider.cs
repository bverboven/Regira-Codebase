using System.Collections;

namespace Regira.Caching.Abstractions;

public interface ICacheProvider : IEnumerable
{
    IList<string> Keys { get; }
    object? this[string key] { get; set; }

    T? Get<T>(string key);
    void Set<T>(string key, T? value, int? duration = null);
    void Remove(string key);
    void RemoveAll();

    new IDictionaryEnumerator GetEnumerator();
}