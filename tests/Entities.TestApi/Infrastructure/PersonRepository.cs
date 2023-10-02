using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Keywords;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.TestApi.Infrastructure;

public enum PersonSortBy
{
    Default,
    Id,
    IdDesc,
    GivenName,
    LastName,
    Title,
}
[Flags]
public enum PersonIncludes
{
    None,
    Supervisor = 1 << 0,
    Subordinates = 1 << 1,
    Departments = 1 << 2,
    All = Supervisor | Subordinates | Departments
}
public class PersonSearchObject : SearchObject
{
    public string? GivenName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}

public class PersonRepository : EntityRepositoryBase<ContosoContext, Person, PersonSearchObject, PersonSortBy, PersonIncludes>
{
    public PersonRepository(ContosoContext dbContext)
        : base(dbContext)
    {
    }

    public override IQueryable<Person> Filter(IQueryable<Person> query, PersonSearchObject? so)
    {
        query = base.Filter(query, so);

        if (so == null)
        {
            return query;
        }

        var qh = QKeywordHelper.Create();

        if (!string.IsNullOrWhiteSpace(so.LastName))
        {
            var name = qh.ParseKeyword(so.LastName);
            query = name.HasWildcard
                ? query.Where(x => EF.Functions.Like(x.LastName!, name.Q!))
                : query.Where(x => x.LastName == name.Keyword);
        }
        if (!string.IsNullOrWhiteSpace(so.GivenName))
        {
            var name = qh.ParseKeyword(so.GivenName);
            query = name.HasWildcard
                ? query.Where(x => EF.Functions.Like(x.GivenName!, name.Q!))
                : query.Where(x => x.GivenName == name.Keyword);
        }

        if (!string.IsNullOrWhiteSpace(so.Phone))
        {
            var phone = qh.ParseKeyword(so.Phone);
            query = phone.HasWildcard
                ? query.Where(x => EF.Functions.Like(x.Phone!, phone.Q!))
                : query.Where(x => x.Phone == phone.Keyword);
        }

        return query;
    }
    public override IQueryable<Person> SortBy(IQueryable<Person> query, PersonSortBy? sortBy = null)
    {
        if (sortBy != null)
        {
            // ThenBy
            if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<Person> sortedQuery)
            {
                if (sortBy == PersonSortBy.Id)
                {
                    return sortedQuery.ThenBy(x => x.Id);
                }

                if (sortBy == PersonSortBy.IdDesc)
                {
                    return sortedQuery.ThenByDescending(x => x.Id);
                }

                if (sortBy == PersonSortBy.GivenName)
                {
                    return sortedQuery.ThenBy(x => x.GivenName);
                }

                if (sortBy == PersonSortBy.LastName)
                {
                    return sortedQuery.ThenBy(x => x.LastName);
                }

                if (sortBy == PersonSortBy.Title)
                {
                    return sortedQuery.ThenBy(x => x.Title);
                }
            }

            // OrderBy
            if (sortBy == PersonSortBy.Id)
            {
                return query.OrderBy(x => x.Id);
            }

            if (sortBy == PersonSortBy.IdDesc)
            {
                return query.OrderByDescending(x => x.Id);
            }

            if (sortBy == PersonSortBy.GivenName)
            {
                return query.OrderBy(x => x.GivenName);
            }

            if (sortBy == PersonSortBy.LastName)
            {
                return query.OrderBy(x => x.LastName);
            }

            if (sortBy == PersonSortBy.Title)
            {
                return query.OrderBy(x => x.Title);
            }
        }

        return query.OrderBy(x => x.LastName);
    }
    public override IQueryable<Person> AddIncludes(IQueryable<Person> query, PersonIncludes? includes)
    {
        if (includes.HasValue)
        {
            if (includes.Value.HasFlag(PersonIncludes.Supervisor))
            {
                query = query.Include(x => x.Supervisor);
            }
            if (includes.Value.HasFlag(PersonIncludes.Subordinates))
            {
                query = query.Include(x => x.Subordinates);
            }
            if (includes.Value.HasFlag(PersonIncludes.Departments))
            {
                query = query.Include(x => x.Departments);
            }
        }

        return query;
    }

    public override void Modify(Person item, Person original)
    {
        base.Modify(item, original);

        DbContext.UpdateEntityChildCollection(original, item, x => x.Departments, (person, departments) => person.Departments = departments);
    }
}

public class PersonManager : EntityManagerBase<Person, PersonSearchObject, PersonSortBy, PersonIncludes>
{
    public PersonManager(IEntityRepository<Person, int, PersonSearchObject, PersonSortBy, PersonIncludes> repo)
        : base(repo)
    {
    }
}