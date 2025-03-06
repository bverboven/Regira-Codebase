﻿using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonSortQueryBuilder(Func<IQueryable<Person>, PersonSortBy?, IQueryable<Person>> sortQuery) : ISortedQueryBuilder<Person, int, PersonSortBy>
{
    public IQueryable<Person> SortBy(IQueryable<Person> query, PersonSortBy? sortBy)
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
}
public class PersonIncludableQueryBuilder : IIncludableQueryBuilder<Person, int, PersonIncludes>
{
    public IQueryable<Person> AddIncludes(IQueryable<Person> query, PersonIncludes? includes = null)
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