using Entities.DependencyInjection.Testing.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor3Services
{
    [Test]
    public void Simple_Without_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>()
            .BuildServiceProvider();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, SearchObject<int>>>();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, SearchObject<int>>>();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var entityService1 = sp.GetService<IEntityService<Course>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityReadService2, Is.Not.Null);
        Assert.That(entityReadService3, Is.Not.Null);
        Assert.That(entityWriteService, Is.Not.Null);
        Assert.That(entityService1, Is.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }
    

    [Test]
    public void Basic_With_FilterQuery()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject>(e => e.AddQueryFilter<CourseQueryFilter3>())
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo1 = sp.GetService<IEntityRepository<Course>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var entityService1 = sp.GetService<IEntityService<Course>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();

        Assert.That(globalFilters, Is.Empty);

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Not.Empty);
        Assert.That(queryFilters.Single(), Is.TypeOf<CourseQueryFilter3>());
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo1, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
        Assert.That(entityService1, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject>>());
    }
}