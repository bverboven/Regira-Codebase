using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Regira.Utilities;

// Copied from Microsoft.AspNet.Mvc.Rendering.PropertyHelper (internal class)
// https://github.com/dotnet/aspnetcore/blob/df16982697a18e29a34488b54e276240f3d1afa0/src/Microsoft.AspNet.Mvc.Rendering/PropertyHelper.cs
public class PropertyUtility
{
    // Delegate type for a by-ref property getter
    private delegate TValue ByRefFunc<TDeclaringType, out TValue>(ref TDeclaringType arg);

    private static readonly MethodInfo CallPropertyGetterOpenGenericMethod = typeof(PropertyUtility).GetTypeInfo().GetDeclaredMethod("CallPropertyGetter")!;
    private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod = typeof(PropertyUtility).GetTypeInfo().GetDeclaredMethod("CallPropertyGetterByReference")!;
    private static readonly ConcurrentDictionary<Type, PropertyUtility[]> ReflectionCache = new();
    private readonly Func<object, object?> _valueGetter;
    private readonly Action<object, object?> _valueSetter;

    /// <summary>
    /// Initializes a fast property helper. 
    /// 
    /// This constructor does not cache the helper. For caching, use GetProperties.
    /// </summary>
    public PropertyUtility(PropertyInfo property)
    {
        Contract.Assert(property != null);

        Name = property.Name;
        _valueGetter = MakeFastPropertyGetter(property);
        _valueSetter = property.SetValue;
    }

    public string Name { get; protected set; }

    public object? GetValue(object instance)
        => _valueGetter(instance);

    public void SetValue(object instance, object? value)
        => _valueSetter(instance, value);

    /// <summary>
    /// Creates and caches fast property helpers that expose getters for every public get property on the 
    /// underlying type.
    /// </summary>
    /// <param name="instance">the instance to extract property accessors for.</param>
    /// <returns>a cached array of all public property getters from the underlying type of target instance.</returns>
    public static PropertyUtility[] GetProperties(object instance)
    {
        return GetProperties(instance, CreateInstance, ReflectionCache);
    }

    /// <summary>
    /// Creates a single fast property getter. The result is not cached.
    /// </summary>
    /// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
    /// <returns>a fast getter.</returns>
    /// <remarks>
    /// This method is more memory efficient than a dynamically compiled lambda, and about the 
    /// same speed.
    /// </remarks>
    public static Func<object, object?> MakeFastPropertyGetter(PropertyInfo propertyInfo)
    {
        Contract.Assert(propertyInfo != null);

        var getMethod = propertyInfo.GetMethod;
        Contract.Assert(getMethod != null);
        Contract.Assert(!getMethod.IsStatic);
        Contract.Assert(getMethod.GetParameters().Length == 0);

        // Instance methods in the CLR can be turned into static methods where the first parameter
        // is open over "target". This parameter is always passed by reference, so we have a code
        // path for value types and a code path for reference types.
        var typeInput = getMethod.DeclaringType!;
        var typeOutput = getMethod.ReturnType;

        Delegate callPropertyGetterDelegate;
        if (typeInput.IsValueType)
        {
            // Create a delegate (ref TDeclaringType) -> TValue
            var delegateType = typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput);
            var propertyGetterAsFunc = getMethod.CreateDelegate(delegateType);
            var callPropertyGetterClosedGenericMethod = CallPropertyGetterByReferenceOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
            callPropertyGetterDelegate = callPropertyGetterClosedGenericMethod.CreateDelegate(typeof(Func<object, object?>), propertyGetterAsFunc);
        }
        else
        {
            // Create a delegate TDeclaringType -> TValue
            var propertyGetterAsFunc = getMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput));
            var callPropertyGetterClosedGenericMethod = CallPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
            callPropertyGetterDelegate = callPropertyGetterClosedGenericMethod.CreateDelegate(typeof(Func<object, object?>), propertyGetterAsFunc);
        }

        return (Func<object, object?>)callPropertyGetterDelegate;
    }

    private static PropertyUtility CreateInstance(PropertyInfo property)
    {
        return new PropertyUtility(property);
    }

    // Called via reflection
    // ReSharper disable once UnusedMember.Local
    private static object? CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object target)
    {
        return getter((TDeclaringType)target);
    }

    // Called via reflection
    // ReSharper disable once UnusedMember.Local
    private static object? CallPropertyGetterByReference<TDeclaringType, TValue>(ByRefFunc<TDeclaringType, TValue> getter, object target)
    {
        var unboxed = (TDeclaringType)target;
        return getter(ref unboxed);
    }

    protected static PropertyUtility[] GetProperties(
        object instance,
        Func<PropertyInfo, PropertyUtility> createPropertyHelper,
        ConcurrentDictionary<Type, PropertyUtility[]> cache)
    {
        // Using an array rather than IEnumerable, as target will be called on the hot path numerous times.
        PropertyUtility[] helpers;

        var type = instance.GetType();

        if (!cache.TryGetValue(type, out helpers!))
        {
            // We avoid loading indexed properties using the where statement.
            // Indexed properties are not useful (or valid) for grabbing properties off an anonymous object.
            var properties = type.GetRuntimeProperties().Where(
                prop => prop.GetIndexParameters().Length == 0 &&
                        prop.GetMethod != null &&
                        prop.GetMethod.IsPublic &&
                        !prop.GetMethod.IsStatic);

            helpers = properties.Select(p => createPropertyHelper(p)).ToArray();
            cache.TryAdd(type, helpers);
        }

        return helpers;
    }
}