using System.Collections;
using System.Reflection;

namespace Regira.Utilities;

public static class PropertyInfoUtility
{
    public static IEnumerable<PropertyInfo> WithInterface(this IEnumerable<PropertyInfo> properties, Type myInterface)
    {
        return properties.Where(p => TypeUtility.GetBaseTypes(p.PropertyType).Any(i => i == myInterface));
    }
    public static IEnumerable<PropertyInfo> WithInterface<T>(this IEnumerable<PropertyInfo> properties)
    {
        return WithInterface(properties, typeof(T));
    }
    public static IEnumerable<PropertyInfo> WithIEnumerableType(this IEnumerable<PropertyInfo> properties, Type myInterface)
    {
        return properties.Where(p =>
            TypeUtility.GetBaseTypes(p.PropertyType).Any(i => i == typeof(IEnumerable))
            && p.PropertyType.GetGenericArguments().Any(a => TypeUtility.GetBaseTypes(a).Any(i => i == myInterface))
        );
    }
    public static IEnumerable<PropertyInfo> WithIEnumerableType<T>(this IEnumerable<PropertyInfo> properties)
    {
        return WithIEnumerableType(properties, typeof(T));
    }
    public static IEnumerable<PropertyInfo> WithCustomAttribute(this IEnumerable<PropertyInfo> properties, Type attribute)
    {
        return properties.Where(p => p.GetCustomAttributes(true).Any(a => a.GetType() == attribute));
    }
    public static IEnumerable<PropertyInfo> WithCustomAttribute<T>(this IEnumerable<PropertyInfo> properties)
    {
        return WithCustomAttribute(properties, typeof(T));
    }

    public static TValue? GetAttributeValue<TAttribute, TValue>(this PropertyInfo prop, Func<TAttribute, TValue> value) where TAttribute : Attribute
    {
        var attr = prop.GetCustomAttributes(typeof(TAttribute), true)
            .FirstOrDefault();

        if (attr is TAttribute att)
        {
            return value(att);
        }

        return default;
    }
}