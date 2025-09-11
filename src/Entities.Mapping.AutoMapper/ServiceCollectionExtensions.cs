using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Models;
using System.Reflection;

namespace Regira.Entities.Mapping.AutoMapper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseAutoMapperMapping(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configure, IEnumerable<Assembly>? assemblies = null)
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

                configure.Invoke(p, e);
            }, assemblies ?? [])
            .AddTransient<AttachmentUriResolver<EntityAttachment, EntityAttachmentDto>>()
            .AddTransient<IEntityMapper, EntityMapper>();

        return services;
    }
}