using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestServiceFor1
{
    [Test]
    public void Simple_Without_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course>()
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
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

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(primers, Is.Empty);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityReadService2, Is.Not.Null);
        Assert.That(entityReadService3, Is.Not.Null);
        Assert.That(entityWriteService, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }
    [Test]
    public void Simple_With_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.UseDefaults())
            .For<Course>()
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, SearchObject<int>>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, SearchObject<int>>>();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var entityService1 = sp.GetService<IEntityService<Course>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(entityNormalizer, Is.TypeOf<DefaultEntityNormalizer>());

        Assert.That(primers, Is.Not.Empty);
        Assert.That(primers.OfType<HasCreatedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<HasLastModifiedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<ArchivablePrimer>(), Is.Not.Empty);

        Assert.That(globalFilters, Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterIdsQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasCreatedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasLastModifiedQueryBuilder<int>>(), Is.Not.Empty);

        Assert.That(queryFilters, Is.Empty);

        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityReadService2, Is.Not.Null);
        Assert.That(entityReadService3, Is.Not.Null);
        Assert.That(entityWriteService, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }

    [Test]
    public void Sort_And_Include()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course>(e =>
            {
                e.SortBy(query => query.OrderBy(x => x.Title));
                e.Includes(query => query.Include(x => x.Instructors));
            })
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

        Assert.That(sortableBuilder, Is.Not.Null);
        Assert.That(includableBuilder, Is.Not.Null);

        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityReadService2, Is.Not.Null);
        Assert.That(entityReadService3, Is.Not.Null);
        Assert.That(entityWriteService, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }
}