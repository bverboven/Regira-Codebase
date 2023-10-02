using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Entities.Testing.Infrastructure.Primers;

public class NormalizingPrimer : EntityPrimerBase<IEntity>
{
    private readonly ObjectNormalizer _objNormalizer;
    public NormalizingPrimer()
    {
        _objNormalizer = new ObjectNormalizer();
    }

    public override Task PrepareAsync(IEntity entity, EntityEntry entry)
    {
        _objNormalizer.HandleNormalize(entity);

        return Task.CompletedTask;
    }
}