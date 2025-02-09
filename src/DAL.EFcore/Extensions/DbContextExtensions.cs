using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.DAL.EFcore.Extensions;

public static class DbContextExtension
{
    /// <summary>
    /// Returns entity entries that have pending changes<br />
    /// <list type="bullet">
    ///     <item><see cref="EntityState.Added"/></item>
    ///     <item><see cref="EntityState.Modified"/></item>
    ///     <item><see cref="EntityState.Deleted"/></item>
    /// </list>
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static IEnumerable<EntityEntry> GetPendingEntries(this DbContext dbContext)
        => dbContext.ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Modified or EntityState.Added or EntityState.Deleted);

    /// <summary>
    /// Extends native SaveChangesAsync, by removing modifications on entries that caused errors, to enable recalling SaveChangesAsync for other entries<br />
    /// Credits: http://www.binaryintellect.net/articles/c1bff938-1789-4501-8161-3f38bc465a8b.aspx
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> SaveAndCleanUpOnError<TContext>(this TContext dbContext,
        bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        try
        {
            return await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            foreach (var entry in ex.Entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }

            throw;
        }
    }
}