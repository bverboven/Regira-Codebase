using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Extensions;

public static class EntityServiceBuilderExtensions
{
    // Attachments
    public static EntityServiceBuilder<TContext, TEntity, int> HasAttachments<TContext, TEntity, TEntityAttachment>
    (
        this EntityServiceBuilder<TContext, TEntity, int> builder,
        Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int>>? configure = null
    )
        where TContext : DbContext
        where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
        where TEntityAttachment : class, IEntityAttachment
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(builder.Services);
        configure?.Invoke(attachmentBuilder);
        if (!attachmentBuilder.HasEntityAttachmentMapping)
        {
            attachmentBuilder.WithDefaultMapping();
        }
        attachmentBuilder.AddDefaultAttachmentService();
        return builder;
    }
    public static EntityServiceBuilder<TContext, TEntity, TKey> HasAttachments<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>
    (
        this EntityServiceBuilder<TContext, TEntity, TKey> builder,
        Action<EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>>? configure = null
    )
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>, IHasAttachments, IHasAttachments<TEntityAttachment>, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TKey, TAttachmentKey>
        where TEntityAttachment : class, IEntityAttachment, IEntityAttachment<TEntityAttachmentKey, TKey, TAttachmentKey>
    {
        var attachmentBuilder = new EntityAttachmentServiceBuilder<TContext, TEntity, TKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>(builder.Services);
        configure?.Invoke(attachmentBuilder);
        if (!attachmentBuilder.HasEntityAttachmentMapping)
        {
            attachmentBuilder.WithDefaultMapping();
        }
        attachmentBuilder.AddDefaultAttachmentService();
        return builder;
    }
}