using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;

namespace Entities.Testing.Infrastructure.Primers;

public class NormalizingPrimer<TEntity>(IObjectNormalizer<TEntity> objectNormalizer) : EntityPrimerBase<TEntity> 
    where TEntity : class
{
    public override Task PrepareAsync(TEntity entity, EntityEntry entry)
    {
        objectNormalizer.HandleNormalize(entity);

        return Task.CompletedTask;
    }
}
public class NormalizingPrimer(IObjectNormalizer? objectNormalizer = null) : EntityPrimerBase<IEntity>
{
    private readonly IObjectNormalizer _objectNormalizer = objectNormalizer ?? new ObjectNormalizer();

    public override Task PrepareAsync(IEntity entity, EntityEntry entry)
    {
        _objectNormalizer.HandleNormalize(entity);

        return Task.CompletedTask;
    }
}