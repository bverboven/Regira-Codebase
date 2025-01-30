using System.Collections.Concurrent;
using Regira.Caching.Abstractions;
using Regira.Utilities;

namespace Regira.Caching;

public class DictionaryCacheProvider(string? prefix = null) : CacheProvider
{
    protected static readonly IDictionary<string, object?> Items = new ConcurrentDictionary<string, object?>();
    public override IList<string> Keys => Items.Keys
        .Where(k => string.IsNullOrWhiteSpace(prefix) || k.StartsWith(prefix!))
        .Select(GetKeyWithoutPrefix)
        .AsList();

    public override T? Get<T>(string key)
        where T : default
    {
        key = GetKey(key);
        if (Items.ContainsKey(key) && Items[key] is T value)
        {
            return value;
        }

        return default;
    }
    public override void Set<T>(string key, T? value, int? duration = null)
        where T : default
    {
        key = GetKey(key);
        Items[key] = value;
    }

    public override void Remove(string key)
    {
        key = GetKey(key);
        Items.Remove(key);
    }
    public override void RemoveAll()
    {
        if (string.IsNullOrEmpty(prefix))
        {
            Items.Clear();
        }
        else
        {
            foreach (var key in Keys)
            {
                Remove(key);
            }
        }
    }


    private string GetKey(string key)
    {
        return $"{prefix}{key}";
    }
    private string GetKeyWithoutPrefix(string key)
    {
        return key.Substring(prefix?.Length ?? 0);
    }
}