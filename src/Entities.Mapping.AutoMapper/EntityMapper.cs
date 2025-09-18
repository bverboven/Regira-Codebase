using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models.Abstractions;

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


public class EntityMapConfigurator(IServiceCollection services) : IEntityMapConfigurator
{
    public void Configure<TSource, TTarget>()
    {
        services.AddAutoMapper(cfg => cfg.CreateMap<TSource, TTarget>());
    }

    public void ConfigureAttachment<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment,
        TEntityAttachmentDto>() where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> where TAttachment : class, IAttachment<TAttachmentKey>, new() where TEntityAttachmentDto : EntityAttachmentDto
    {
        services
            .AddAutoMapper(cfg =>
                cfg.CreateMap<TEntityAttachment, TEntityAttachmentDto>()
                    .ForMember(
                        x => x.Uri,
                        opt => opt
                            .MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey,
                                TAttachmentKey, TAttachment, TEntityAttachmentDto>>()
                    )
            )
            .AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey,
                TAttachment, TEntityAttachmentDto>>();
    }
}