using Entities.TestApi.Infrastructure.Persons;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Courses;

public class CoursePersonQueryFilter : FilteredQueryBuilderBase<Person, PersonSearchObject>
{
    public override IQueryable<Person> Build(IQueryable<Person> query, PersonSearchObject? so)
    {
        if (so?.StudentCourseIds?.Any() == true)
        {
            query = query.Where(x => (x as Student)!.Enrollments!.Any(e => so.StudentCourseIds.Contains(e.CourseId)));
        }

        return query;
    }
}