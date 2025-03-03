using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class EntityIntServiceBuilder<TContext, TEntity>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, int>(services),
        IEntityServiceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    public new EntityIntServiceBuilder<TContext, TEntity, TSearchObject> WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<int>, new()
    {
        return new EntityIntServiceBuilder<TContext, TEntity, TSearchObject>(Services);
    }
}
public class EntityIntServiceBuilder<TContext, TEntity, TSearchObject>(IServiceCollection services)
    : EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject>(services), IEntityServiceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
{
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var simpleBuilder = new EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject>(this);
        var builder = new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(simpleBuilder);
        //configure?.Invoke(builder);

        // Query Builder
        if (!builder.HasService<IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.AddDefaultQueryBuilder();
        }

        // Read Service
        if (!builder.HasService<IEntityReadService<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.UseReadService<EntityReadService<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>>();
        }
        // Write Service
        if (!builder.HasService<IEntityWriteService<TEntity, int>>())
        {
            builder.UseWriteService<EntityWriteService<TContext, TEntity, int>>();
        }

        // Entity Repository
        if (!builder.HasService<IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>())
        {
            builder.HasRepositoryInner<EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();
        }

        // Entity Service
        if (!builder.HasEntityService())
        {
            builder.AddTransient<IEntityService<TEntity, int>, EntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, int, TSearchObject>, EntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>();
            builder.AddTransient<IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>, EntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>();
        }

        return builder;
    }
}

public partial class ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(
    EntitySearchObjectServiceBuilder<TContext, TEntity, int, TSearchObject> services)
    : ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;


public partial class EntityServiceBuilder<TContext, TEntity, TKey>(IServiceCollection services)
    : EntityServiceCollection<TContext>(services),
        IEntityServiceBuilder<TContext, TEntity, TKey>//,
                                                      //IEntityServiceBuilderImplementation<EntityServiceBuilder<TContext, TEntity, TKey>, TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject> WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<TKey>, new()
        => new(Services);
}
public partial class EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, TKey>(services),
        IEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Complex<TSortBy, TIncludes>()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
        => new(this);
}

public partial class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject> services)
    : EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;