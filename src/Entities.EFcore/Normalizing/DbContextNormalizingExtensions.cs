using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Regira.DAL.EFcore.Extensions;
using Regira.Normalizing;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Normalizing;

public static class DbContextNormalizingExtensions
{
    /// <summary>
    /// Normalizes all properties with a <see cref="NormalizedAttribute"/> for <see cref="EntityEntry">Entries</see> that have pending changes
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="entityType"></param>
    public static Task ApplyNormalizers(this DbContext dbContext, Type? entityType = null)
        => ApplyNormalizers(dbContext, dbContext.GetPendingEntries()
            .Where(e => e.State != EntityState.Deleted)
            .Where(x => entityType == null || x.Entity.GetType() == entityType || TypeUtility.GetBaseTypes(x.Entity.GetType()).Contains(entityType))
            .Select(x => x.Entity));
    /// <summary>
    /// <inheritdoc cref="ApplyNormalizers(DbContext, Type?)"/>
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="dbContext"></param>
    public static void ApplyNormalizers<T>(this DbContext dbContext)
        => ApplyNormalizers(dbContext, typeof(T));
    /// <summary>
    /// Normalizes all properties with a <see cref="NormalizedAttribute"/> for the given items
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="items"></param>
    public static async Task ApplyNormalizers(this DbContext dbContext, IEnumerable<object> items)
    {
        var normalizingContainer = dbContext.GetService<EntityNormalizerContainer>();

        var itemsByType = items.GroupBy(item => item.GetType()).ToArray();
        foreach (var typedItems in itemsByType)
        {
            var normalizers = normalizingContainer.FindAll(typedItems.Key).ToArray();
            var exclusiveNormalizer = normalizers.FirstOrDefault(x => x.IsExclusive);
            if (exclusiveNormalizer != null)
            {
                await exclusiveNormalizer.HandleNormalizeMany(typedItems);
            }
            else
            {
                foreach (var normalizer in normalizers)
                {
                    await normalizer.HandleNormalizeMany(typedItems);
                }
            }
        }
    }
}