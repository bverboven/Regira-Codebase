using Entities.TestApi.Infrastructure.Persons;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
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
                e.UseEntityService<CourseRepository>();
                e.AddMapping<CourseDto, CourseInputDto>();
                e.HasAttachments<TContext, Course, CourseAttachment>(a =>
                {
                    a.AddMapping<CourseAttachmentDto, CourseAttachmentInputDto>();
                });
                // extra person filter
                e.AddTransient<IFilteredQueryBuilder<Person, PersonSearchObject>, CoursePersonQueryFilter>();
            });

        return services;
    }
}