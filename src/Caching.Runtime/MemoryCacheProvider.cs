using Regira.Caching.Abstractions;
using System.Runtime.Caching;

namespace Regira.Caching.Runtime;

public class MemoryCacheProvider : CacheProvider
{
    public class Options
    {
        public string? Prefix { get; set; }
        /// <summary>
        /// Default caching duration in seconds
        /// </summary>
        public int DefaultDuration { get; set; } = 60 * 60;
    }

    private readonly string? _prefix;
    private readonly int _defaultDuration;
    protected readonly MemoryCache Cache = MemoryCache.Default;
    public override IList<string> Keys { get; }
    public MemoryCacheProvider(Options? options = null)
    {
        options ??= new Options();
        _prefix = options.Prefix;
        _defaultDuration = options.DefaultDuration;
        Keys = new List<string>();
    }


    public override T? Get<T>(string key)
        where T : default
    {
        var fullKey = GetKey(key);
        var cachedItem = Cache.GetCacheItem(fullKey);
        return (T?)cachedItem.Value;
    }
    public override void Set<T>(string key, T? value, int? duration = null)
        where T : default
    {
        var fullKey = GetKey(key);
        Cache.Set(fullKey, value!, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(duration ?? _defaultDuration) });
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