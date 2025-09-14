using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Regira.Entities.Mapping.Mapster;

public class EntityAdapterConfigBuilder(TypeAdapterConfig config, IServiceCollection services)
{
    protected internal TypeAdapterConfig Config => config;
    protected internal IServiceCollection Services => services;
    protected internal List<Action<IServiceProvider>> MappingActions = [];

    public EntityAdapterConfigBuilder Map<TEntity, TEntityDto, TEntityInputDto>(Action<EntityAdapterConfig>? configure = null)
    {
        MappingActions.Add(_ =>
        {
            config.NewConfig<TEntity, TEntityDto>();
            config.NewConfig<TEntityInputDto, TEntity>();
        });

        configure?.Invoke(new EntityAdapterConfig(this));

        return this;
    }

    protected internal void Build(IServiceProvider serviceProvider)
    {
        foreach (var action in MappingActions)
        {
            action.Invoke(serviceProvider);
        }
    }
}