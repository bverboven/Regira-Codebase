using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Mappings;
using Regira.Entities.Web.Attachments.Models;

namespace Regira.Entities.DependencyInjection;

public class EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(IServiceCollection services)
    : EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int>(services),
        IEntityAttachmentServiceBuilder<TEntity, TEntityAttachment>
    where TContext : DbContext
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    where TEntityAttachment : class, IEntityAttachment
{
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> WithDefaultMapping()
    {
        base.WithDefaultMapping();
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, EntityAttachmentDto>>();
        return this;
    }
    public new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment> AddDefaultAttachmentService()
    {
        base.AddDefaultAttachmentService();
        Services.AddTransient<IEntityService<TEntityAttachment>, EntityAttachmentRepository<TContext, TEntity, TEntityAttachment>>();

        return this;
    }
}
public class
    EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey,
        TAttachmentKey>(IServiceCollection services) : EntityServiceBuilder<TContext, TObject, TObjectKey>(services),
    IEntityAttachmentServiceBuilder<TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments,
    IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey>
{
    protected internal bool HasEntityAttachmentMapping { get; set; }

    /// <summary>
    /// Adds AutoMapper maps
    /// <list type="bullet">
    ///     <item><typeparamref name="TEntityAttachment"/> -&gt; <see cref="EntityAttachmentDto"/></item>
    ///     <item><see cref="EntityAttachmentInputDto"/> -&gt; <typeparamref name="TEntityAttachment"/></item>
    /// </list>
    /// An implementation of <see cref="AttachmentUriResolver{TEntity,TEntityAttachmentKey,TObjectKey,TAttachmentKey,TDto}"/> resolves the <see cref="EntityAttachmentDto.Uri"/> property
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey> WithDefaultMapping()
    {
        Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntityAttachment, EntityAttachmentDto>()
                .ForMember(
                    x => x.Uri,
                    opt => opt.MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, EntityAttachmentDto>>()
                );
            cfg.CreateMap<EntityAttachmentInputDto, TEntityAttachment>();
        });
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, EntityAttachmentDto>>();
        HasEntityAttachmentMapping = true;

        return this;
    }

    public new EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey> AddMapping<TEntityAttachmentDto, TEntityAttachmentInputDto>()
        where TEntityAttachmentDto : EntityAttachmentDto
    {
        Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntityAttachment, TEntityAttachmentDto>()
                .ForMember(
                    x => x.Uri,
                    opt => opt.MapFrom<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TEntityAttachmentDto>>()
                );
            cfg.CreateMap<TEntityAttachmentInputDto, TEntityAttachment>();
        });
        Services.AddTransient<AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TEntityAttachmentDto>>();
        HasEntityAttachmentMapping = true;

        return this;
    }
    /// <summary>
    /// Adds implementations for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntityAttachment}"/></item>
    ///     <item><see cref="IEntityService{TEntityAttachment,TKey}"/></item>
    ///     <item><see cref="AttachmentUriResolver{TEntity,TEntityAttachmentKey,TObjectKey,TAttachmentKey,TDto}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityAttachmentServiceBuilder<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey> AddDefaultAttachmentService()
    {
        Services.AddTransient<IEntityService<TEntityAttachment, TEntityAttachmentKey>,
            EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, EntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, TAttachmentKey>>();
        return this;
    }
}