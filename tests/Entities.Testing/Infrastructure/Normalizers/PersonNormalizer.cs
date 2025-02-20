﻿using Regira.Normalizing;
using Testing.Library.Contoso;

namespace Entities.Testing.Infrastructure.Normalizers;
public class PersonNormalizer : ObjectNormalizer<Person>
{
    public override async Task HandleNormalizeMany(IEnumerable<Person?> items, bool recursive = false)
    {
        await base.HandleNormalizeMany(items, false);
    }
}