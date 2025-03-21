using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Entities.Testing.Infrastructure.Primers;

public class TimestampPrimer : EntityPrimerBase<IHasTimestamps>
{
    public override Task PrepareAsync(IHasTimestamps entity, EntityEntry entry)
    {
        entity.Created = (DateTime)entry.OriginalValues[nameof(entity.Created)]!;

        if (entity.Created == DateTime.MinValue)
        {
            entity.Created = DateTime.Now;
        }

        if (entry.State == EntityState.Modified)
        {
            entity.LastModified = DateTime.Now;
        }

        return Task.CompletedTask;
    }
}