using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Primers;

/// <summary>
/// Sets <see cref="IHasCreated.Created"/> to current UTC time when entity is added, or keeps original value when entity is modified<br />
/// </summary>
public class HasCreatedDbPrimer : EntityPrimerBase<IHasCreated>
{
    public override Task PrepareAsync(IHasCreated entity, EntityEntry entry, CancellationToken token = default)
    {
        entity.Created = (DateTime)entry.OriginalValues[nameof(entity.Created)]!;

        if (entity.Created == DateTime.MinValue)
        {
            entity.Created = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }
}