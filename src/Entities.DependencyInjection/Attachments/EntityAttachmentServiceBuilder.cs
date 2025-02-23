using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Processing.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Mappings;
using Regira.Entities.Web.Attachments.Models;

namespace Regira.Entities.DependencyInjection.Attachments;

public class EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(IServiceCollection services)
    : EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int, Attachment>(services),
        IEntityAttachmentServiceBuilder<TEntity, TEntityAttachment>
    where TContext : DbContext
    where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
{
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> WithDefaultMapping()
    {
        base.WithDefaultMapping();
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, EntityAttachmentDto>>();
        return this;
    }
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> AddDefaultAttachmentServices()
    {
        base.AddDefaultAttachmentServices();

        For<TEntityAttachment, int, EntityAttachmentSearchObject>(e =>
        {
            e.AddQueryFilter<EntityAttachmentFilteredQueryBuilder<TEntityAttachment, EntityAttachmentSearchObject>>();
            e.AddTransient<IEntityProcessor, EntityAttachmentProcessor>();
            e.HasRepository<EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
            e.AddTransient<IEntityRepository<TEntityAttachment>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
            e.AddTransient<IEntityService<TEntityAttachment>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        });

        return this;
    }
}

public class
    EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TObject, TObjectKey>(services),
        IEntityAttachmentServiceBuilder<TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    protected internal bool HasEntityAttachmentMapping { get; set; }

    /// <summary>
    /// Adds AutoMapper maps
    /// <list type="bullet">
    ///     <item><typeparamref name="TEntityAttachment"/> -&gt; <see cref="EntityAttachmentDto"/></item>
    ///     <item><see cref="EntityAttachmentInputDto"/> -&gt; <typeparamref name="TEntityAttachment"/></item>
    /// </list>
    /// An implementation of <see cref="AttachmentUriResolver{TEntity,TEntityAttachmentKey,TObjectKey,TAttachmentKey,TAttachment,TDto}"/> resolves the <see cref="EntityAttachmentDto.Uri"/> property
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment> WithDefaultMapping()
    {
        Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntityAttachment, EntityAttachmentDto>()
                .ForMember(
                    x => x.Uri,
                    opt => opt.MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, EntityAttachmentDto>>()
                );
            cfg.CreateMap<EntityAttachmentInputDto, TEntityAttachment>();
        });
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, EntityAttachmentDto>>();
        HasEntityAttachmentMapping = true;

        return this;
    }

    public new EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment> AddMapping<TEntityAttachmentDto, TEntityAttachmentInputDto>()
        where TEntityAttachmentDto : EntityAttachmentDto
    {
        Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntityAttachment, TEntityAttachmentDto>()
                .ForMember(
                    x => x.Uri,
                    opt => opt.MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, TEntityAttachmentDto>>()
                );
            cfg.CreateMap<TEntityAttachmentInputDto, TEntityAttachment>();
        });
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, TEntityAttachmentDto>>();
        HasEntityAttachmentMapping = true;

        return this;
    }

    /// <summary>
    /// Adds implementations for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntityAttachment}"/></item>
    ///     <item><see cref="IEntityService{TEntityAttachment,TKey}"/></item>
    ///     <item><see cref="AttachmentUriResolver{TEntity,TEntityAttachmentKey,TObjectKey,TAttachmentKey,TAttachment,TDto}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment> AddDefaultAttachmentServices()
    {
        For<TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>>(e =>
        {
            e.Includes(query => query.Include(x => x.Attachment));
            e.AddQueryFilter<EntityAttachmentFilteredQueryBuilder<TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey, TAttachment>>();
            e.AddTransient<IEntityProcessor, EntityAttachmentProcessor<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
            e.HasRepository<EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey, TAttachment>>();
        });

        return this;
    }
}

