using System.Collections;

namespace Regira.Caching.Abstractions;

public abstract class CacheProvider : ICacheProvider
{
    public abstract IList<string> Keys { get; }

    public object? this[string key]
    {
        get => Get<object>(key);
        set => Set(key, value);
    }

    public abstract T? Get<T>(string key);
    public abstract void Set<T>(string key, T? value, int? duration = null);

    public abstract void Remove(string key);
    public abstract void RemoveAll();

    private class CacheEnumerator : IEnumerator<KeyValuePair<string, object?>>, IDictionaryEnumerator
    {
        private int _index;
        private readonly IList<string> _keys;
        private readonly Func<string, object?> _getter;
        public CacheEnumerator(IList<string> keys, Func<string, object?> getter)
        {
            _keys = keys;
            _getter = getter;
        }

        public bool MoveNext()
        {
            if (++_index >= _keys.Count)
            {
                return false;
            }
            return true;
        }
        public void Reset()
        {
            _index = 0;
        }

        object IEnumerator.Current => Current;

        public object Key => _keys[_index];
        public object? Value => _getter(_keys[_index]);
        public KeyValuePair<string, object?> Current => new(_keys[_index], Value);
        public DictionaryEntry Entry => new(Current.Key, Current.Value);

        public void Dispose()
        {
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public IDictionaryEnumerator GetEnumerator()
    {
        return new CacheEnumerator(Keys, Get<object>);
    }
}