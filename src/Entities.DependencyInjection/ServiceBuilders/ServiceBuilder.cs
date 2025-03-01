using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Web.DependencyInjection;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public class EntityServiceCollection2(IServiceCollection services) : ServiceCollectionWrapper(services)
{

}
public class EntityServiceBuilder1<TContext, TEntity>(IServiceCollection services) : EntityServiceCollection2(services), IEntityServiceBuilder<TEntity, int>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
}
public class EntityServiceBuilder2<TContext, TEntity, TKey>(IServiceCollection services) : EntityServiceCollection2(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
}
public class EntityServiceBuilder3<TContext, TEntity, TKey>(IServiceCollection services) : EntityServiceCollection2(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
}
public class EntityServiceBuilder5<TContext, TEntity, TKey>(IServiceCollection services) : EntityServiceCollection2(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
}