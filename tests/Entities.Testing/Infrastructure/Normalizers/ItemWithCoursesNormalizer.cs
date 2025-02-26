﻿using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Testing.Library.Contoso;

namespace Entities.Testing.Infrastructure.Normalizers;

public class ItemWithCoursesNormalizer(INormalizer? normalizer)
    : EntityNormalizerBase<IHasCourses>(normalizer)
{
    public override Task HandleNormalize(IHasCourses item)
    {
        item.NormalizedContent = $"{item.NormalizedContent} {string.Join(" ", (item.Courses ?? []).Select(c => DefaultPropertyNormalizer.Normalize(c.Title)))}";
        return Task.CompletedTask;
    }
}
