﻿using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Testing.Library.Contoso;

namespace Entities.Testing.Infrastructure.Normalizers;

public class PersonNormalizer(INormalizer? normalizer) : EntityNormalizerBase<Person>(normalizer)
{
    public override Task HandleNormalizeMany(IEnumerable<Person> items)
    {
        foreach (var item in items)
        {
            item.NormalizedContent = $"PERSON {item.NormalizedContent}".Trim();
        }

        return Task.CompletedTask;
    }
}