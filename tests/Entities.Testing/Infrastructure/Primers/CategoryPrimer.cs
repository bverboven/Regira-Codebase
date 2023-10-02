using Entities.Testing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Entities.Testing.Infrastructure.Primers;

public class CategoryPrimer : EntityPrimerBase<Category>
{
    private readonly ProductContext _dbContext;
    private readonly IEntityPrimer<IHasTimestamps> _timestampPrimer;
    private readonly NormalizingPrimer _normalizingPrimer;
    private int _maxSortOrder;
    public CategoryPrimer(ProductContext dbContext, IEntityPrimer<IHasTimestamps> timestampPrimer, NormalizingPrimer normalizingPrimer)
    {
        _dbContext = dbContext;
        _timestampPrimer = timestampPrimer;
        _normalizingPrimer = normalizingPrimer;
    }


    public override async Task PrepareManyAsync(IList<EntityEntry> entries)
    {
        _maxSortOrder = await _dbContext.Categories.MaxAsync(x => (int?)x.SortOrder) ?? -1;
        await base.PrepareManyAsync(entries);
    }
    public override async Task PrepareAsync(Category entity, EntityEntry entry)
    {
        await _timestampPrimer.PrepareAsync(entity, entry);
        await _normalizingPrimer.PrepareAsync(entity, entry);
        if (entry.State == EntityState.Added)
        {
            entity.SortOrder = ++_maxSortOrder;
        }
    }
}