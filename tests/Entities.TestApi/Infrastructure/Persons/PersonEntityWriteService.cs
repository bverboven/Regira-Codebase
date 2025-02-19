using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Services;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonEntityWriteService(ContosoContext dbContext, IEntityReadService<Person> readService)
    : EntityWriteService<ContosoContext, Person>(dbContext, readService)
{
    public override async Task<Person?> Modify(Person item)
    {
        var original = await base.Modify(item);

        DbContext.UpdateEntityChildCollection(original!, item, x => x.Departments, (person, departments) => person.Departments = departments);

        return original;
    }
}