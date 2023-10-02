using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Normalizing;


public class ObjectNormalizerContainer
{
#if NETSTANDARD2_0
    public class NormalizerDescriptor
    {
        public Type Type { get; set; } = null!;
        public IObjectNormalizer Normalizer { get; set; } = null!;
    }
#else
    public record NormalizerDescriptor(Type Type, IObjectNormalizer Normalizer);
#endif
#if NETSTANDARD2_0
    public IList<NormalizerDescriptor> Descriptors { get; set; }
#else
    public IList<NormalizerDescriptor> Descriptors { get; init; }
#endif
    public ObjectNormalizerContainer()
    {
        Descriptors = new List<NormalizerDescriptor>();
    }


    public ObjectNormalizerContainer Register<T>(IObjectNormalizer normalizer)
        => Register(typeof(T), normalizer);
    public ObjectNormalizerContainer Register(Type type, IObjectNormalizer normalizer)
    {
#if NETSTANDARD2_0
        Descriptors.Add(new NormalizerDescriptor { Type = type, Normalizer = normalizer });
#else
        Descriptors.Add(new NormalizerDescriptor(type, normalizer));
#endif
        return this;
    }

    public ObjectNormalizerContainer Insert<T>(int index, IObjectNormalizer normalizer)
        => Insert(index, typeof(T), normalizer);
    public ObjectNormalizerContainer Insert(int index, Type type, IObjectNormalizer normalizer)
    {
#if NETSTANDARD2_0
        Descriptors.Insert(index,new NormalizerDescriptor { Type = type, Normalizer = normalizer });
#else
        Descriptors.Insert(index,new NormalizerDescriptor(type, normalizer));
#endif
        return this;
    }

    public IEnumerable<IObjectNormalizer> FindAll<T>()
        => FindAll(typeof(T));
    public IEnumerable<IObjectNormalizer> FindAll(Type type)
        => Descriptors.Where(d => d.Type == type || TypeUtility.GetBaseTypes(type).Contains(d.Type))
            .Select(d => d.Normalizer);

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

        return descriptor?.Normalizer;
    }
}