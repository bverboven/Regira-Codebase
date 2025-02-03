using Regira.Entities.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonManager(IEntityRepository<Person, int, PersonSearchObject, PersonSortBy, PersonIncludes> repo)
    : EntityManagerBase<Person, PersonSearchObject, PersonSortBy, PersonIncludes>(repo);