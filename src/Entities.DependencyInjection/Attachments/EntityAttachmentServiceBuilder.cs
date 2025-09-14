using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.Attachments;

public class EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(IServiceCollection services)
    : EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int, Attachment>(services),
        IEntityAttachmentServiceBuilder<TEntity, TEntityAttachment>
    where TContext : DbContext
    where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
    where TEntity : class, IEntity<int>, IHasAttachments<TEntityAttachment>
{
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> AddDefaultAttachmentServices()
    {
        For<TEntityAttachment>();

        base.AddDefaultAttachmentServices();

        return this;
    }
}

public class EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(IServiceCollection services)
    : EntitySearchObjectServiceBuilder<TContext, TEntityAttachment, TEntityAttachmentKey, TSearchObject>(services),
        IEntityAttachmentServiceBuilder<TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, new()
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    /// <summary>
    /// EntityAttachment and Attachment are strictly related (one on one), which implies removing the Attachment when removing the EntityAttachment
    /// Defaults to true
    /// </summary>
    public bool HasStrictRelation { get; set; } = true;
    public bool HasEntityAttachmentMapping { get; set; }
    
    /// <summary>
    /// Adds implementations for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntityAttachment}"/></item>
    ///     <item><see cref="IEntityService{TEntityAttachment,TKey}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> AddDefaultAttachmentServices()
    {
        For<TEntityAttachment, TEntityAttachmentKey, TSearchObject>(e =>
        {
            e.Includes((query, _) => query.Include(x => x.Attachment));
            e.AddQueryFilter<EntityAttachmentFilteredQueryBuilder<TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>>();
            e.Process<EntityAttachmentProcessor<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
            e.AddPrepper<EntityAttachmentPrepper<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
            e.UseWriteService<EntityAttachmentWriteService<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
        });

        return this;
    }

    // Related
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> RelatedAttachments(
        Expression<Func<TObject, ICollection<TEntityAttachment>?>> navigationExpression, Action<TObject>? prepareFunc = null, bool isStrictRelation = true)
    {
        Services.AddPrepper(p => new RelatedAttachmentsPrepper<TContext, TObject, TEntityAttachment, TObjectKey, TEntityAttachmentKey, TAttachmentKey, TAttachment>(
            p.GetRequiredService<TContext>(),
            navigationExpression,
            new RelatedAttachmentsPrepper<TContext, TObject, TEntityAttachment, TObjectKey, TEntityAttachmentKey, TAttachmentKey, TAttachment>.Options { IsStrictRelation = isStrictRelation })
        );

        if (prepareFunc != null)
        {
            Services.AddPrepper(prepareFunc);
        }

        return this;
    }
}
