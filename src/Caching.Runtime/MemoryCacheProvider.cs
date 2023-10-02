using Regira.Caching.Abstractions;
using System.Runtime.Caching;

namespace Regira.Caching.Runtime;

public class MemoryCacheProvider : CacheProvider
{
    public class Options
    {
        public string? Prefix { get; set; }
    }

    private readonly string? _prefix;
    protected readonly MemoryCache Cache = MemoryCache.Default;
    public override IList<string> Keys { get; }
    public MemoryCacheProvider(Options? options = null)
    {
        _prefix = options?.Prefix;
        Keys = new List<string>();
    }
    // compatibility
    public MemoryCacheProvider(string prefix)
        : this(new Options { Prefix = prefix })
    {
    }


    public override T? Get<T>(string key)
        where T : default
    {
        var fullKey = GetKey(key);
        var cachedItem = Cache.GetCacheItem(fullKey);
        return (T?)cachedItem.Value;
    }
    public override void Set<T>(string key, T? value, int duration)
        where T : default
    {
        var fullKey = GetKey(key);
        Cache.Set(fullKey, value!, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(duration) });
    }
    public override void Remove(string key)
    {
        var fullKey = GetKey(key);
        Cache.Remove(fullKey);
    }
    public override void RemoveAll()
    {
        foreach (var key in Keys)
        {
            Remove(key);
        }
    }

    protected string GetKey(string key)
    {
        return $"{_prefix}{key}";
    }
}