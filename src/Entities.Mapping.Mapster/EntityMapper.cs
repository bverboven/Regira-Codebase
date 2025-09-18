using Mapster;
using MapsterMapper;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Mapping.Mapster;

public class EntityMapper(IMapper mapper) : IEntityMapper
{
    public TTarget Map<TTarget>(object source)
        => mapper.Map<TTarget>(source);

    public TTarget Map<TSource, TTarget>(TSource source)
        => mapper.Map<TSource, TTarget>(source!);

    public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        => mapper.Map(source, target);
}

public class EntityMapConfigurator(TypeAdapterConfig config) : IEntityMapConfigurator
{
    public void Configure<TSource, TTarget>()
    {
        config.NewConfig<TSource, TTarget>();
    }
    
    public void ConfigureAttachment<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment,
        TEntityAttachmentDto>() where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> where TAttachment : class, IAttachment<TAttachmentKey>, new() where TEntityAttachmentDto : EntityAttachmentDto
    {
        
    }
}