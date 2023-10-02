using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Entities.Testing.Infrastructure.Primers;

public class TimestampPrimer : EntityPrimerBase<IHasTimestamps>
{
    public override Task PrepareAsync(IHasTimestamps entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Modified)
        {
            entity.Created = (DateTime)entry.OriginalValues[nameof(entity.Created)]!;
            entity.LastModified = DateTime.Now;
        }
        else if (entity.Created == DateTime.MinValue)
        {
            entity.Created = DateTime.Now;
        }

        return Task.CompletedTask;
    }
}