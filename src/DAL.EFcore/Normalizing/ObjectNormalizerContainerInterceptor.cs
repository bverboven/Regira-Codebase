#if NETCOREAPP3_1_OR_GREATER

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Normalizing.Abstractions;

namespace Regira.DAL.EFcore.Normalizing;

public class ObjectNormalizerContainerInterceptor(IEnumerable<IObjectNormalizer> normalizers) : SaveChangesInterceptor
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
                    var entities = entriesGroup.Select(e => e.Entity).ToArray();
                    var exclusiveNormalizer = normalizers.FirstOrDefault(x => x.IsExclusive);
                    if (exclusiveNormalizer != null)
                    {
                        await exclusiveNormalizer.HandleNormalizeMany(entities);
                    }
                    else
                    {
                        foreach (var normalizer in normalizers)
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
public static class ObjectNormalizerContainerInterceptorExtensions
{
    /// <summary>
    /// Will find all services that implements <see cref="IObjectNormalizer"/> and execute them when calling SaveChanges on DbContext
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
        var normalizers = services.CollectDescriptors<IObjectNormalizer>()
            .Select(d => (IObjectNormalizer)serviceProvider.GetRequiredService(d.ServiceType));
        var normalizerContainer = new ObjectNormalizerContainerInterceptor(normalizers);
        return optionsBuilder.AddInterceptors(normalizerContainer);
    }
}

#endif