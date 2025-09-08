using Entities.TestApi.Infrastructure.Persons;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Attachments;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Testing.Library.Contoso;

namespace Entities.TestApi.Infrastructure.Courses;

public static class CourseServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddCourses<TContext>(this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services
            .For<Course, int, CourseSearchObject>(e =>
            {
                e.Includes((query, _) => query.IncludeEntityAttachments());
                e.AddQueryFilter<CourseQueryFilter>();
                //e.AddMapping<TContext, Course, int, CourseDto, CourseInputDto>();
                e.HasAttachments(
                    course => course.Attachments//, a => a.AddMapping<TContext, CourseAttachment, int, CourseAttachmentDto, CourseAttachmentInputDto>()
                );
                // extra person filter
                e.AddTransient<IFilteredQueryBuilder<Person, int, PersonSearchObject>, CoursePersonQueryFilter>();
            });

        return services;
    }
}