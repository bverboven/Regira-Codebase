using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public class MappedEntityServiceBuilder<TContext, TEntity, TKey>(EntityServiceCollectionOptions options) : EntityServiceBuilder<TContext, TEntity, TKey>(options)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>()
        where TImplementation : class, IEntityAfterMapper
    {
        Services.AddTransient<IEntityAfterMapper, TImplementation>();
        return this;
    }
    public MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityAfterMapper
    {
        Services.AddTransient<IEntityAfterMapper>(factory);
        return this;
    }
    public MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TTarget>(Action<TEntity, TTarget> afterMapAction)
    {
        Services.AddTransient<IEntityAfterMapper>(_ => new EntityAfterMapper<TEntity, TTarget>(afterMapAction));
        return this;
    }
}