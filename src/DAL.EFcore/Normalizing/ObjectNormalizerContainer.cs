using Microsoft.Extensions.DependencyInjection;
using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Normalizing;


public class ObjectNormalizerContainer
{
    private readonly IServiceProvider _services = null!;
#if NETSTANDARD2_0
    public class NormalizerDescriptor
    {
        public Type Type { get; set; } = null!;
        public ServiceDescriptor Descriptor = null!;
    }
#else
    public record NormalizerDescriptor(Type Type, ServiceDescriptor Descriptor);

#endif
#if NETSTANDARD2_0
    public IList<NormalizerDescriptor> Descriptors { get; set; } = null!;
#else
    public IList<NormalizerDescriptor> Descriptors { get; init; } = null!;
#endif

    public ObjectNormalizerContainer(IServiceProvider services)
    {
        _services = services;
        Descriptors = new List<NormalizerDescriptor>();
    }

    public ObjectNormalizerContainer Register<T>(Func<IServiceProvider, IObjectNormalizer> factory)
        => Register(typeof(T), factory);
    public ObjectNormalizerContainer Register(Type type, Func<IServiceProvider, IObjectNormalizer> factory)
    {
#if NETSTANDARD2_0
        Descriptors.Add(new NormalizerDescriptor { Type = type, Descriptor = new ServiceDescriptor(typeof(IObjectNormalizer<>).MakeGenericType(type), factory, ServiceLifetime.Transient) });
#else
        Descriptors.Add(new NormalizerDescriptor(type, new ServiceDescriptor(typeof(IObjectNormalizer<>).MakeGenericType(type), factory, ServiceLifetime.Transient)));
#endif
        return this;
    }

    public ObjectNormalizerContainer Insert<T>(int index, Func<IServiceProvider, IObjectNormalizer> factory)
        => Insert(index, typeof(T), factory);
    public ObjectNormalizerContainer Insert(int index, Type type, Func<IServiceProvider, IObjectNormalizer> factory)
    {
#if NETSTANDARD2_0
        Descriptors.Insert(index, new NormalizerDescriptor { Type = typeof(IObjectNormalizer<>).MakeGenericType(type), Descriptor = new ServiceDescriptor(type, factory, ServiceLifetime.Transient) });
#else
        Descriptors.Insert(index, new NormalizerDescriptor(type, new ServiceDescriptor(typeof(IObjectNormalizer<>).MakeGenericType(type), factory, ServiceLifetime.Transient)));
#endif
        return this;
    }

    /// <summary>
    /// <inheritdoc cref="FindAll(Type)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<IObjectNormalizer> FindAll<T>()
        => FindAll(typeof(T));
    /// <summary>
    /// Returns all corresponding normalizers<br />
    /// If any exclusive normalizers are present, only the first one will be returned
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IEnumerable<IObjectNormalizer> FindAll(Type type)
    {
        var normalizer = Find(type);
        if (normalizer?.IsExclusive == true)
        {
            return [normalizer];
        }

        return Descriptors.Where(d => d.Type == type || TypeUtility.GetBaseTypes(type).Contains(d.Type))
                .Select(d => (IObjectNormalizer)(d.Descriptor.ImplementationInstance ?? d.Descriptor.ImplementationFactory!(_services)))
                //.Select(d => (IObjectNormalizer)ActivatorUtilities.GetServiceOrCreateInstance(_services, d!.Descriptor.ServiceType))
                ;
    }

    public IObjectNormalizer? Find<T>()
        => Find(typeof(T));
    public IObjectNormalizer? Find(Type type)
    {
        var descriptor = Descriptors.FirstOrDefault(d => d.Type == type);
        if (descriptor == null)
        {
            if (type.BaseType != null)
            {
                return Find(type.BaseType);
            }
        }

        return (IObjectNormalizer?)(descriptor?.Descriptor.ImplementationInstance ?? descriptor?.Descriptor.ImplementationFactory!(_services));
        //return (IObjectNormalizer?)ActivatorUtilities.GetServiceOrCreateInstance(_services, descriptor!.Descriptor.ServiceType);
    }
}