#if NETCOREAPP3_1_OR_GREATER

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Regira.Entities.EFcore.Primers;

public class EntityPrimerContainerInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
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
                foreach (var entriesGroup in groupedEntries)
                {
                    var matchingPrimers = primers.FindMatchingServices(entriesGroup.Key);

                    foreach (var primer in matchingPrimers)
                    {
                        await primer.PrepareManyAsync(entriesGroup.ToArray());
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
