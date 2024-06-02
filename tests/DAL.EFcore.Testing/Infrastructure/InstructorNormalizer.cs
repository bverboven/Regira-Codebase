using Testing.Library.Contoso;

namespace DAL.EFcore.Testing.Infrastructure;

public class InstructorNormalizer : EntityNormalizerBase<Instructor>
{
    public override async Task HandleNormalize(IEnumerable<Instructor> items)
    {
        await base.HandleNormalizeMany(items);

        foreach (var item in items)
        {
            item.NormalizedContent = $"{item.NormalizedContent} {string.Join(" ", (item.Courses ?? []).Select(c => DefaultNormalizer.Normalize(c.Title)))}";
        }
    }
}
