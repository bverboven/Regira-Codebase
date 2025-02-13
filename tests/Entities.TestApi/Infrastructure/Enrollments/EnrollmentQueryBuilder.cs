using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Enrollments;

public class EnrollmentQueryBuilder(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<Enrollment, SearchObject<int>>>? filters = null,
    ILoggerFactory? loggerFactory = null) : QueryBuilder<Enrollment>(globalFilters, filters, loggerFactory)
{
    public override IQueryable<Enrollment> AddIncludes(IQueryable<Enrollment> query, IList<SearchObject<int>?>? so, IList<EntitySortBy>? sortByList, EntityIncludes? includes)
    {
        return query
            .Include(x => x.Course)
            .Include(x => x.Student);
    }
}