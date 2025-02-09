#if NETCOREAPP3_1_OR_GREATER

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Regira.DAL.EFcore.Extensions;

namespace Regira.DAL.EFcore.Services;
public class AutoTruncateDbContextInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Truncates all string properties with a <see cref="MaxLengthAttribute"/> for <see cref="EntityEntry">Entries</see> that have pending changes<br />
    /// Credits: https://gist.github.com/abrari/dfe772db172f950e9f0d8acdd3982fbb
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="result"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.GetPendingEntries())
            {
                if (entry.State != EntityState.Deleted)
                {
                    entry.AutoTruncate();
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public static class DbContextInterceptorExtensions
{
    public static DbContextOptionsBuilder<TContext> AddAutoTruncateInterceptors<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder)
        where TContext : DbContext
        => optionsBuilder.AddInterceptors(new AutoTruncateDbContextInterceptor());
}

#endif