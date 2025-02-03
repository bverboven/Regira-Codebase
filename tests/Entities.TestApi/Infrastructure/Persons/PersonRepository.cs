using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonRepository(ContosoContext dbContext, IQueryBuilder<Person, PersonSearchObject, PersonSortBy, PersonIncludes> queryBuilder)
    : EntityRepositoryBase<ContosoContext, Person, PersonSearchObject, PersonSortBy, PersonIncludes>(dbContext, queryBuilder)
{
    private readonly ContosoContext _dbContext = dbContext;

    public override void Modify(Person item, Person original)
    {
        base.Modify(item, original);

        _dbContext.UpdateEntityChildCollection(original, item, x => x.Departments, (person, departments) => person.Departments = departments);
    }
}