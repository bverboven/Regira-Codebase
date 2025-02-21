using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor2Services
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
}