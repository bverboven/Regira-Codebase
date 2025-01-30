using Regira.Serializing.Abstractions;
using Regira.Utilities;

namespace Regira.Serializing.Helpers;

public class ConvertHelper(ISerializer serializer)
{
    public IEnumerable<T>? Convert<T>(IEnumerable<object> items)
        where T : class, new()
    {
        var dics = items
            .Select(x => DictionaryUtility.ToDictionary(x))
            .ToList();
        dics.MakeTypeSafe<T>();
        var json = serializer.Serialize(dics);
        var typedItems = serializer.Deserialize<List<T>>(json);
        return typedItems;
    }
}