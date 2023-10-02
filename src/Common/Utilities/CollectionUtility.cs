using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using static System.Convert;

namespace Regira.Utilities;

public static class CollectionUtility
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();

    public static List<T> AsList<T>(this IEnumerable<T>? items)
    {
        return items switch
        {
            null => new List<T>(),
            List<T> list => list,
            _ => items.ToList()
        };
    }
    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> items, Func<T, object> selector, bool returnLastValue = false)
    {
        //return items.GroupBy(selector).Select(x => returnLastValue ? x.LastOrDefault() : x.FirstOrDefault());
        return DistinctBy(items, selector, x => returnLastValue ? x.Last() : x.First());
    }
    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> items, Func<T, object> groupSelector, Func<IGrouping<object, T>, T> distinctSelector)
    {
        return items.GroupBy(groupSelector).Select(distinctSelector);
    }

    // https://stackoverflow.com/questions/481603/set-extend-listt-length-in-c-sharp#answer-481658
    public static List<T?> EnsureSize<T>(this List<T> list, int size)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        return EnsureSize(list!, size, default);
    }
    public static List<T> EnsureSize<T>(this List<T> list, int size, T value)
    {
        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        var count = list.Count;
        if (count < size)
        {
            var capacity = list.Capacity;
            if (capacity < size)
            {
                list.Capacity = Math.Max(size, capacity * 2);
            }

            while (count < size)
            {
                list.Add(value);
                ++count;
            }
        }

        return list;
    }
    public static List<T?> SetSize<T>(this List<T> list, int size, T? defaultValue = default)
    {
        return EnsureSize(list!, size, defaultValue);
    }
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> items, int times)
    {
        var list = items.AsList();
        for (var i = 0; i < times; i++)
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<T> SortItems<T>(this IEnumerable<T> items, string sortBy)
    {
        var itemProperties = GetItemProperties<T>();
        var sortSegments = sortBy.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var sortProperty = itemProperties.FirstOrDefault(p => p.Name.Equals(sortSegments.First(), StringComparison.CurrentCultureIgnoreCase));
        if (sortProperty != null)
        {
            var sortDirection = sortSegments.Length > 1 ? sortSegments[1] : "asc";
            items = "desc".Equals(sortDirection, StringComparison.CurrentCultureIgnoreCase)
                ? items.OrderByDescending(x => sortProperty.GetValue(x))
                : items.OrderBy(x => sortProperty.GetValue(x));
        }

        return items;
    }
    public static IEnumerable<T> PageItems<T>(this IEnumerable<T> items, int pageSize, int pageIndex)
    {
        if (pageIndex > 0)
        {
            items = items.Skip(pageSize * pageIndex);
        }

        items = items.Take(pageSize);

        return items;
    }


    public static async Task<IList<TResult>> ChunkActionsAsync<T, TResult>(this IEnumerable<T> items, Func<T, Task<TResult>> action, int size = 10)
    {
        var results = new List<TResult>();

        var chunks = items.Chunk(size);
        foreach (var chunk in chunks)
        {
            var tasks = chunk.Select(action);
            var chunkResults = await Task.WhenAll(tasks);
            results.AddRange(chunkResults);
        }

        return results;
    }


    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        // https://stackoverflow.com/questions/5807128/an-extension-method-on-ienumerable-needed-for-shuffling#answer-5807238
        if (source == null) throw new ArgumentNullException(nameof(source));

        return source.ShuffleIterator(new Random());
    }
    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
    {
        var buffer = source.ToList();
        for (var i = 0; i < buffer.Count; i++)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }

    public static IEnumerable<T> FilterItems<T>(this IEnumerable<T> items, object searchObject)
        => FilterItems(items, DictionaryUtility.ToDictionary(searchObject));
    public static IEnumerable<T> FilterItems<T>(this IEnumerable<T> items, IDictionary<string, object?> searchObject)
    {
        var flatSo = DictionaryUtility.Flatten(searchObject);

        return items
            .Where(x => flatSo.All(kvp =>
            {
                var itemDic = DictionaryUtility.Flatten(
                    DictionaryUtility.ToDictionary(x, new DictionaryOptions { IncludeNulls = true }),
                    new FlattenOptions { IgnoreCollections = true }
                );

                var searchKey = kvp.Key;
                var searchValue = kvp.Value;
                var itemKey = FindItemKey(itemDic.Keys, searchKey);

                if (itemKey == null)
                {
                    return false;
                }

                var itemValue = itemDic[itemKey];
                if (itemValue != null)
                {
                    var simpleType = TypeUtility.GetSimpleType(itemValue.GetType());
                    if (simpleType.IsEnum)
                    {
                        return CompareEnums(itemValue, searchValue, simpleType);
                    }

                    var simpleSearchValue = ChangeType(searchValue, simpleType);

                    var minMaxResult = CompareMinMax(itemValue, simpleSearchValue, searchKey);
                    if (minMaxResult.HasValue)
                    {
                        return minMaxResult.Value;
                    }

                    if (simpleType == typeof(string))
                    {
                        return CompareStrings(itemValue, simpleSearchValue);
                    }
                }

                return itemValue == searchValue || searchValue != null && searchValue.Equals(itemValue);
            }))
            .ToList();
    }
    private static string? FindItemKey(IEnumerable<string> itemKeys, string searchKey)
    {
        var searchKeySegments = searchKey.Split('.');
        var searchKeyPrefix = string.Join(".", searchKeySegments.Take(searchKeySegments.Length - 1));
        return itemKeys.FirstOrDefault(itemKey =>
        {
            if (itemKey.Equals(searchKey, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            var itemKeySegments = itemKey.Split('.');
            var itemKeyPrefix = string.Join(".", itemKeySegments.Take(itemKeySegments.Length - 1));
            return itemKeyPrefix.Equals(searchKeyPrefix, StringComparison.InvariantCultureIgnoreCase)
                   && (
                       $"Min{itemKeySegments.Last()}".Equals(searchKeySegments.Last(), StringComparison.InvariantCultureIgnoreCase)
                       || $"Max{itemKeySegments.Last()}".Equals(searchKeySegments.Last(), StringComparison.InvariantCultureIgnoreCase)
                   );
        });
    }
    private static bool CompareEnums(object itemValue, object? searchValue, Type enumType)
    {
        var enumString = searchValue?.ToString();
        // compare enum by int-value
        if (int.TryParse(enumString, out var enumInt))
        {
            return enumInt == (int)itemValue;
        }
        // compare enum by string-value
        var correctedEnumValue = Enum.GetNames(enumType).FirstOrDefault(n => n.Equals(enumString, StringComparison.InvariantCultureIgnoreCase));
        return correctedEnumValue != null && correctedEnumValue.Equals(itemValue.ToString());
    }
    private static bool? CompareMinMax(object itemValue, object? searchValue, string searchKey)
    {
        var searchKeySegments = searchKey.Split('.');
        var lastSegment = searchKeySegments.Last();
        if (lastSegment.StartsWith("Min", StringComparison.InvariantCultureIgnoreCase))
        {
            return ((IComparable)itemValue).CompareTo(searchValue) >= 0;
        }

        if (lastSegment.StartsWith("Max", StringComparison.InvariantCultureIgnoreCase))
        {
            return ((IComparable)itemValue).CompareTo(searchValue) <= 0;
        }

        return null;
    }
    private static bool CompareStrings(object itemValue, object? searchValue)
    {
        var searchString = searchValue?.ToString();
        var itemValueString = itemValue.ToString();

        if (searchString?.Contains('*') ?? false)
        {
            if (itemValueString != null)
            {
#if NETSTANDARD2_0
                    var startsWith = searchString.StartsWith("*");
                    var endsWith = searchString.EndsWith("*");
                    var trimmedSearchString = searchString.Trim("*".ToCharArray());
#else
                var startsWith = searchString.StartsWith('*');
                var endsWith = searchString.EndsWith('*');
                var trimmedSearchString = searchString.Trim('*');
#endif
                if (startsWith && endsWith)
                {
#if NETSTANDARD2_0
                        return itemValueString.Contains(trimmedSearchString);
#else
                    return itemValueString.Contains(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase);
#endif
                }
                if (startsWith)
                {
                    return itemValueString.EndsWith(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase);
                }
                if (endsWith)
                {
                    return itemValueString.StartsWith(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }

        return itemValueString?.Equals(searchString, StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
    private static PropertyInfo[] GetItemProperties<T>()
    {
        var itemType = typeof(T);
        if (!Cache.TryGetValue(itemType, out var itemProperties))
        {
            itemProperties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null)
                .ToArray();

            Cache.TryAdd(itemType, itemProperties);
        }

        return itemProperties;
    }


    public static IList CreateGenericList(Type genericType, ICollection? values = null, Type? listType = null)
    {
        var genericListType = TypeUtility.CreateGenericListType(genericType, listType);
        var list = (Activator.CreateInstance(genericListType) as IList)!;
        if (values?.Count > 0)
        {
            foreach (var value in values)
            {
                list.Add(value);
            }
        }
        return list;
    }

#if !NET6_0_OR_GREATER

        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return ChunkIterator(source, size);
        }

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();
            while (e.MoveNext())
            {
                TSource[] chunk = new TSource[size];
                chunk[0] = e.Current;

                int i = 1;
                for (; i < chunk.Length && e.MoveNext(); i++)
                {
                    chunk[i] = e.Current;
                }

                if (i == chunk.Length)
                {
                    yield return chunk;
                }
                else
                {
                    Array.Resize(ref chunk, i);
                    yield return chunk;
                    yield break;
                }
            }
        }
#endif
}