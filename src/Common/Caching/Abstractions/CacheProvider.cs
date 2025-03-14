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

    private class CacheEnumerator(IList<string> keys, Func<string, object?> getter)
        : IEnumerator<KeyValuePair<string, object?>>, IDictionaryEnumerator
    {
        private int _index;

        public bool MoveNext()
        {
            if (++_index >= keys.Count)
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

        public object Key => keys[_index];
        public object? Value => getter(keys[_index]);
        public KeyValuePair<string, object?> Current => new(keys[_index], Value);
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