using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Regira.Entities.DependencyInjection.Extensions;

public class EntityServiceCollectionOptions(IServiceCollection services)
{
    internal IServiceCollection Services => services;
    public ICollection<Assembly> ProfileAssemblies { get; set; } = new List<Assembly>();
}