using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Testing.Library.Contoso;

namespace Entities.DependencyInjection.Testing.Infrastructure;

public class CourseQueryFilter1 : FilteredQueryBuilderBase<Course, int, SearchObject<int>>
{
    public override IQueryable<Course> Build(IQueryable<Course> query, SearchObject<int>? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            query = query.Where(x => x.Title!.Contains(so.Q));
        }

        return query;
    }
}

public class CourseQueryFilter3 : FilteredQueryBuilderBase<Course, int, CourseSearchObject>
{
    public override IQueryable<Course> Build(IQueryable<Course> query, CourseSearchObject? so)
    {
        if (so?.DepartmentId is not null)
        {
            query = query.Where(x => x.DepartmentId == so.DepartmentId);
        }
        return query;
    }
}