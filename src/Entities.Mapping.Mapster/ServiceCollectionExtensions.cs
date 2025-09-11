using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Attachments.Services;

namespace Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseMapsterMapping(this IServiceCollection services, Action<TypeAdapterConfig, IServiceProvider>? configure = null)
    {
        services
            .AddSingleton(p =>
            {
                var config = new TypeAdapterConfig();

                // important to prevent stackoverflow!!
                config.Default.PreserveReference(true);

                // Generic mappings
                config.NewConfig<Attachment, AttachmentDto>();
                config.NewConfig<Attachment, AttachmentDto<int>>();
                config.NewConfig(typeof(Attachment<>), typeof(AttachmentDto<>));

                config.NewConfig<AttachmentInputDto, Attachment>();
                config.NewConfig<AttachmentInputDto<int>, Attachment>();
                config.NewConfig(typeof(AttachmentInputDto<>), typeof(Attachment<>));

                config.NewConfig(typeof(EntityAttachment<,,,>), typeof(EntityAttachmentDto<,,>));
                config.NewConfig<EntityAttachmentInputDto, EntityAttachment>();
                config.NewConfig(typeof(EntityAttachmentInputDto<,,>), typeof(EntityAttachment<,,,>));
                
                configure?.Invoke(config, p);

                return config;
            })
            .AddTransient<AttachmentUriResolver<EntityAttachment>>()
            .AddTransient<IEntityMapper, EntityMapper>()
            .AddMapster();

        return services;
    }
}