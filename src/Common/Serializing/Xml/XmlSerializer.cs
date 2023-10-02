using Regira.Serializing.Abstractions;
using Regira.Utilities;

namespace Regira.Serializing.Xml;

public class XmlSerializer : ISerializer
{
    public T? Deserialize<T>(string? content)
        => XmlUtility.Deserialize<T>(content);

    public object? Deserialize(string? content, Type type)
        => ObjectUtility.Fill(Activator.CreateInstance(type), XmlUtility.Deserialize<object?>(content));


    public string Serialize<T>(T item)
    {
        return XmlUtility.Serialize(item);
    }
}