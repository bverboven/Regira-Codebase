using Regira.Serializing.Abstractions;
using Regira.Utilities;

namespace Regira.Serializing.Helpers;

public class ConvertHelper
{
    private readonly ISerializer _serializer;
    public ConvertHelper(ISerializer serializer)
    {
        _serializer = serializer;
    }


    public IEnumerable<T>? Convert<T>(IEnumerable<object> items)
        where T : class, new()
    {
        var dics = items
            .Select(x => DictionaryUtility.ToDictionary(x))
            .ToList();
        dics.MakeTypeSafe<T>();
        var json = _serializer.Serialize(dics);
        var typedItems = _serializer.Deserialize<List<T>>(json);
        return typedItems;
    }
}