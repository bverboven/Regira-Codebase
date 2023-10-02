using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Regira.Normalizing.Models;

namespace Entities.Testing.Infrastructure.Primers;

public class TitlePrimer : EntityPrimerBase<IHasNormalizedTitle>
{
    private readonly DefaultNormalizer _normalizer;
    public TitlePrimer()
    {
        _normalizer = new DefaultNormalizer(new NormalizeOptions { RemoveDiacritics = true, Transform = TextTransform.ToUpperCase });
    }

    public override Task PrepareAsync(IHasNormalizedTitle entity, EntityEntry entry)
    {
        entity.NormalizedTitle = _normalizer.Normalize(entity.Title);

        return Task.CompletedTask;
    }
}