#if NETCOREAPP3_1_OR_GREATER
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.EFcore.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Services;

public class EntityPrimerContainerInterceptor(IEnumerable<IEntityPrimer> primers) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var groupedEntries = eventData.Context
                .GetPendingEntries()
                .GroupBy(e => e.Entity.GetType());
            foreach (var entriesGroup in groupedEntries)
            {
                var genericPrimerTypes = new[] { entriesGroup.Key }.Concat(TypeUtility.GetBaseTypes(entriesGroup.Key)).Distinct();
                foreach (var genericPrimerType in genericPrimerTypes)
                {
                    var primerType = typeof(IEntityPrimer<>).MakeGenericType(genericPrimerType);
                    var matchingPrimers = primers.Where(x => TypeUtility.ImplementsInterface(x.GetType(), primerType));
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
    public static DbContextOptionsBuilder AddPrimerInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceCollection services)
    {
        var serviceProvider = optionsBuilder.Options
                                  .Extensions.OfType<CoreOptionsExtension>()
                                  .FirstOrDefault()
                                  ?.ApplicationServiceProvider
                              ?? services.BuildServiceProvider();
        var primerDescriptors = services.CollectDescriptors<IEntityPrimer>();
        var primerImplementations = primerDescriptors
            .Select(p => (IEntityPrimer)serviceProvider.GetRequiredService(p.ServiceType));
        var primerContainer = new EntityPrimerContainerInterceptor(primerImplementations);
        return optionsBuilder.AddInterceptors(primerContainer);
    }
    public static DbContextOptionsBuilder AddPrimerInterceptors(this DbContextOptionsBuilder optionsBuilder, IEnumerable<IEntityPrimer> primers)
        => optionsBuilder.AddInterceptors(new EntityPrimerContainerInterceptor(primers));
    public static DbContextOptionsBuilder AddPrimerInterceptors(this DbContextOptionsBuilder optionsBuilder, Func<IEnumerable<IEntityPrimer>> createPrimers)
        => AddPrimerInterceptors(optionsBuilder, createPrimers());
}
#endif