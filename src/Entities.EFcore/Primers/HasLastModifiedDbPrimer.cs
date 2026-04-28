using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Primers;

/// <summary>
/// Sets <see cref="IHasLastModified.LastModified"/> to current UTC time when entity is modified<br />
/// </summary>
public class HasLastModifiedDbPrimer : EntityPrimerBase<IHasLastModified>
{
    public override Task PrepareAsync(IHasLastModified entity, EntityEntry entry, CancellationToken token = default)
    {
        if (entry.State == EntityState.Modified)
        {
            entity.LastModified = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}