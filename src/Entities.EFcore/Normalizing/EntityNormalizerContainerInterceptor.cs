#if NETCOREAPP3_1_OR_GREATER

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Normalizing.Abstractions;

namespace Regira.Entities.EFcore.Normalizing;

public class EntityNormalizerContainerInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var normalizers = serviceProvider.GetServices<IEntityNormalizer>()
                .Distinct()
                .ToArray();

            var groupedEntries = eventData.Context
                .GetPendingEntries()
                .GroupBy(e => e.Entity.GetType())
                .ToArray();

            if (normalizers.Any() && groupedEntries.Any())
            {
                foreach (var entriesGroup in groupedEntries)
                {
                    var matchingNormalizers = normalizers.FindMatchingServices(entriesGroup.Key);

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
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder AddNormalizerInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider? serviceProvider = null)
    {
        serviceProvider ??= optionsBuilder.Options
                                 .Extensions.OfType<CoreOptionsExtension>()
                                 .FirstOrDefault()
                                 ?.ApplicationServiceProvider
            ?? throw new NotImplementedException("Could not create a ServiceProvider instance");

        var normalizerContainer = new EntityNormalizerContainerInterceptor(serviceProvider);
        return optionsBuilder.AddInterceptors(normalizerContainer);
    }
}

#endif