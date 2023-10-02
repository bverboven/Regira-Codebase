using System.Collections;
using System.Reflection;

namespace Regira.Utilities;

public static class ObjectUtility
{
    public static T Merge<T>(T target, params T[] sources)
    {
        var type = target!.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null)
            .ToArray();

        foreach (var source in sources)
        {
            Merge(target, source, properties);
        }

        return target;
    }
    private static void Merge<T>(T target, T source, IEnumerable<PropertyInfo> properties)
    {
        foreach (var property in properties)
        {
            var value = property.GetValue(source);
            if (value != null)
            {
                var targetValue = property.GetValue(target);
                var propertyType = property.PropertyType;
                if (targetValue != null && !TypeUtility.IsSimpleType(propertyType) && propertyType.IsClass)
                {
                    Merge(targetValue, property.GetValue(source));
                }
                else
                {
                    property.SetValue(target, value);
                }
            }
        }
    }

    public static T Fill<T>(T target, object? input)
    {
        var targetProperties = typeof(T)
            .GetProperties()
            .Where(prop => prop.GetIndexParameters().Length == 0 && prop.SetMethod != null)
            .ToArray();

        if (input != null)
        {
            if (input is IDictionary<string, object?> dic)
            {
                return Fill(target, dic);
            }

            var inputProperties = input.GetType()
                .GetRuntimeProperties()
                .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null)
                .ToArray();
            foreach (var property in targetProperties)
            {
                var inputProperty = inputProperties.FirstOrDefault(p => p.Name == property.Name || p.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase) || p.Name.ToPascalCase()!.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                if (inputProperty != null)
                {
                    var value = ConvertValue(inputProperty.GetValue(input), property.PropertyType);
                    property.SetValue(target!, value, null);
                }
            }
        }

        return target;
    }
    public static T Create<T>(object input)
        where T : new()
        => Fill(new T(), input);

    public static T Fill<T>(T target, IDictionary<string, object?> input)
    {
        var caseInsensitiveInput = new Dictionary<string, object?>(input, StringComparer.InvariantCultureIgnoreCase);
        var targetProperties = typeof(T).GetProperties()
            .Where(prop => prop.GetIndexParameters().Length == 0 && prop.SetMethod != null);
        foreach (var property in targetProperties)
        {
            // find the corresponding input key using ToPascalCase to have some tolerance
            var inputKey = caseInsensitiveInput.Keys.FirstOrDefault(key => key == property.Name || key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase) || key.ToPascalCase()!.Equals(property.Name, StringComparison.CurrentCultureIgnoreCase));
            if (inputKey != null)
            {
                var value = ConvertValue(caseInsensitiveInput[inputKey], property.PropertyType);
                property.SetValue(target!, value, null);
            }
        }

        return target;
    }

    public static object? ConvertValue(object? value, Type type)
    {
        if (value is ICollection collection)
        {
            var genericType = TypeUtility.GetGenericCollectionType(type);
            if (genericType != null)
            {
                value = CollectionUtility.CreateGenericList(genericType, collection);
            }
        }

        if (value != null && value.GetType() != TypeUtility.GetSimpleType(type) && !TypeUtility.GetBaseTypes(value.GetType()).Contains(TypeUtility.GetSimpleType(type)))
        {
            return Convert.ChangeType(value, TypeUtility.GetSimpleType(type));
        }

        return value;
    }
}