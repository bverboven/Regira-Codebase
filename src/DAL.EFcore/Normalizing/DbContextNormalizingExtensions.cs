using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Regira.DAL.EFcore.Extensions;
using Regira.Normalizing;
using Regira.Normalizing.Models;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Normalizing;

public static class DbContextNormalizingExtensions
{
    public static void AutoNormalizeStringsForEntries(this DbContext dbContext, NormalizingOptions? options = null)
    {
        foreach (var entry in dbContext.GetPendingEntries())
        {
            if (entry.State != EntityState.Deleted)
            {
                NormalizingUtility.InvokeObjectNormalizer(entry.Entity, options);
            }
        }
    }

    /// <summary>
    /// Normalizes all properties with a <see cref="NormalizedAttribute"/> for <see cref="EntityEntry">Entries</see> that have pending changes
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="entityType"></param>
    public static void ApplyNormalizers(this DbContext dbContext, Type? entityType = null)
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
    public static void ApplyNormalizers(this DbContext dbContext, IEnumerable<object> items)
    {
        var normalizingSelector = dbContext.GetService<ObjectNormalizerContainer>();

        var itemsByType = items.GroupBy(item => item.GetType()).ToArray();
        foreach (var typedItems in itemsByType)
        {
            var normalizer = normalizingSelector.Find(typedItems.Key);
            normalizer?.HandleNormalizeMany(typedItems);
        }
    }
}