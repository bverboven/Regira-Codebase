using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Mapping;
using Regira.Entities.Web.Mapping.Abstractions;
using System.Reflection;

namespace Regira.Entities.Mapping.AutoMapper;

public static class ServiceCollectionExtensions
{
    public static IMappedEntityServiceCollection UseAutoMapperMapping(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression>? configure = null, IEnumerable<Assembly>? assemblies = null)
    {
        services
            .AddAutoMapper((p, e) =>
            {
                e.CreateMap<Attachment, AttachmentDto>();
                e.CreateMap<Attachment, AttachmentDto<int>>();
                e.CreateMap(typeof(Attachment<>), typeof(AttachmentDto<>));
                e.CreateMap<AttachmentInputDto, Attachment>();
                e.CreateMap<AttachmentInputDto<int>, Attachment>();
                e.CreateMap(typeof(AttachmentInputDto<>), typeof(Attachment<>));
                e.CreateMap<EntityAttachment, EntityAttachmentDto>()
                    .ForMember(
                        x => x.Uri,
                        opt => opt.MapFrom<AttachmentUriResolver<EntityAttachment, EntityAttachmentDto>>()
                    );
                e.CreateMap(typeof(EntityAttachment<,,,>), typeof(EntityAttachmentDto<,,>));
                e.CreateMap<EntityAttachmentInputDto, EntityAttachment>();
                e.CreateMap(typeof(EntityAttachmentInputDto<,,>), typeof(EntityAttachment<,,,>));

                configure?.Invoke(p, e);
            }, assemblies ?? [])
            .AddTransient<AttachmentUriResolver<EntityAttachment, EntityAttachmentDto>>()
            .AddTransient<IEntityMapper, EntityMapper>();

        return new MappedEntityServiceCollection(services);
    }

    public static IMappedEntityServiceCollection Map<TEntity, TEntityDto, TEntityInputDto>(this IMappedEntityServiceCollection mappedServices, Action<IMappedEntityServiceCollectionOptions<TEntity, TEntityDto, TEntityInputDto>>? configure = null)
    {
        mappedServices.Services.AddAutoMapper(e =>
        {
            e.CreateMap<TEntity, TEntityDto>();
            e.CreateMap<TEntityInputDto, TEntity>();
        });

        var options = new MappedEntityServiceCollectionOptions<TEntity, TEntityDto, TEntityInputDto>(mappedServices);
        configure?.Invoke(options);

        return mappedServices;
    }
}