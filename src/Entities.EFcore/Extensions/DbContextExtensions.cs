using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Extensions;

public static class DbContextExtensions
{
    #region Primers
    /// <summary>
    /// Applies <see cref="IEntityPrimer{T}"/> without saving to DB<br />
    /// Make sure <see cref="EntityPrimerContainer"/> is registered in <see cref="IServiceCollection"/>, by calling <see cref="EntityPrimerContainerExtensions.RegisterPrimerContainer{TContext}"/> extension method
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public static Task ApplyPrimers(this DbContext dbContext, Type? entityType = null)
    {
        var primerContainer = dbContext.GetService<EntityPrimerContainer>();
        return primerContainer.ApplyPrimers(entityType);
    }
    public static Task ApplyPrimers<T>(this DbContext dbContext)
        => dbContext.ApplyPrimers(typeof(T));
    #endregion


    #region ChildCollections
    public static void UpdateEntityChildCollection<TEntity, TChild>(this DbContext dbContext, TEntity original, TEntity modified, Func<TEntity, ICollection<TChild>?> childrenGetter, Action<TEntity, ICollection<TChild>> childrenSetter, Action<TChild, TChild>? processExtra = null)
        where TEntity : class, IEntity<int>
        where TChild : class, IEntity<int>
    {
        // ignore when no child collection is passed for either original OR modified entity
        if (childrenGetter(original) == null || childrenGetter(modified) == null)
        {
            return;
        }

        // processes modified & deleted children
        UpdateChildCollection(dbContext, childrenGetter(original)!, childrenGetter(modified)!, processExtra);
        // also includes newly added children
        childrenSetter(original, childrenGetter(modified)!);
    }
    public static void UpdateChildCollection<T>(this DbContext dbContext, ICollection<T> originalItems, ICollection<T> modifiedItems, Action<T, T>? processExtra = null)
        where T : class, IEntity<int>
        => UpdateChildCollection(dbContext, originalItems, modifiedItems, x => x.Id, processExtra);
    public static void UpdateChildCollection<T>(this DbContext dbContext, ICollection<T> originalItems, ICollection<T> modifiedItems, Func<T, object> idSelector, Action<T, T>? processExtra = null)
        where T : class, IEntity
    {
        foreach (var originalItem in originalItems)
        {
            var modifiedItem = modifiedItems.FirstOrDefault(p => idSelector(p).Equals(idSelector(originalItem)));
            if (modifiedItem == null)
            {
                dbContext.Entry(originalItem).State = EntityState.Deleted;
            }
            else
            {
                processExtra?.Invoke(originalItem, modifiedItem);
                dbContext.Entry(originalItem).State = EntityState.Detached;
                dbContext.Entry(modifiedItem).OriginalValues.SetValues(originalItem);
                dbContext.Update(modifiedItem);
            }
        }
    }

    #region PrepareUpdate
    public class UpdateEntityChildrenWrapper<TEntity, TChild>
        where TEntity : class, IEntity
        where TChild : class, IEntity
    {
        public DbContext DbContext { get; set; } = null!;
        public TEntity Original { get; set; } = null!;
        public TEntity Modified { get; set; } = null!;
        public Func<TEntity, ICollection<TChild>> ChildrenGetter { get; set; } = null!;
        public Action<TEntity, ICollection<TChild>> ChildrenSetter { get; set; } = null!;
        public Func<TChild, object> IdSelector { get; set; } = null!;
    }
    /// <summary>
    /// Prepares a child collection update.<br />
    /// Call <see cref="Submit{TEntity,TChild}"/> to execute prepared actions
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="original"></param>
    /// <param name="modified"></param>
    /// <param name="childrenGetter"></param>
    /// <param name="childrenSetter"></param>
    /// <returns></returns>
    public static UpdateEntityChildrenWrapper<TEntity, TChild> PrepareEntityChildrenUpdate<TEntity, TChild>(this DbContext dbContext, TEntity original, TEntity modified, Func<TEntity, ICollection<TChild>> childrenGetter, Action<TEntity, ICollection<TChild>> childrenSetter)
        where TEntity : class, IEntity<int>
        where TChild : class, IEntity<int>
    {
        return new UpdateEntityChildrenWrapper<TEntity, TChild>
        {
            DbContext = dbContext,
            Original = original,
            Modified = modified,
            ChildrenGetter = childrenGetter,
            ChildrenSetter = childrenSetter,
            IdSelector = x => x.Id
        };
    }
    /// <summary>
    /// Executes prepared actions to update an entity's child collection
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="updater"></param>
    /// <param name="processExtra"></param>
    public static void Submit<TEntity, TChild>(this UpdateEntityChildrenWrapper<TEntity, TChild> updater, Action<TChild, TChild>? processExtra = null)
        where TEntity : class, IEntity<int>
        where TChild : class, IEntity<int>
        => UpdateEntityChildCollection(updater.DbContext, updater.Original, updater.Modified, updater.ChildrenGetter, updater.ChildrenSetter, processExtra);
    #endregion

    #endregion
}