using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Abstractions;

public interface IEntityServiceBuilder<TEntity> : IEntityServiceBuilder<TEntity, int>
    where TEntity : class, IEntity<int>;
// ReSharper disable once UnusedTypeParameter
public interface IEntityServiceBuilder<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    IServiceCollection Services { get; }
}
// ReSharper disable once UnusedTypeParameter
public interface IEntityServiceBuilder<TEntity, TKey, TSearchObject> : IEntityServiceBuilder<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();