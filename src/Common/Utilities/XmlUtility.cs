using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Regira.Utilities;

public static class XmlUtility
{
    public static Stream ToStream(string xml)
    {
        var doc = XDocument.Parse(xml);
        return ToStream(doc);
    }
    public static string FromStream(Stream stream)
    {
        var doc = XDocument.Load(stream);
        return doc.ToString();
    }
    public static Stream ToStream(this XDocument doc)
    {
        var ms = new MemoryStream();
        var xws = new XmlWriterSettings
        {
            //OmitXmlDeclaration = true,
            Indent = true
        };

        using var xw = XmlWriter.Create(ms, xws);
        doc.WriteTo(xw);

        return ms;
    }

    public static string Serialize<T>(T obj)
    {
        using var writer = new StringWriter();
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(writer, obj!);
        return writer.ToString();
    }
    public static T? Deserialize<T>(string? content)
    {
        var result = default(T);

        if (!string.IsNullOrEmpty(content))
        {
            using var stringReader = new StringReader(content);
            using var xmlReader = XmlReader.Create(stringReader);
            var serializer = new XmlSerializer(typeof(T));

            if (serializer.CanDeserialize(xmlReader))
            {
                result = (T?)serializer.Deserialize(xmlReader);
            }
        }
        return result;
    }
}