//using Microsoft.EntityFrameworkCore;
//using Regira.Entities.Abstractions;
//using Regira.Entities.Attachments.Abstractions;
//using Regira.Entities.Attachments.Models;
//using Regira.Entities.DependencyInjection.ServiceBuilders;
//using Regira.Entities.Models.Abstractions;

//namespace Regira.Entities.DependencyInjection.Attachments;

//public static class EntityServiceBuilderExtensions
//{
//    // Attachments
//    public static EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int> HasAttachments<TContext, TEntity, TEntityAttachment>
//    (
//        this EntityServiceBuilder<TContext, TEntity, int> builder,
//        Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int>>? configure = null
//    )
//        where TContext : DbContext
//        where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
//        where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
//    {
//        HasAttachments<TContext, TEntity, int, TEntityAttachment, int, int>(builder, configure);

//        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(builder.Services);
//        if (!attachmentBuilder.HasService<IEntityService<TEntityAttachment>>())
//        {
//            attachmentBuilder.AddDefaultAttachmentService();
//        }

//        if (!attachmentBuilder.HasEntityAttachmentMapping)
//        {
//            attachmentBuilder.WithDefaultMapping();
//        }

//        return attachmentBuilder;
//    }

//    public static EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>
//        HasAttachments<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>
//        (
//            this EntityServiceBuilder<TContext, TEntity, TKey> builder,
//            Action<EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>>? configure = null
//        )
//        where TContext : DbContext
//        where TEntity : class, IEntity<TKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TKey, TAttachmentKey, Attachment>
//        where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TKey, TAttachmentKey, Attachment>
//    {
//        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>(builder.Services);

//        configure?.Invoke(attachmentBuilder);

//        if (!attachmentBuilder.HasService<IEntityService<TEntityAttachment, TEntityAttachmentKey>>())
//        {
//            attachmentBuilder.AddDefaultAttachmentService();
//        }

//        if (!attachmentBuilder.HasEntityAttachmentMapping)
//        {
//            attachmentBuilder.WithDefaultMapping();
//        }

//        return attachmentBuilder;
//    }
//}