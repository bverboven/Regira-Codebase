using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Mapping.Abstractions;
using System.Reflection;
using Regira.Entities.Mapping.Models;

namespace Regira.Entities.Mapping.AutoMapper;

public static class ServiceCollectionExtensions
{
    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, Action<IServiceProvider, IMapperConfigurationExpression>? configure = null, IEnumerable<Assembly>? assemblies = null)
    {
        options.EntityMapConfigurator = new EntityMapConfigurator(options.Services);
        
        options.Services
            .AddAutoMapper((p, e) =>
            {
                e.AllowNullCollections = true;

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

        return options;
    }
}