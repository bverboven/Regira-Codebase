using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Regira.Utilities;

public static class TypeUtility
{
    /// <summary>
    /// Get Base type and interfaces, including inherited ones
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetBaseTypes(Type type)
    {
        if (type.BaseType == null)
        {
            return type.GetInterfaces();
        }
        return new[] { type.BaseType }
            .Concat(type.GetInterfaces())
            .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
            .Concat(GetBaseTypes(type.BaseType))
            .Distinct();
    }
    /// <summary>
    /// Converts a nullable type to it's corresponding simple type (int? -> int)
    /// </summary>
    /// <param name="nullableType"></param>
    /// <returns></returns>
    public static Type GetSimpleType(Type nullableType)
    {
        return Nullable.GetUnderlyingType(nullableType) ?? nullableType;
    }

    /// <summary>
    /// Checks if a type is a simple type:
    /// <list type="bullet">
    ///     <item>(Nullable) Primitive type</item>
    ///     <item>Enum</item>
    ///     <item>String</item>
    ///     <item>DateTime</item>
    ///     <item>DateOnly</item>
    ///     <item>DateTimeOffset</item>
    ///     <item>TimeSpan</item>
    ///     <item>Guid</item>
    /// </list>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsEnum
                                || new[] { typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid) }.Contains(type)
#if NETCOREAPP2_1_OR_GREATER
                                || new[] { typeof(DateOnly) }.Contains(type)
#endif
                                || Convert.GetTypeCode(type) != TypeCode.Object || (IsNullableType(type) && IsSimpleType(type.GetGenericArguments()[0]));
    }
    /// <summary>
    /// Checks if type implements Nullable&lt;&gt;
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// <inheritdoc cref="IsTypeEnumerable"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsTypeEnumerable<T>(Type sourceType)
        where T : IEnumerable
        => IsTypeEnumerable(sourceType, typeof(T));
    /// <summary>
    /// Checks if type can be enumerated
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsTypeEnumerable(Type sourceType, Type? type = null)
    {
        if (type != null && !type.GetInterfaces().Contains(typeof(IEnumerable)))
        {
            throw new ArgumentException($"{nameof(type)} is not an enumerable type");
        }

        if (type == sourceType)
        {
            return true;
        }

        if (sourceType.IsGenericType)
        {
            var faceType = sourceType.GetInterface(typeof(IEnumerable<>).FullName!);
            if (faceType == null)
            {
                return false;
            }

            type ??= typeof(IEnumerable<>);
            var genericCollectionType = type.MakeGenericType(GetGenericCollectionType(sourceType)!);
            var baseTypes = GetBaseTypes(sourceType).ToArray();
            return baseTypes.Contains(genericCollectionType);
        }

        type ??= typeof(IEnumerable);
        return GetBaseTypes(sourceType).Contains(type);
    }

    /// <summary>
    /// <inheritdoc cref="IsTypeACollection"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsTypeACollection<T>(Type sourceType)
        where T : IEnumerable
        => IsTypeACollection(sourceType, typeof(T));
    /// <summary>
    /// Checks if type is a collection (or list)
    /// </summary>
    /// <param name="sourceType"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsTypeACollection(Type sourceType, Type? type = null)
    {
        if (type != null)
        {
            var interfaces = (type.IsInterface ? new[] { type }.Concat(type.GetInterfaces()) : type.GetInterfaces())
                .ToArray();
            var genericArg = GetGenericCollectionType(type);
            var listType = genericArg == null ? typeof(ICollection) : typeof(ICollection<>).MakeGenericType(genericArg);
            if (!interfaces.Contains(listType))
            {
                throw new ArgumentException($"{nameof(type)} is not a collection type");
            }
        }

        return IsTypeEnumerable(sourceType, type);
    }

    public static Type? GetGenericCollectionType(Type collectionType)
    {
        var enumerableType = collectionType.GetInterface(typeof(IEnumerable<>).FullName!);
        return enumerableType?.GetGenericArguments().FirstOrDefault()
               ?? enumerableType?.GetElementType();
    }

    public static string? GetPropertyName(Expression<Func<object>> selector)
    {
        return GetMemberName(selector);
    }
    /// <summary>
    /// Get the property name by selecting an object's property member
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns>Name of property</returns>
    public static string? GetPropertyName<T>(Expression<Func<T, object>> selector)
    {
        return GetMemberName(selector);
    }
    private static string? GetMemberName<TFunc>(Expression<TFunc> selector)
    {
        // https://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression#answer-71965639
        var memberExpression = selector.Body is UnaryExpression expression
            ? expression.Operand as MemberExpression
            : selector.Body as MemberExpression;

        return (memberExpression?.Member as PropertyInfo)?.Name;

        // https://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression#answer-2916344
        //if (selector.Body is MemberExpression body)
        //{
        //    return body.Member.Name;
        //}

        //var ubody = (UnaryExpression)selector.Body;
        //var bodyExpression = ubody.Operand as MemberExpression;
        //return bodyExpression?.Member.Name;
    }

    /// <summary>
    /// <inheritdoc cref="ImplementsInterface"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool ImplementsInterface<T>(Type type)
        => ImplementsInterface(type, typeof(T));
    /// <summary>
    /// Checks if a type implements a given interface
    /// </summary>
    /// <param name="type"></param>
    /// <param name="implementedInterface"></param>
    /// <returns></returns>
    public static bool ImplementsInterface(Type type, Type implementedInterface)
    {
        if (type == implementedInterface)
        {
            return true;
        }

        var interfaces = type.GetInterfaces();
        return !implementedInterface.IsGenericType
            ? interfaces.Contains(implementedInterface)
            : interfaces.Any(x => x == implementedInterface || x.IsGenericType && x.GetGenericTypeDefinition() == implementedInterface);
    }

    public static bool HasGenericArgument(Type type, Type genericType)
        => type.GetGenericArguments()
            .Union(GetBaseTypes(type).SelectMany(t => t.GetGenericArguments()))
            .Contains(genericType);


    public static Type CreateGenericListType<TList, TGeneric>()
        where TList : IList<TGeneric>
        => CreateGenericListType(typeof(TGeneric), typeof(TList));
    public static Type CreateGenericListType<TGeneric>()
        => CreateGenericListType(typeof(TGeneric));
    public static Type CreateGenericListType(Type genericType, Type? listType = null)
    {
        listType ??= typeof(List<>);
        if (!ImplementsInterface<IList>(listType))
        {
            throw new ArgumentException($"{nameof(listType)} is not a list type");
        }
        return listType.MakeGenericType(genericType);
    }
}