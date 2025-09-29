using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class EntityIntServiceBuilder<TContext, TEntity>(EntityServiceCollectionOptions options)
    : EntityServiceBuilder<TContext, TEntity, int>(options),
        IEntityServiceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    public new EntityIntServiceBuilder<TContext, TEntity, TSearchObject> WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<int>, new()
    {
        return new EntityIntServiceBuilder<TContext, TEntity, TSearchObject>(Options);
    }

    public override void Build()
    {
        base.Build();

        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>>())
        {
            AddDefaultQueryBuilder();
        }
        // Read Service
        if (!HasService<IEntityReadService<TEntity, int, SearchObject<int>>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, int, SearchObject<int>>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, int>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity>>();
        }
        // Entity Repository
        if (!HasService<IEntityRepository<TEntity>>())
        {
            HasRepositoryInner<EntityRepository<TEntity>>();
        }
        // Entity Service
        if (!HasService<IEntityService<TEntity>>())
        {
            UseEntityService<EntityRepository<TEntity>>();
        }
    }
}
public class EntityIntServiceBuilder<TContext, TEntity, TSearchObject>(EntityServiceCollectionOptions options)
    : EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject>(options), IEntityServiceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
{
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var simpleBuilder = new EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject>(Options);
        var builder = new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(simpleBuilder);

        return builder;
    }

    public override void Build()
    {
        base.Build();

        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, int, TSearchObject, EntitySortBy, EntityIncludes>>())
        {
            AddDefaultQueryBuilder();
        }
        // Read Service
        if (!HasService<IEntityReadService<TEntity, int, TSearchObject>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, int, TSearchObject>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, int>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity, int>>();
        }
        // Entity Repository
        if (!HasService<IEntityRepository<TEntity, int, TSearchObject>>())
        {
            HasRepositoryInner<EntityRepository<TEntity, int, TSearchObject>>();
        }
        // Entity Service
        if (!HasService<IEntityService<TEntity, int, TSearchObject>>())
        {
            UseEntityService<EntityRepository<TEntity, int, TSearchObject>>();
        }
    }
}

public partial class ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(
    EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject> services)
    : ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{

    public override void Build()
    {
        base.Build();

        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            AddDefaultQueryBuilder();
        }

        // Read Service
        if (!HasService<IEntityReadService<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, int>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity, int>>();
        }

        // Entity Repository
        if (!HasService<IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            HasRepositoryInner<EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
        }

        // Entity Service
        if (!HasEntityService())
        {
            UseEntityService<EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
        }
    }
}


public partial class EntityServiceBuilder<TContext, TEntity, TKey>(EntityServiceCollectionOptions options)
    : EntityServiceCollection<TContext>(options),
        IEntityServiceBuilder<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject> WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<TKey>, new()
        => new(Options);

    public virtual void Build()
    {
        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>>())
        {
            AddDefaultQueryBuilder();
        }
        // Read Service
        if (!HasService<IEntityReadService<TEntity, TKey, SearchObject<TKey>>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, TKey, SearchObject<TKey>>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, TKey>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }
        // Entity Repository
        if (!HasService<IEntityRepository<TEntity, TKey>>())
        {
            HasRepositoryInner<EntityRepository<TEntity, TKey>>();
        }
        // Entity Service
        if (!HasService<IEntityService<TEntity, TKey>>())
        {
            UseEntityService<EntityRepository<TEntity, TKey>>();
        }
    }
}
public partial class EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>(EntityServiceCollectionOptions options)
    : EntityServiceBuilder<TContext, TEntity, TKey>(options),
        IEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
        => new(this);

    public override void Build()
    {
        base.Build();

        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>>())
        {
            AddDefaultQueryBuilder();
        }
        // Read Service
        if (!HasService<IEntityReadService<TEntity, TKey, TSearchObject>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, TKey, TSearchObject>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, TKey>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }
        // Entity Repository
        if (!HasService<IEntityRepository<TEntity, TKey, TSearchObject>>())
        {
            HasRepositoryInner<EntityRepository<TEntity, TKey, TSearchObject>>();
        }
        // Entity Service
        if (!HasService<IEntityService<TEntity, TKey, TSearchObject>>())
        {
            UseEntityService<EntityRepository<TEntity, TKey, TSearchObject>>();
        }
    }
}

public partial class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject> builder)
    : EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>(builder.Options)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public override void Build()
    {
        base.Build();

        // Query Builder
        if (!HasService<IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>())
        {
            AddDefaultQueryBuilder();
        }

        // Read Service
        if (!HasService<IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>())
        {
            UseReadService<EntityReadService<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        }
        // Write Service
        if (!HasService<IEntityWriteService<TEntity, TKey>>())
        {
            UseWriteService<EntityWriteService<TContext, TEntity, TKey>>();
        }

        // Entity Repository
        if (!HasService<IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>())
        {
            HasRepositoryInner<EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        }

        // Entity Service
        if (!HasEntityService())
        {
            UseEntityService<EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        }
    }
}