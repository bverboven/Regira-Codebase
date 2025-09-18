using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Web.Attachments.Services;

namespace Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static EntityServiceCollectionOptions UseMapsterMapping(this EntityServiceCollectionOptions options, Action<TypeAdapterConfig>? configure = null)
    {
        var config = new TypeAdapterConfig();

        options.EntityMapConfigurator = new EntityMapConfigurator(config);

        options.Services
            .AddSingleton(p =>
            {
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

                configure?.Invoke(config);

                return config;
            })
            .AddTransient<AttachmentUriResolver<EntityAttachment>>()
            .AddTransient<IEntityMapper, EntityMapper>()
            .AddMapster();

        return options;
    }
}