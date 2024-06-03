using Regira.Normalizing;
using Testing.Library.Contoso;

namespace Entities.Testing.Infrastructure.Normalizers;

public class ItemWithCoursesNormalizer : ObjectNormalizer<IHasCourses>
{
    public override async Task HandleNormalizeMany(IEnumerable<IHasCourses?> items, bool recursive = false)
    {
        await base.HandleNormalizeMany(items, recursive);

        foreach (var item in items)
        {
            if (item != null)
            {
                item.NormalizedContent = $"{item.NormalizedContent} {string.Join(" ", (item.Courses ?? []).Select(c => DefaultNormalizer.Normalize(c.Title)))}";
            }
        }
    }
}
