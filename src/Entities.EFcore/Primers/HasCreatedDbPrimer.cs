using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Primers;

public class HasCreatedDbPrimer : EntityPrimerBase<IHasCreated>
{
    public override Task PrepareAsync(IHasCreated entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Modified)
        {
            entity.Created = (DateTime)entry.OriginalValues[nameof(entity.Created)]!;
        }
        if (entity.Created == DateTime.MinValue)
        {
            entity.Created = DateTime.Now;
        }

        return Task.CompletedTask;
    }
}