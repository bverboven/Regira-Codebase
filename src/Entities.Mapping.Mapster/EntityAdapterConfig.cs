//using Microsoft.Extensions.DependencyInjection;
//using Regira.Entities.Attachments.Abstractions;
//using Regira.Entities.Attachments.Models;
//using Regira.Entities.Web.Attachments.Models;
//using Regira.Entities.Web.Attachments.Services;

//namespace Regira.Entities.Mapping.Mapster;

//public class EntityAdapterConfig(EntityAdapterConfigBuilder entityAdapterConfigBuilder)
//{
//    public EntityAdapterConfigBuilder HasAttachments<TEntityAttachment, TEntityAttachmentDto, TEntityAttachmentInputDto>()
//        where TEntityAttachment : IEntityAttachment<int, int, int, Attachment>
//        where TEntityAttachmentDto : EntityAttachmentDto<int, int, int>
//    {
//        entityAdapterConfigBuilder.MappingActions.Add(serviceProvider =>
//        {
//            entityAdapterConfigBuilder.Config.NewConfig<TEntityAttachment, TEntityAttachmentDto>()
//                .Map(
//                    dest => dest.Uri,
//                    src => serviceProvider.GetRequiredService<AttachmentUriResolver<TEntityAttachment>>().Resolve(src)
//                );
//            entityAdapterConfigBuilder.Config.NewConfig<TEntityAttachmentInputDto, TEntityAttachment>();
//        });
//        entityAdapterConfigBuilder.Services.AddTransient<AttachmentUriResolver<TEntityAttachment>>();

//        return entityAdapterConfigBuilder;
//    }
//}