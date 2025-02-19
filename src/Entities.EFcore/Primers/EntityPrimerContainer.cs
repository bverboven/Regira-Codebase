using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Primers;

public class EntityPrimerContainer
{
    private readonly DbContext _dbContext;
    private readonly IServiceCollection? _services;
    private readonly ICollection<IEntityPrimer>? _primers;
    private ICollection<IEntityPrimer> Primers => _primers ?? GetPrimers(_services!);

    public EntityPrimerContainer(DbContext dbContext, IEnumerable<IEntityPrimer> primers)
    {
        _dbContext = dbContext;
        _primers = primers.ToArray();
    }
    public EntityPrimerContainer(DbContext dbContext, IServiceCollection services)
    {
        _dbContext = dbContext;
        _services = services;
    }

    public IEntityPrimer[] GetPrimers(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        return services
            .Where(s => TypeUtility.ImplementsInterface<IEntityPrimer>(s.ServiceType))
            .Select(x => (IEntityPrimer)serviceProvider.GetService(x.ServiceType)!)
            .ToArray();
    }

    public async Task ApplyPrimers(Type? entityType = null)
    {
        var groupedEntries = _dbContext.GetPendingEntries()
            .GroupBy(e => e.Entity.GetType())
            .Where(g => entityType == null || g.Key == entityType || TypeUtility.GetBaseTypes(g.Key).Contains(entityType));
        foreach (var entriesGroup in groupedEntries)
        {
            var genericPrimerTypes = new[] { entriesGroup.Key }.Concat(TypeUtility.GetBaseTypes(entriesGroup.Key)).Distinct();
            foreach (var genericPrimerType in genericPrimerTypes)
            {
                var primerType = typeof(IEntityPrimer<>).MakeGenericType(genericPrimerType);
                var primers = Primers.Where(x => TypeUtility.ImplementsInterface(x.GetType(), primerType));
                foreach (var primer in primers)
                {
                    await primer.PrepareManyAsync(entriesGroup.ToArray());
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Type of entity</typeparam>
    /// <returns></returns>
    public Task ApplyPrimers<T>()
        => ApplyPrimers(typeof(T));
}

public static class EntityPrimerContainerExtensions
{
    public static IServiceCollection RegisterPrimerContainer<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddTransient(p => new EntityPrimerContainer(p.GetRequiredService<TContext>(), services));
}