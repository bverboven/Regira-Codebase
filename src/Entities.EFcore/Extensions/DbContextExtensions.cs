using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

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
    public static void UpdateRelatedCollection<TEntity, TRelated>(this DbContext dbContext, TEntity modified, TEntity original, Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression)
        where TEntity : class, IEntity<int>
        where TRelated : class, IEntity<int>
    => dbContext.UpdateRelatedCollection<TEntity, TRelated, int, int>(modified, original, navigationExpression);
    public static void UpdateRelatedCollection<TEntity, TRelated, TEntityKey, TRelatedKey>(this DbContext dbContext, TEntity modified, TEntity original, Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression)
        where TEntity : class, IEntity<TEntityKey>
        where TRelated : class, IEntity<TRelatedKey>
    {
        var selectorFunc = navigationExpression.Compile();
        var originalItems = selectorFunc(original);
        var modifiedItems = selectorFunc(modified);

        dbContext.UpdateRelatedCollection<TEntity, TRelated, TEntityKey, TRelatedKey>(modified, modifiedItems, original, originalItems);
    }
    public static void UpdateRelatedCollection<TEntity, TRelated, TEntityKey, TRelatedKey>(this DbContext dbContext, TEntity modified, ICollection<TRelated>? modifiedItems, TEntity original, ICollection<TRelated>? originalItems)
        where TEntity : class, IEntity<TEntityKey>
        where TRelated : class, IEntity<TRelatedKey>
    {
        if (modifiedItems == null || originalItems == null)
        {
            return;
        }
        
        var relatedItemsToAdd = modifiedItems.Where(m => m.Id == null || m.Id.Equals(default(TRelatedKey)) || originalItems.All(o => m.Id.Equals(o.Id) != true)).ToArray();
        var relatedItemsToDelete = originalItems.Where(o => modifiedItems.All(m => m.Id != null && m.Id.Equals(o.Id) != true)).ToArray();
        foreach (var entity in relatedItemsToAdd)
        {
            dbContext.Entry(entity).State = EntityState.Added;
        }
        foreach (var entity in relatedItemsToDelete)
        {
            dbContext.Entry(entity).State = EntityState.Deleted;
        }
        var relatedItemsToModify = modifiedItems.Except(relatedItemsToAdd).Where(m => m.Id != null && !m.Id.Equals(default(TRelatedKey)));
        foreach (var entity in relatedItemsToModify)
        {
            var originalEntity = originalItems.Single(p => p.Id!.Equals(entity.Id));
            dbContext.Entry(originalEntity).State = EntityState.Detached;
            dbContext.Attach(entity);
            dbContext.Entry(entity).OriginalValues.SetValues(originalEntity);
            dbContext.Update(entity);
        }
    }
    #endregion
}