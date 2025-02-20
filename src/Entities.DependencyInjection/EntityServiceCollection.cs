using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Models;
using Regira.IO.Storage.Abstractions;
using Regira.Web.DependencyInjection;

namespace Regira.Entities.DependencyInjection;

public class EntityServiceCollection<TContext>(IServiceCollection services) : ServiceCollectionWrapper(services), IEntityServiceCollection<TContext> where TContext : DbContext
{
    // Default service
    public EntityServiceCollection<TContext> For<TEntity>(Action<EntityServiceBuilder<TContext, TEntity>>? configure = null)
        where TEntity : class, IEntity<int>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity>(this);
        configure?.Invoke(builder);
        
        // Query Builder
        if (!builder.HasService<IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }
        // Read Service
        if (!builder.HasService<IEntityReadService<TEntity, int>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity>>();
        }
        // Write Service
        if (!builder.HasService<IEntityWriteService<TEntity, int>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity>>();
        }
        // Entity Service
        if (!builder.HasService<IEntityService<TEntity>>())
        {
            builder.UseEntityService<EntityRepository<TEntity>>();
        }
        if (!builder.HasService<IEntityService<TEntity, int, SearchObject<int>>>())
        {
            Services.AddTransient<IEntityService<TEntity, int, SearchObject<int>>, EntityRepository<TEntity, int, SearchObject<int>>>();
        }

        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext,TEntity,TKey}.AddDefaultService"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="configure"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> For<TEntity, TKey>(Action<EntityServiceBuilder<TContext, TEntity, TKey>>? configure = null)
        where TEntity : class, IEntity<TKey>
    {
        var builder = new EntityServiceBuilder<TContext, TEntity, TKey>(this);
        configure?.Invoke(builder);
        
        // Query Builder
        if (!builder.HasService<IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }
        // Read Service
        if (!builder.HasService<IEntityReadService<TEntity, TKey>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity, TKey>>();
        }
        // Write Service
        if (!builder.HasService<IEntityWriteService<TEntity, TKey>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }
        // Entity Service
        if (!builder.HasEntityService())
        {
            builder.UseEntityService<EntityRepository<TEntity, TKey>>();
        }
        if (!builder.HasService<IEntityService<TEntity, TKey, SearchObject<TKey>>>())
        {
            Services.AddTransient<IEntityService<TEntity, TKey, SearchObject<TKey>>, EntityRepository<TEntity, TKey, SearchObject<TKey>>>();
        }

        return this;
    }
    public EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject>(Action<EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
    {
        var builder = new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>(this);
        configure?.Invoke(builder);

        if (!builder.HasService<IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }
        if (!builder.HasService<IEntityReadService<TEntity, TKey, TSearchObject>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity, TKey, TSearchObject>>();
        }
        if (!builder.HasService<IEntityWriteService<TEntity, TKey>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }
        if (!builder.HasEntityService())
        {
            builder.AddTransient<IEntityService<TEntity, TKey>, EntityRepository<TEntity, TKey, TSearchObject>>();
            builder.AddTransient<IEntityService<TEntity, TKey, TSearchObject>, EntityRepository<TEntity, TKey, TSearchObject>>();
        }

        return this;
    }
    public EntityServiceCollection<TContext> For<TEntity, TSearchObject, TSortBy, TIncludes>
        (Action<ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<int>
        where TSearchObject : class, ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var simpleBuilder = new EntityServiceBuilder<TContext, TEntity, int, TSearchObject>(this);
        var builder = new ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(simpleBuilder);
        configure?.Invoke(builder);

        if (!builder.HasService<IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }
        if (!builder.HasService<IEntityReadService<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity, TSearchObject, TSortBy, TIncludes>>();
        }
        if (!builder.HasService<IEntityWriteService<TEntity, int>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity>>();
        }
        if (!builder.HasEntityService())
        {
            builder.AddTransient<IEntityService<TEntity>, EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();

            builder.AddTransient<IEntityService<TEntity, int>, EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, int, TSearchObject>, EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>, EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
        }

        return this;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TSearchObject"></typeparam>
    /// <typeparam name="TSortBy"></typeparam>
    /// <typeparam name="TIncludes"></typeparam>
    /// <param name="configure"></param>
    /// <returns></returns>
    public EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
        (Action<ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var simpleBuilder = new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>(this);
        var builder = new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(simpleBuilder);
        configure?.Invoke(builder);

        if (!builder.HasService<IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }
        if (!builder.HasService<IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        }
        if (!builder.HasService<IEntityWriteService<TEntity, TKey>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }
        if (!builder.HasEntityService())
        {
            builder.AddTransient<IEntityService<TEntity, TKey>, EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, TKey, TSearchObject>, EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        }

        return this;
    }


    // Complex service with attachments
    public EntityServiceCollection<TContext> ConfigureAttachmentService(Func<IServiceProvider, IFileService> factory)
    {
        Services.AddTransient<IQueryBuilder<Attachment, int, AttachmentSearchObject>>(p =>
            new QueryBuilder<Attachment, int, AttachmentSearchObject>(
                p.GetServices<IGlobalFilteredQueryBuilder>(),
            [
                new AttachmentFilteredQueryBuilder(p.GetRequiredService<IQKeywordHelper>())
            ])
        );
        Services.AddTransient<IAttachmentService>(p
            => new AttachmentRepository<TContext>(
                p.GetRequiredService<TContext>(),
                factory(p),
                p.GetRequiredService<IEntityReadService<Attachment<int>, int, AttachmentSearchObject<int>>>(),
                p.GetRequiredService<IEntityWriteService<Attachment<int>, int>>()
            ));
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
        Services.AddTransient<IQueryBuilder<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>>(p =>
            new QueryBuilder<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>(
                p.GetServices<IGlobalFilteredQueryBuilder>(),
                [
                    new AttachmentFilteredQueryBuilder<TKey>(p.GetRequiredService<IQKeywordHelper>())
                ]
            )
        );
        Services
            .AddTransient<IAttachmentService<TKey>>(p
                => new AttachmentRepository<TContext, TKey>(
                    p.GetRequiredService<TContext>(),
                    factory(p),
                    p.GetRequiredService<IEntityReadService<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>>(),
                    p.GetRequiredService<IEntityWriteService<Attachment<TKey>, TKey>>()
                )
            )
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