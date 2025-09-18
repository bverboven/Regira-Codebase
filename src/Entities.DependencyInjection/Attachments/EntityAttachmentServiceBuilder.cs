using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.Attachments;

public class EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(EntityServiceCollectionOptions options)
    : EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int, Attachment>(options),
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

public class EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(EntityServiceCollectionOptions options)
    : EntitySearchObjectServiceBuilder<TContext, TEntityAttachment, TEntityAttachmentKey, TSearchObject>(options),
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
    /// Adds AutoMapper maps
    /// <list type="bullet">
    ///     <item><typeparamref name="TEntityAttachment"/> -&gt; <see cref="EntityAttachmentDto"/></item>
    ///     <item><see cref="EntityAttachmentInputDto"/> -&gt; <typeparamref name="TEntityAttachment"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> WithDefaultMapping()
        => AddMapping<EntityAttachmentDto, EntityAttachmentInputDto>();
    /// <summary>
    /// Adds mapping configurations for the specified entity attachment DTO and input DTO types.
    /// </summary>
    /// <remarks>This method configures mappings between the entity attachment type and the specified DTO
    /// types. It also sets the <c>HasEntityAttachmentMapping</c> flag to <see langword="true"/>.</remarks>
    /// <typeparam name="TEntityAttachmentDto">The type of the entity attachment DTO to be mapped.</typeparam>
    /// <typeparam name="TEntityAttachmentInputDto">The type of the entity attachment input DTO to be mapped.</typeparam>
    /// <returns>The current <see cref="EntityAttachmentServiceBuilder{TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment}"/> instance, allowing for method chaining.</returns>
    public new EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> AddMapping<TEntityAttachmentDto, TEntityAttachmentInputDto>()
        where TEntityAttachmentDto : EntityAttachmentDto
    {
        Options.EntityMapConfigurator.ConfigureAttachment<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, TEntityAttachmentDto>();
        Options.EntityMapConfigurator.Configure<TEntityAttachmentInputDto, TEntityAttachment>();

        HasEntityAttachmentMapping = true;

        return this;
    }


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
