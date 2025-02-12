using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonQueryBuilder(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<Person, PersonSearchObject>> filters,
    ILoggerFactory loggerFactory)
    : QueryBuilder<Person, PersonSearchObject, PersonSortBy, PersonIncludes>(globalFilters, filters, loggerFactory)
{
    public override IQueryable<Person> SortBy(IQueryable<Person> query, IList<PersonSearchObject?>? so, PersonSortBy? sortBy, PersonIncludes? includes)
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
    public override IQueryable<Person> AddIncludes(IQueryable<Person> query, IList<PersonSearchObject?>? so, IList<PersonSortBy>? sortByList, PersonIncludes? includes)
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
}