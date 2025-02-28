using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Mappings;
using Regira.Entities.Web.Attachments.Models;

namespace Regira.Entities.DependencyInjection.Attachments;

public class EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(IServiceCollection services)
    : EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int, Attachment>(services),
        IEntityAttachmentServiceBuilder<TEntity, TEntityAttachment>
    where TContext : DbContext
    where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new()
    where TEntity : class, IEntity<int>, IHasAttachments<TEntityAttachment>
{
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> WithDefaultMapping()
    {
        base.WithDefaultMapping();
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, EntityAttachmentDto>>();
        return this;
    }
}

public class EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntityAttachment, TEntityAttachmentKey, TSearchObject>(services),
        IEntityAttachmentServiceBuilder<TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TAttachment>
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, new()
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
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> WithDefaultMapping()
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

    public new EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> AddMapping<TEntityAttachmentDto, TEntityAttachmentInputDto>()
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
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment> AddDefaultAttachmentServices()
    {
        For<TEntityAttachment, TEntityAttachmentKey, TSearchObject>(e =>
        {
            e.AddTransient<
                IFileIdentifierGenerator<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>,
                DefaultFileIdentifierGenerator<TObject, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
            >();
            e.Includes((query, _) => query.Include(x => x.Attachment));
            e.AddQueryFilter<EntityAttachmentFilteredQueryBuilder<TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>>();
            e.Process<EntityAttachmentProcessor<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
            e.AddPrepper<EntityAttachmentPrepper<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
            e.UseWriteService<EntityAttachmentWriteService<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>>();
        });

        return this;
    }
}
