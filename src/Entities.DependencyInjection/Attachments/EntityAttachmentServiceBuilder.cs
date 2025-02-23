using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
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
        // Query filter
        Services.AddTransient<IFilteredQueryBuilder<TEntityAttachment, int, EntityAttachmentSearchObject>,
            EntityAttachmentFilteredQueryBuilder<TEntityAttachment, EntityAttachmentSearchObject>>();
        // Query builder
        Services.AddDefaultQueryBuilder<TEntityAttachment, int, EntityAttachmentSearchObject, EntitySortBy, EntityIncludes>();
        // Read service
        Services.AddTransient<IEntityReadService<TEntityAttachment, int>, EntityReadService<TContext, TEntityAttachment, int, EntityAttachmentSearchObject>>();
        Services.AddTransient<IEntityReadService<TEntityAttachment, int, EntityAttachmentSearchObject>, EntityReadService<TContext, TEntityAttachment, int, EntityAttachmentSearchObject>>();
        // Write service
        Services.AddTransient<IEntityWriteService<TEntityAttachment, int>, EntityWriteService<TContext, TEntityAttachment>>();
        // Repository
        Services.AddTransient<IEntityRepository<TEntityAttachment>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        Services.AddTransient<IEntityRepository<TEntityAttachment, int>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        Services.AddTransient<IEntityRepository<TEntityAttachment, int, EntityAttachmentSearchObject>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        // Entity Service
        Services.AddTransient<IEntityService<TEntityAttachment>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        Services.AddTransient<IEntityService<TEntityAttachment, int>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();
        Services.AddTransient<IEntityService<TEntityAttachment, int, EntityAttachmentSearchObject>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>>();

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
        // Query filter
        Services.AddTransient<IFilteredQueryBuilder<TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>>,
            EntityAttachmentFilteredQueryBuilder<TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey, TAttachment>>();
        // Query builder
        Services.AddDefaultQueryBuilder<TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, EntitySortBy, EntityIncludes>();
        // Read service
        Services.AddTransient<IEntityReadService<TEntityAttachment, TEntityAttachmentKey>, EntityReadService<TContext, TEntityAttachment, TEntityAttachmentKey>>();
        Services.AddTransient<IEntityReadService<TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>>, EntityReadService<TContext, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>>>();
        // Write service
        Services.AddTransient<IEntityWriteService<TEntityAttachment, TEntityAttachmentKey>, EntityWriteService<TContext, TEntityAttachment, TEntityAttachmentKey>>();
        // Repository
        Services.AddTransient<IEntityService<TEntityAttachment, TEntityAttachmentKey>,
            EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey, TAttachment>>();
        Services.AddTransient<IEntityService<TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>>,
            EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey, TAttachment>>();

        return this;
    }
}