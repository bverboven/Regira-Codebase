using AutoMapper;
using Regira.Entities.Abstractions;

namespace Regira.Entities.Mapping.AutoMapper;

public class EntityMapper(IMapper mapper) : IEntityMapper
{
    public TTarget Map<TTarget>(object source) 
        => mapper.Map<TTarget>(source);

    public TTarget Map<TSource, TTarget>(TSource source) 
        => mapper.Map<TTarget>(source);

    public TTarget Map<TSource, TTarget>(TSource source, TTarget target) 
        => mapper.Map(source, target);
}