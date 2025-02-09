using Microsoft.EntityFrameworkCore;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.Testing.Infrastructure.Normalizers;

public class DepartmentNormalizer(ContosoContext dbContext, IObjectNormalizer<IHasCourses> coursesNormalizer) : ObjectNormalizer<Department>
{
    public override bool IsExclusive => true;
    public override async Task HandleNormalizeMany(IEnumerable<Department?> items, bool recursive = false)
    {
        var itemsList = items.ToArray();
        await base.HandleNormalizeMany(itemsList, recursive);
        await coursesNormalizer.HandleNormalizeMany(itemsList);

        var adminIds = itemsList.Select(x => x?.AdministratorId).Where(id => id.HasValue).Distinct().ToArray();
        var admins = await dbContext.Persons.Where(x => adminIds.Contains(x.Id)).ToListAsync();
        foreach (var item in itemsList)
        {

            var admin = admins.FirstOrDefault(x => x.Id == item?.AdministratorId);
            if (admin != null)
            {
                item!.NormalizedContent = $"{item.NormalizedContent} {admin.NormalizedTitle}";
            }
        }
    }
}
