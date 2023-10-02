using Regira.Caching.Abstractions;
using Regira.Utilities;
using System.Collections.Concurrent;

namespace Regira.Caching;

public class InMemoryCacheProvider : CacheProvider
{
    protected static readonly IDictionary<string, object?> Items = new ConcurrentDictionary<string, object?>();
    public override IList<string> Keys => Items.Keys
        .Where(k => string.IsNullOrWhiteSpace(_prefix) || k.StartsWith(_prefix!))
        .Select(GetKeyWithoutPrefix)
        .AsList();

    private readonly string? _prefix;
    public InMemoryCacheProvider(string? prefix = null)
    {
        _prefix = prefix;
    }

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
    public override void Set<T>(string key, T? value, int duration)
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
        if (string.IsNullOrEmpty(_prefix))
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
        return $"{_prefix}{key}";
    }
    private string GetKeyWithoutPrefix(string key)
    {
        return key.Substring(_prefix?.Length ?? 0);
    }
}