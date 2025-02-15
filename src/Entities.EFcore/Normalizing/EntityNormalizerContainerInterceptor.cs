#if NETCOREAPP3_1_OR_GREATER

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Normalizing;

public class EntityNormalizerContainerInterceptor(IEnumerable<IEntityNormalizer> normalizers) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var groupedEntries = eventData.Context
                .GetPendingEntries()
                .GroupBy(e => e.Entity.GetType())
                .ToArray();
            if (normalizers.Any() && groupedEntries.Any())
            {
                foreach (var entriesGroup in groupedEntries)
                {
                    var genericEntityTypes = new[] { entriesGroup.Key }
                        .Concat(TypeUtility.GetBaseTypes(entriesGroup.Key))
                        .Distinct();
                    var matchingNormalizers = normalizers
                        .Where(x => genericEntityTypes.Any(entityType =>
                        {
                            var normalizerType = typeof(IEntityNormalizer<>).MakeGenericType(entityType);
                            return TypeUtility.ImplementsInterface(x.GetType(), normalizerType);
                        }))
                        .ToArray();

                    var entities = entriesGroup.Select(e => e.Entity).ToArray();

                    var exclusiveNormalizer = matchingNormalizers.FirstOrDefault(x => x.IsExclusive);
                    if (exclusiveNormalizer != null)
                    {
                        await exclusiveNormalizer.HandleNormalizeMany(entities);
                    }
                    else
                    {
                        foreach (var normalizer in matchingNormalizers)
                        {
                            await normalizer.HandleNormalizeMany(entities);
                        }
                    }
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
public static class EntityNormalizerContainerInterceptorExtensions
{
    /// <summary>
    /// Will find all services that implements <see cref="IEntityNormalizer"/> and execute them when calling SaveChanges on DbContext
    /// </summary>
    /// <param name="optionsBuilder"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder AddNormalizerInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceCollection services)
    {
        var serviceProvider = optionsBuilder.Options
                                 .Extensions.OfType<CoreOptionsExtension>()
                                 .FirstOrDefault()
                                 ?.ApplicationServiceProvider
                             ?? services.BuildServiceProvider();

        var descriptors = services.CollectDescriptors<IEntityNormalizer>();
        var normalizers = descriptors
            .DistinctBy(x => x.ServiceType)
            .SelectMany(d =>
            {
                return serviceProvider.GetServices(d.ServiceType).OfType<IEntityNormalizer>();
            })
            .DistinctBy(x => x.GetType())
            .ToArray();

        var normalizerContainer = new EntityNormalizerContainerInterceptor(normalizers);
        return optionsBuilder.AddInterceptors(normalizerContainer);
    }
}

#endif