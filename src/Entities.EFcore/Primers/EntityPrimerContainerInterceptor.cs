#if NETCOREAPP3_1_OR_GREATER

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Regira.Entities.EFcore.Primers;

public class EntityPrimerContainerInterceptor(IServiceProvider serviceProvider, ILogger<EntityPrimerContainerInterceptor>? logger = null) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var primers = serviceProvider.GetServices<IEntityPrimer>()
                .Distinct()
                .ToArray();

            var groupedEntries = eventData.Context
                .GetPendingEntries()
                .GroupBy(e => e.Entity.GetType())
                .ToArray();

            if (primers.Any() && groupedEntries.Any())
            {
                // execute primers in same order than they were registered
                foreach (var primer in primers)
                {
                    foreach (var entriesGroup in groupedEntries)
                    {
                        if (primer.IsMatch(entriesGroup.Key))
                        {
                            logger?.LogDebug($"Priming {entriesGroup.Count()} {entriesGroup.Key.FullName} entries using {primer.GetType().FullName}");
                            await primer.PrepareManyAsync(entriesGroup.ToArray());
                        }
                    }
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public static class EntityPrimerContainerInterceptorExtensions
{
    public static DbContextOptionsBuilder AddPrimerInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider? serviceProvider = null)
    {
        serviceProvider ??= optionsBuilder.Options
                                .Extensions.OfType<CoreOptionsExtension>()
                                .FirstOrDefault()
                                ?.ApplicationServiceProvider
                            ?? throw new NotImplementedException("Could not create a ServiceProvider instance");

        var primerContainer = new EntityPrimerContainerInterceptor(serviceProvider);
        return optionsBuilder.AddInterceptors(primerContainer);
    }
}
#endif
