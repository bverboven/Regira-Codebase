using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Models;
using Regira.IO.Storage.Abstractions;
using Regira.Web.DependencyInjection;

namespace Regira.Entities.DependencyInjection;

public class EntityServiceCollection<TContext> : ServiceCollectionWrapper
    where TContext : DbContext
{
    public EntityServiceCollection(IServiceCollection services)
        : base(services)
    {
    }

    // Entity service
    public EntityServiceCollection<TContext> For<TEntity, TService>(
        Action<EntityServiceBuilder<TContext, TEntity, int>>? configure = null)
        where TEntity : class, IEntity<int>
        where TService : class, IEntityService<TEntity>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity>(this)
            .AddService<TService>();
        configure?.Invoke(builder);

        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext, TEntity}.AddService{TService}"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TService"></typeparam>
    /// <param name="configure"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> For<TEntity, TKey, TService>(Action<EntityServiceBuilder<TContext, TEntity, TKey>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TService : class, IEntityService<TEntity, TKey>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity, TKey>(this)
            .AddService<TService>();
        configure?.Invoke(builder);

        return this;
    }

    // Default service
    public EntityServiceCollection<TContext> For<TEntity>(Action<EntityServiceBuilder<TContext, TEntity, int>>? configure = null)
        where TEntity : class, IEntity<int>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity>(this)
            .AddDefaultService();
        configure?.Invoke(builder);

        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext, TEntity}.AddDefaultService"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="configure"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> For<TEntity, TKey>(Action<EntityServiceBuilder<TContext, TEntity, TKey>>? configure = null)
        where TEntity : class, IEntity<TKey>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity, TKey>(this)
            .AddDefaultService();
        configure?.Invoke(builder);

        return this;
    }


    // Service with attachments
    //public EntityServiceCollection<TContext> For<TEntity, TEntityAttachment, TService>(
    //    Action<EntityServiceBuilder<TContext, TEntity, int>>? configure = null,
    //    Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int>>?
    //        configureAttachments = null
    //)
    //    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    //    where TEntityAttachment : class, IEntityAttachment
    //    where TService : class, IEntityService<TEntity>
    //{
    //    var builder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(this)
    //        .AddDefaultAttachmentService()
    //        .WithDefaultMapping();
    //    configureAttachments?.Invoke(builder);
        
    //    return For<TEntity, TService>(configure);
    //}
    ///// <summary>
    ///// <inheritdoc cref="EntityAttachmentServiceBuilder{TContext, TEntity, TEntityAttachment}.AddDefaultAttachmentService"/>
    ///// <inheritdoc cref="EntityAttachmentServiceBuilder{TContext, TEntity, TEntityAttachment}.WithDefaultMapping"/>
    ///// <inheritdoc cref="EntityServiceBuilder{TContext, TEntity}.AddService{TService}"/>
    ///// </summary>
    ///// <typeparam name="TEntity"></typeparam>
    ///// <typeparam name="TEntityKey"></typeparam>
    ///// <typeparam name="TEntityAttachment"></typeparam>
    ///// <typeparam name="TEntityAttachmentKey"></typeparam>
    ///// <typeparam name="TAttachmentKey"></typeparam>
    ///// <typeparam name="TService"></typeparam>
    ///// <param name="configure"></param>
    ///// <param name="configureAttachments"></param>
    ///// <returns></returns>
    //public EntityServiceCollection<TContext> For<TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey, TService>(
    //    Action<EntityServiceBuilder<TContext, TEntity, TEntityKey>>? configure = null,
    //    Action<EntityAttachmentServiceBuilder<TContext, TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>>? configureAttachments = null
    //)
    //    where TEntity : class, IEntity<TEntityKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TEntityKey, TAttachmentKey>
    //    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TEntityKey, TAttachmentKey>
    //    where TService : class, IEntityService<TEntity, TEntityKey>
    //{
    //    var builder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>(this)
    //        .AddDefaultAttachmentService()
    //        .WithDefaultMapping();
    //    configureAttachments?.Invoke(builder);

    //    return For<TEntity, TEntityKey, TService>(configure);
    //}

    // Default service with attachments
    //public EntityServiceCollection<TContext> For<TEntity, TEntityAttachment>(
    //    Action<EntityServiceBuilder<TContext, TEntity, int>>? configure = null,
    //    // ReSharper disable once MethodOverloadWithOptionalParameter
    //    Action<EntityAttachmentServiceBuilder<TContext, TEntity, int, TEntityAttachment, int, int>>?
    //        configureAttachments = null
    //)
    //    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    //    where TEntityAttachment : class, IEntityAttachment
    //{
    //    var builder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityAttachment>(this)
    //        .AddDefaultAttachmentService()
    //        .WithDefaultMapping();
    //    configureAttachments?.Invoke(builder);

    //    return For(configure);
    //}
    ///// <summary>
    ///// <inheritdoc cref="EntityAttachmentServiceBuilder{TContext, TEntity, TEntityAttachment}.AddDefaultAttachmentService"/>
    ///// <inheritdoc cref="EntityAttachmentServiceBuilder{TContext, TEntity, TEntityAttachment}.WithDefaultMapping"/>
    ///// <inheritdoc cref="EntityServiceCollection{TContext}.For{TEntity}"/>
    ///// </summary>
    ///// <typeparam name="TEntity"></typeparam>
    ///// <typeparam name="TEntityKey"></typeparam>
    ///// <typeparam name="TEntityAttachment"></typeparam>
    ///// <typeparam name="TEntityAttachmentKey"></typeparam>
    ///// <typeparam name="TAttachmentKey"></typeparam>
    ///// <param name="configure"></param>
    ///// <param name="configureAttachments"></param>
    ///// <returns></returns>
    //public EntityServiceCollection<TContext> For<TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>(
    //    Action<EntityServiceBuilder<TContext, TEntity, TEntityKey>>? configure = null,
    //    // ReSharper disable once MethodOverloadWithOptionalParameter
    //    Action<EntityAttachmentServiceBuilder<TContext, TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>>? configureAttachments = null
    //)
    //    where TEntity : class, IEntity<TEntityKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TEntityKey, TAttachmentKey>
    //    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TEntityKey, TAttachmentKey>
    //{
    //    var builder = new EntityAttachmentServiceBuilder<TContext, TEntity, TEntityKey, TEntityAttachment, TEntityAttachmentKey, TAttachmentKey>(this)
    //        .AddDefaultAttachmentService()
    //        .WithDefaultMapping();
    //    configureAttachments?.Invoke(builder);

    //    return For(configure);
    //}


    // Complex service
    public EntityServiceCollection<TContext> For<TEntity, TService, TSearchObject, TSortBy, TIncludes>(
        Action<ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TService : class, IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity>
        where TEntity : class, IEntity<int>
        where TSearchObject : class, ISearchObject, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var builder = new EntityServiceBuilder<TContext, TEntity>(this)
            .AddComplexService<TService, TSearchObject, TSortBy, TIncludes>();
        configure?.Invoke(builder);
        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext, TEntity}.AddComplexService{TService, TSearchObject, TSortBy, TIncludes}"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TSearchObject"></typeparam>
    /// <typeparam name="TSortBy"></typeparam>
    /// <typeparam name="TIncludes"></typeparam>
    /// <param name="configure"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> For<TEntity, TKey, TService, TSearchObject, TSortBy, TIncludes>(Action<ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TService : class, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var builder = new EntityServiceBuilder<TContext, TEntity, TKey>(this)
            .AddComplexService<TService, TSearchObject, TSortBy, TIncludes>();
        configure?.Invoke(builder);

        return this;
    }

    // Complex service with attachments
    public EntityServiceCollection<TContext> ConfigureAttachmentService(Func<IServiceProvider, IFileService> factory)
    {
        Services.AddTransient<IAttachmentService>(p 
            => new AttachmentRepository<TContext>(p.GetRequiredService<TContext>(), factory(p)));
        return ConfigureAttachmentService<int>(factory);
    }

    /// <summary>
    /// Adds <see cref="IAttachmentService"/> to <see cref="IServiceCollection"/> with an implementation of <see cref="IFileService"/>.<br />
    /// Adds <see cref="IMappingExpression">AutoMapper maps</see> for <see cref="Attachment" /> to <see cref="AttachmentDto"/> and <see cref="AttachmentInputDto"/> to <see cref="Attachment" />.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> ConfigureAttachmentService<TKey>(Func<IServiceProvider, IFileService> factory)
    {
        Services
            .AddTransient<IAttachmentService<TKey>>(p 
                => new AttachmentRepository<TContext, TKey>(p.GetRequiredService<TContext>(), factory(p)))
            .AddAutoMapper(cfg =>
            {
                cfg.CreateMap<Attachment<TKey>, AttachmentDto<TKey>>()
                    .ReverseMap();
                cfg.CreateMap<AttachmentInputDto<TKey>, Attachment<TKey>>();
                //cfg.CreateMap<EntityAttachmentBase, EntityAttachmentDto>()
                //    .IncludeAllDerived();
            });
        return this;
    }
    /// <summary>
    /// Adds <see cref="ITypedAttachmentService"/> to <see cref="IServiceCollection"/>
    /// using a collection of <see cref="AttachmentQuerySetDescriptor{T}"/>
    /// </summary>
    /// <param name="queryFactory"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> ConfigureTypedAttachmentService(Func<TContext, IList<IAttachmentQuerySetDescriptor>> queryFactory)
    {
        Services.AddTransient<ITypedAttachmentService>(p
            => new TypedAttachmentService<TContext>(p.GetRequiredService<TContext>(), queryFactory));
        return this;
    }
    public EntityServiceCollection<TContext> ConfigureTypedAttachmentService<TService>()
        where TService : class, ITypedAttachmentService
    {
        Services.AddTransient<ITypedAttachmentService, TService>();
        return this;
    }


    // helpers
    public EntityServiceCollection<TContext> AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddTransient<TService, TImplementation>();

        return this;
    }
    public EntityServiceCollection<TContext> AddTransient<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        Services.AddTransient(factory);

        return this;
    }
}