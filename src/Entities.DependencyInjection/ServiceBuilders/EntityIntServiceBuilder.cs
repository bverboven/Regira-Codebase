﻿using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Preppers;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class EntityIntServiceBuilder<TContext, TEntity>
{
    public new bool HasEntityService() => HasService<IEntityService<TEntity>>();

    // Entity service
    public new EntityIntServiceBuilder<TContext, TEntity> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity>>();
    public new EntityIntServiceBuilder<TContext, TEntity> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity>, IEntityService<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityService<TEntity>, TService>();
        Services.AddTransient<IEntityService<TEntity, int>, TService>();
        Services.AddTransient<IEntityService<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public new EntityIntServiceBuilder<TContext, TEntity> UseEntityService<TService>(Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity>, IEntityService<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityService<TEntity>>(factory);
        Services.AddTransient<IEntityService<TEntity, int>>(factory);
        Services.AddTransient<IEntityService<TEntity, int, SearchObject<int>>>(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Entity Repository
    protected internal new EntityIntServiceBuilder<TContext, TEntity> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    protected internal new EntityIntServiceBuilder<TContext, TEntity> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, int>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, int, SearchObject<int>>>(factory);
        return this;
    }
    public new EntityIntServiceBuilder<TContext, TEntity> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
        return this;
    }
    public new EntityIntServiceBuilder<TContext, TEntity> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }

    // Entity Manager
    public new EntityIntServiceBuilder<TContext, TEntity> HasManager<TService>()
        where TService : class, IEntityManager<TEntity>, IEntityManager<TEntity, int, SearchObject<int>>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    public new EntityIntServiceBuilder<TContext, TEntity> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity>, IEntityManager<TEntity, int, SearchObject<int>>
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        Services.AddTransient<IEntityManager<TEntity, int>>(factory);
        Services.AddTransient<IEntityManager<TEntity, int, SearchObject<int>>>(factory);
        return this;
    }

    // Query Builders
    public new EntityIntServiceBuilder<TContext, TEntity> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity>();
        Services.UseQueryBuilder<TEntity, QueryBuilder<TEntity>>();
        return this;
    }
    public new EntityIntServiceBuilder<TContext, TEntity> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TImplementation>();
        return this;
    }
    public new EntityIntServiceBuilder<TContext, TEntity> UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TImplementation>(factory);
        return this;
    }

    // EntityNormalizers
    public new EntityIntServiceBuilder<TContext, TEntity> AddNormalizer<TNormalizer>()
        where TNormalizer : class, IEntityNormalizer<TEntity>
    {
        base.AddNormalizer<TNormalizer>();
        return this;
    }

    // Preppers
    public new EntityIntServiceBuilder<TContext, TEntity> Prepare(Func<TEntity, TContext, Task> prepareFunc)
    {
        Services.AddTransient<IEntityPrepper>(p => new EntityPrepper<TContext, TEntity, int>(p.GetRequiredService<TContext>(), prepareFunc));

        return this;
    }
    // Related
    public EntityIntServiceBuilder<TContext, TEntity> Related<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression)
        where TRelated : class, IEntity<int>
    {
        Services.AddPrepper(p => new RelatedCollectionPrepper<TContext, TEntity, TRelated, int, int>(p.GetRequiredService<TContext>(), navigationExpression));

        return this;
    }
}