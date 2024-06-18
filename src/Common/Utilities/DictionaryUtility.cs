using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Regira.Utilities;

#region Options
public class DictionaryOptions
{
    /// <summary>
    /// Include null values (default = true)
    /// </summary>
    public bool IncludeNulls { get; set; } = true;
    /// <summary>
    /// Make keys key insensitive (default = true)
    /// </summary>
    public bool IgnoreCase { get; set; } = true;
    /// <summary>
    /// Process nested objects (default = true)
    /// </summary>
    public bool Recursive { get; set; } = true;
    /// <summary>
    /// Renames a duplicate key, otherwise overwrites existing value (default = false)
    /// </summary>
    public bool RenameDuplicateKeys { get; set; }
}
public class FlattenOptions
{
    /// <summary>
    /// Character(s) between properties (default = ".")
    /// </summary>
    public string Separator { get; set; } = ".";
    /// <summary>
    /// Exclude flattening collection values (default false)
    /// </summary>
    public bool IgnoreCollections { get; set; }
}
public class TableArrayOptions
{
    /// <summary>
    /// Expect headers in the first row
    /// </summary>
    public bool HeadersOnFirstRow { get; set; } = true;
    /// <summary>
    /// Exclude null values (default = false)
    /// </summary>
    public bool IgnoreNulls { get; set; } = false;
}
#endregion

public static class DictionaryUtility
{
    public static IDictionary<string, string?> ToDictionary(this NameValueCollection collection)
    {
        IDictionary<string, string?> dic = new Dictionary<string, string?>();
        var distinctKeys = collection.AllKeys.Distinct().ToArray();
        foreach (var key in distinctKeys)
        {
            dic.Add(key!, collection[key]);
        }
        return dic;
    }

    /// <summary>
    /// Keys should start with '-'<br />
    /// A value arg should be preceded with a key arg<br />
    /// A value can be null
    /// </summary>
    /// <param name="args"></param>
    /// <param name="keyPrefix"></param>
    /// <returns></returns>
    public static IDictionary<string, string?> ToDictionary(this IEnumerable<string> args, string keyPrefix = "-")
    {
        var dic = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);

        void Add(Queue<string> queue)
        {
            if (queue.Count > 0)
            {
                var arg = queue.Dequeue();
                if (arg.StartsWith(keyPrefix))
                {
                    var key = arg.Substring(1);
                    if (queue.Count > 0)
                    {
                        var value = queue.Peek();
                        dic.Add(key, value.StartsWith(keyPrefix) != true ? queue.Dequeue() : null);
                    }
                    else
                    {
                        dic.Add(key, null);
                    }
                }
                Add(queue);
            }
        }
        Add(new Queue<string>(args));

        return dic;
    }

    /// <summary>
    /// Converts an object to a dictionary
    /// </summary>
    /// <param name="source">object source</param>
    /// <param name="options">Dictionary options</param>
    /// <returns>IDictionary&lt;string, object&gt;</returns>
    public static IDictionary<string, object?> ToDictionary(object? source, DictionaryOptions? options = null)
    {
        options ??= new DictionaryOptions();

        var dic = CreateDictionary(source, options);
        if (options.IgnoreCase)
        {
            return new Dictionary<string, object?>(dic, StringComparer.InvariantCultureIgnoreCase);
        }

        return dic;
    }
    private static IDictionary<string, object?> CreateDictionary(object? source, DictionaryOptions options)
    {
        if (source == null || source is string)
        {
            return new Dictionary<string, object?>();
        }
        if (source is IDictionary<string, object?> objects)
        {
            return objects;
        }
        if (source is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return pairs.ToDictionary(k => k.Key, v => v.Value);
        }
        if (source is IEnumerable<KeyValuePair<string, string?>> stringPairs)
        {
            return stringPairs.ToDictionary(x => x.Key, x => (object?)x.Value);
        }
        if (source is NameValueCollection nameValues)
        {
            var nvDic = new Dictionary<string, object?>();
            foreach (var key in nameValues.AllKeys)
            {
                var value = nameValues[key];
                if (value != null || options.IncludeNulls)
                {
                    nvDic.Add(key!, value);
                }
            }
            return nvDic;
        }


        var dic = new Dictionary<string, object?>();
        foreach (var property in PropertyUtility.GetProperties(source))
        {
            var value = property.GetValue(source);
            if (value == null && !options.IncludeNulls)
            {
                continue;
            }

            var key = property.Name;
            if (options.RenameDuplicateKeys)
            {
                var index = 2;
                while (dic.ContainsKey(key))
                {
                    key = $"{property.Name}_{index}";
                }
            }

            dic[key] = GetValue(value, options.Recursive, options.IncludeNulls, options.RenameDuplicateKeys);
        }
        return dic;
    }
    public static object? GetValue(object? value, bool recursive, bool includeNulls, bool renameDuplicateKeys)
    {
        var valueType = value?.GetType();
        if (value == null || valueType == null || !recursive || TypeUtility.IsSimpleType(valueType) || TypeUtility.IsTypeACollection(valueType))
        {
            if (valueType != null && !TypeUtility.IsSimpleType(valueType) && value is IList list)//!TypeUtility.IsSimpleType(valueType) && TypeUtility.IsTypeACollection(valueType))
            {
                var listValues = new List<object?>(list.Count);
                foreach (var item in list)
                {
                    var listValue = GetValue(item, recursive, includeNulls, renameDuplicateKeys);
                    listValues.Add(listValue);
                }
                return listValues;
            }
        }
        else
        {
            return CreateDictionary(value, new DictionaryOptions { IncludeNulls = includeNulls, Recursive = true, RenameDuplicateKeys = renameDuplicateKeys });
        }

        return value;
    }
    public static IDictionary<string, object> NonNullable(this IDictionary<string, object?> source)
        => source.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value!);

    public static IDictionary<string, object?> Flatten(this IDictionary<string, object?> source, FlattenOptions? options = null)
    {
        options ??= new FlattenOptions();

        var target = new Dictionary<string, object?>();

        string BuildPrefix(string prefix, string key) => $"{prefix}{options.Separator}{key}".Trim(options.Separator.ToCharArray());
        void ParseRoot(IDictionary<string, object?> root, string prefix = "")
        {
            foreach (var kv in root)
            {
                SetItem(kv.Key, kv.Value, prefix);
            }
        }
        void SetItem(string key, object? value, string prefix = "")
        {
            var fullKey = BuildPrefix(prefix, key);
            if (value is IDictionary<string, object?> dic)
            {
                var newPrefix = fullKey;
                ParseRoot(dic, newPrefix);
            }
            else if (value is IEnumerable<IDictionary<string, object?>> collection)
            {
                var list = collection.ToList();
                if (options.IgnoreCollections)
                {
                    target[fullKey] = list.Select(item => Flatten(item, options)).ToList();
                }
                else
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        ParseRoot(list[i], BuildPrefix(fullKey, i.ToString()));
                    }
                }
            }
            else
            {
                target[fullKey] = value;
            }
        }

        ParseRoot(source);
        return target;
    }
    public static IDictionary<string, object?> Unflatten(this IDictionary<string, object?> source, FlattenOptions? options = null)
    {
        options ??= new FlattenOptions();

        string[] KeySegments(string key)
        {
            return key.Split(options.Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
        string FirstKeySegment(string key)
        {
            return KeySegments(key).First();
            //return key.Substring(0, key.IndexOf(separator, StringComparison.Ordinal));
        }
        string NextKeyString(string key)
        {
            return key.Substring(key.IndexOf(options.Separator, StringComparison.Ordinal) + 1);
        }
        IDictionary<string, object?> ParseRoot(IDictionary<string, object?> root)
        {
            var target = new Dictionary<string, object?>();
            foreach (var kv in root)
            {
                SetItem(kv.Key, kv.Value, target);
            }
            return target;
        }
        void AddHierarchy(string key, object? value, IDictionary<string, object?> target)
        {
            var parentKey = FirstKeySegment(key);
            var newKeyString = NextKeyString(key);
            var nextKey = FirstKeySegment(newKeyString);

            //var isNextKeyAnIndex = int.TryParse(newKeyString, out _);
            var isNextKeyAnIndex = int.TryParse(nextKey, out _);

            if (!target.ContainsKey(parentKey))
            {
                var parent = isNextKeyAnIndex
                    ? (object)new List<Dictionary<string, object?>>()
                    : new Dictionary<string, object?>();
                target[parentKey] = parent;
            }

            if (isNextKeyAnIndex)
            {
                SetListItem(newKeyString, value, ((List<Dictionary<string, object?>?>)target[parentKey]!));
            }
            else
            {
                SetItem(newKeyString, value, target[parentKey]);
            }
        }

        void SetListItem(string key, object? value, List<Dictionary<string, object?>?> target)
        {
            var index = int.Parse(FirstKeySegment(key));
            var newKeyString = NextKeyString(key);
            if (target.Count <= index)
            {
                target.Add(new Dictionary<string, object?>());
            }
            SetItem(newKeyString, value, target[index]);
        }
        void SetItem(string key, object? value, object? target)
        {
            if (target is IDictionary<string, object?> dic)
            {
                if (key.Contains(options.Separator))
                {
                    AddHierarchy(key, value, dic);
                }
                else if (value is IEnumerable<IDictionary<string, object?>> collection)
                {
                    dic[key] = collection
                        .Select(ParseRoot)
                        .ToList();
                }
                else
                {
                    dic[key] = value;
                }
            }
            else if (target is List<object?> list)
            {
                var index = int.Parse(key);
                CollectionUtility.EnsureSize(list, index);
                list[index] = value;
            }
        }

        return ParseRoot(source);
    }

    public static object?[,] ToTableArray(this IEnumerable<IDictionary<string, object?>> listOfDictionaries, TableArrayOptions? options = null)
    {
        options ??= new TableArrayOptions();

        var dicList = listOfDictionaries.ToArray();
        var keys = dicList
            .SelectMany(dic => dic.Keys).Distinct().ToArray();
        var table = new object?[dicList.Length + 1, keys.Length];

        var startRow = 0;
        if (options.HeadersOnFirstRow)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                table[0, i] = keys[i];
            }

            startRow = 1;
        }

        for (var i = 0; i < keys.Length; i++)
        {
            for (var row = startRow; row <= dicList.Length; row++)
            {
                var header = keys[i];
                var dic = dicList[row - startRow];
                if (dic.TryGetValue(header, out var value))
                {
                    table[row, i] = value;
                }
            }
        }

        return table;
    }
    public static IEnumerable<IDictionary<string, object?>> FromTableArray(object?[,] table, TableArrayOptions? options = null)
    {
        options ??= new TableArrayOptions();

        if (table.Rank != 2)
        {
            throw new NotSupportedException("Array must have 2 dimensions -> [,]");
        }

        var rowCount = table.GetLength(0) - (options.HeadersOnFirstRow ? 1 : 0);
        var colCount = table.GetLength(1);
        var dicList = new List<Dictionary<string, object?>>(rowCount);

        var keys = new string[colCount];
        for (var row = 0; row < table.GetLength(0); row++)
        {
            var dic = new Dictionary<string, object?>(colCount);
            for (var col = 0; col < table.GetLength(1); col++)
            {
                if (row == 0)
                {
                    if (options.HeadersOnFirstRow)
                    {
                        keys[col] = table[row, col]?.ToString()!;
                    }
                    else
                    {
                        keys[col] = $"Field-{col}";
                    }
                }

                if (row > 0 || !options.HeadersOnFirstRow)
                {
                    var value = table[row, col];
                    if (value != null || !options.IgnoreNulls)
                    {
                        var key = keys[col];
                        dic[key] = value;
                    }
                }
            }

            if (row > 0 || !options.HeadersOnFirstRow)
            {
                dicList.Add(dic);
            }
        }

        return dicList;
    }

    public static void MakeTypeSafe<T>(this IDictionary<string, object?> dic, PropertyInfo[]? properties = null)
        where T : class, new()
    {
        properties ??= typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (dic[prop.Name] != null && dic.ContainsKey(prop.Name) && dic[prop.Name]?.GetType() != TypeUtility.GetSimpleType(prop.PropertyType))
            {
                dic[prop.Name] = Convert.ChangeType(dic[prop.Name], TypeUtility.GetSimpleType(prop.PropertyType));
            }
        }
    }
    public static void MakeTypeSafe<T>(this IEnumerable<IDictionary<string, object?>> collection)
        where T : class, new()
    {
        var properties = typeof(T).GetProperties();

        foreach (var dic in collection)
        {
            MakeTypeSafe<T>(dic, properties);
        }
    }

    public static bool TryGetValue<T, TResult>(this IDictionary<string, T> dic, string key, out TResult? value)
    {
        if (dic.ContainsKey(key))
        {
            if (dic[key] is TResult _value)
            {
                value = _value;
                return true;
            }

            try
            {
                value = (TResult?)Convert.ChangeType(dic[key], typeof(TResult));
                return true;
            }
            catch
            {
                // continue below and return false
            }
        }

        value = default;
        return false;
    }
    public static IDictionary<string, TElement> ToDictionary<TSource, TElement>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector, bool renameDuplicateKeys)
    {
        var pairs = source.Select(x => new KeyValuePair<string, TElement>(keySelector(x), elementSelector(x)));
        var dic = new Dictionary<string, TElement>();
        foreach (var kvp in pairs)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            var index = 2;
            while (renameDuplicateKeys && dic.ContainsKey(key))
            {
                key = $"{kvp.Key}{index}";
            }
            dic[key] = value;
        }
        return dic;
    }

    public static T ToObject<T>(this IDictionary<string, object?> input, T target)
    {
        var caseInsensitiveInput = new Dictionary<string, object?>(input, StringComparer.InvariantCultureIgnoreCase);
        var targetProperties = typeof(T).GetProperties()
            .Where(prop => prop.GetIndexParameters().Length == 0 && prop.SetMethod != null);
        foreach (var property in targetProperties)
        {
            if (caseInsensitiveInput.TryGetValue(property.Name, out var value))
            {
                property.SetValue(target!, value, null);
            }
        }

        return target;
    }
    public static T ToObject<T>(this IDictionary<string, object?> input)
        where T : new()
        => ToObject(input, new T());

    /// <summary>
    /// Returns the first value for the given key
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="values"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetValue<TKey, TValue>(this ILookup<TKey, TValue> values, TKey key, out TValue? value)
    {
        var items = values.FirstOrDefault(x => x.Key!.Equals(key));
        if (items?.Any() != true)
        {
            value = default;
            return false;
        }

        value = items.FirstOrDefault();

        return true;
    }
}