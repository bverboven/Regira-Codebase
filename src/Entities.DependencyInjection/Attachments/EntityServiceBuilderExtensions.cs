using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Attachments;

public static class EntityServiceBuilderExtensions
{
    // Attachments
    public static EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int, Attachment> HasAttachments<TContext, TEntity, TEntityAttachment>
    (
        this IEntityServiceBuilder<TEntity, int> builder,
        Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int, Attachment>>? configure = null
    )
        where TContext : DbContext
        where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
        where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(builder.Services);

        configure?.Invoke(attachmentBuilder);

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

    public static EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>
        HasAttachments<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>
        (
            this IEntityServiceBuilder<TEntity, TKey> builder,
            Action<EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>>? configure = null
        )
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TKey, TAttachmentKey, TAttachment>
        where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TKey, TAttachmentKey, TAttachment>
        where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TKey>, new()
        where TAttachment : class, IAttachment<TAttachmentKey>, new()
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(builder.Services);

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