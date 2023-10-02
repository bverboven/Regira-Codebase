using Regira.Normalizing.Abstractions;
using Regira.Normalizing.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace Regira.Normalizing;

public class NormalizingUtility
{

    private static readonly object Lock1 = new();
    private static readonly object Lock2 = new();
    private static readonly IDictionary<PropertyInfo, NormalizedAttribute?> CachedAttributes = new ConcurrentDictionary<PropertyInfo, NormalizedAttribute?>();
    private static readonly IDictionary<Type, MethodInfo?> CachedNormalizers = new ConcurrentDictionary<Type, MethodInfo?>();

    public static void InvokeObjectNormalizer(object instance, NormalizingOptions? options = null)
    {
        options ??= new NormalizingOptions();

        IObjectNormalizer? objNormalizerInstance = null;
        var attr = GetNormalizedAttribute(instance.GetType());
        if (attr != null)
        {
            objNormalizerInstance = GetObjectNormalizer(attr, options.ServiceProvider);
        }

        objNormalizerInstance ??= options.DefaultObjectNormalizer ?? NormalizingDefaults.DefaultObjectNormalizer ?? new ObjectNormalizer(options);
        objNormalizerInstance.HandleNormalize(instance, attr?.Recursive ?? options.DefaultRecursive);
    }
    public static void InvokePropertyNormalizer(object instance, PropertyInfo prop, NormalizingOptions? options = null)
    {
        var attribute = GetNormalizedAttribute(prop);
        if (attribute == null)
        {
            return;
        }

        var propNames = attribute.SourceProperties ?? new[] { attribute.SourceProperty ?? prop.Name };

        var targetValues = propNames
            .Aggregate(new List<string>(), (r, propName) =>
            {
                var sourceProp = instance.GetType().GetProperty(propName);
                if (sourceProp != null)
                {
                    var value = sourceProp.GetValue(instance)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        r.Add(value);
                    }
                }
                return r;
            });

        if (targetValues.Any())
        {
            var normalizerInstance = GetPropertyNormalizer(attribute, options?.ServiceProvider) ?? options?.DefaultNormalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer(options);
            prop.SetValue(instance, normalizerInstance.Normalize(string.Join(" ", targetValues)));
        }
    }

    public static NormalizedAttribute? GetNormalizedAttribute(Type classType)
        => classType.GetCustomAttributes<NormalizedAttribute>().FirstOrDefault();
    public static NormalizedAttribute? GetNormalizedAttribute(PropertyInfo prop)
    {
        lock (Lock2)
        {
            if (!CachedAttributes.ContainsKey(prop))
            {
                var attr = (NormalizedAttribute?)prop.GetCustomAttributes(true)
                    .FirstOrDefault(a => a is NormalizedAttribute);
#if NETSTANDARD2_0
                CachedAttributes.Add(prop, attr);
#else
                CachedAttributes.TryAdd(prop, attr);
#endif
                return attr;
            }

            return CachedAttributes[prop];
        }
    }

    public static IObjectNormalizer? GetObjectNormalizer(NormalizedAttribute attribute, IServiceProvider? sp = null)
    {
        var normalizerType = attribute.Normalizer;
        if (normalizerType == null)
        {
            return null;
        }

        return (IObjectNormalizer)(sp?.GetService(normalizerType) ?? Activator.CreateInstance(normalizerType)!);
    }
    public static INormalizer? GetPropertyNormalizer(NormalizedAttribute attribute, IServiceProvider? sp = null)
    {
        var normalizerType = attribute.Normalizer;

        if (normalizerType == null)
        {
            return null;
        }

        // using a lock since parallel testing often causes a System.ArgumentException "key already existed"
        lock (Lock1)
        {
            if (!CachedNormalizers.ContainsKey(normalizerType))
            {
                var method = normalizerType.GetMethod("Normalize");
#if NETSTANDARD2_0
                CachedNormalizers.Add(normalizerType, method);
#else
                CachedNormalizers.TryAdd(normalizerType, method);
#endif
            }

            return (INormalizer)(sp?.GetService(normalizerType) ?? Activator.CreateInstance(normalizerType)!);
        }
    }
}