using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Attachments;

public static class EntityServiceBuilderExtensions
{
    // Attachments
    public static EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int, Attachment> HasAttachments<TContext, TEntity, TEntityAttachment>
    (
        this EntityServiceBuilder<TContext, TEntity> builder,
        Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int, Attachment>>? configure = null
    )
        where TContext : DbContext
        where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
        where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(builder.Services);
        if (!attachmentBuilder.HasService<IEntityService<TEntityAttachment>>())
        {
            attachmentBuilder.AddDefaultAttachmentServices();
        }

        if (!attachmentBuilder.HasEntityAttachmentMapping)
        {
            attachmentBuilder.WithDefaultMapping();
        }

        return attachmentBuilder;
    }

    public static EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>
        HasAttachments<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>
        (
            this EntityServiceBuilder<TContext, TEntity, TKey> builder,
            Action<EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>>? configure = null
        )
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TKey, TAttachmentKey, TAttachment>
        where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TKey, TAttachmentKey, TAttachment>
        where TAttachment : class, IAttachment<TAttachmentKey>, new()
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>(builder.Services);

        configure?.Invoke(attachmentBuilder);

        if (!attachmentBuilder.HasService<IEntityService<TEntityAttachment, TEntityAttachmentKey>>())
        {
            attachmentBuilder.AddDefaultAttachmentServices();
        }

        if (!attachmentBuilder.HasEntityAttachmentMapping)
        {
            attachmentBuilder.WithDefaultMapping();
        }

        return attachmentBuilder;
    }
}