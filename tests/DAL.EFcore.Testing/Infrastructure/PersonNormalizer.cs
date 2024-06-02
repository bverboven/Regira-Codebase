using Testing.Library.Contoso;

namespace DAL.EFcore.Testing.Infrastructure;
public class PersonNormalizer : EntityNormalizerBase<Person>
{
    public override async Task HandleNormalize(IEnumerable<Person> items)
    {
        await base.HandleNormalizeMany(items);
    }
}