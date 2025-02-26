using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Primers;

public class HasLastModifiedDbPrimer : EntityPrimerBase<IHasLastModified>
{
    public override Task PrepareAsync(IHasLastModified entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Modified)
        {
            entity.LastModified = DateTime.Now;
        }

        return Task.CompletedTask;
    }
}