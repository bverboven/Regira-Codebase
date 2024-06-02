using Regira.Normalizing.Abstractions;
using Regira.Normalizing.Models;
using Regira.Utilities;
using System.Collections;

namespace Regira.Normalizing;

public class ObjectNormalizer : IObjectNormalizer
{
    private readonly NormalizingOptions? _options;
    public INormalizer DefaultNormalizer { get; }
    public ObjectNormalizer(NormalizingOptions? options = null)
    {
        _options = options;
        DefaultNormalizer = _options?.DefaultNormalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer(options);
    }


    public virtual Task HandleNormalizeMany(IEnumerable<object?> instances, bool recursive = true)
    {
        foreach (var instance in instances)
        {
            HandleNormalize(instance, recursive, new HashSet<object>());
        }

        return Task.CompletedTask;
    }
    public virtual void HandleNormalize(object? instance, bool recursive = true)
        => HandleNormalizeMany([instance], recursive).Wait();

    protected internal virtual void HandleNormalize(object? instance, bool recursive, HashSet<object> processedInstances)
    {
        if (instance == null)
        {
            return;
        }

        // prevent stack overflow
        if (processedInstances.Contains(instance))
        {
            return;
        }

        processedInstances.Add(instance);

        var type = instance.GetType();

        // Property Normalizing
        foreach (var prop in type.GetProperties())
        {
            NormalizingUtility.InvokePropertyNormalizer(instance, prop, _options);

            if (recursive)
            {
                if (!TypeUtility.IsSimpleType(prop.PropertyType))
                {
                    if (TypeUtility.IsTypeACollection(prop.PropertyType))
                    {
                        var value = prop.GetValue(instance);
                        if (value is IList list)
                        {
                            foreach (var item in list)
                            {
                                HandleNormalize(item, true, processedInstances);
                            }
                        }
                    }
                    else
                    {
                        HandleNormalize(prop.GetValue(instance), true, processedInstances);
                    }
                }
            }
        }
    }
}