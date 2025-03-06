﻿using Entities.DependencyInjection.Testing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.Processing;
using Regira.Entities.EFcore.Processing.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor5Services
{
    [Test]
    public void With_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.UseDefaults())
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>()
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityNormalizer, Is.TypeOf<DefaultEntityNormalizer>());
        Assert.That(globalFilters, Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterIdsQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasCreatedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasLastModifiedQueryBuilder<int>>(), Is.Not.Empty);

        Assert.That(primers, Is.Not.Empty);
        Assert.That(primers.OfType<HasCreatedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<HasLastModifiedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<ArchivablePrimer>(), Is.Not.Empty);

        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void Without_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>()
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_GlobalFilter()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>())
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>()
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(globalFilters, Is.Not.Empty);
        Assert.That(globalFilters.Length, Is.EqualTo(1));
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder>(), Is.Not.Empty);

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Sort()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.SortBy(query => query.OrderBy(x => x.Title));
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(sortableBuilder, Is.Not.Null);
        Assert.That(sortableBuilder, Is.TypeOf<SortedQueryBuilder<Course, int>>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Include()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.Includes<CourseIncludingQueryBuilder>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(includableBuilder, Is.Not.Null);
        Assert.That(includableBuilder, Is.TypeOf<CourseIncludingQueryBuilder>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Include_Shortcut()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.Includes((query, _) => query.Include(x => x.Instructors));
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(includableBuilder, Is.Not.Null);
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<Course, int, CourseIncludes>>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_FilterQueryBuilder()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.AddQueryFilter<CourseQueryFilter1>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(queryFilters.First(), Is.TypeOf<CourseQueryFilter1>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_QueryFilter()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.Filter((query, so) =>
                {
                    if (!string.IsNullOrWhiteSpace(so?.Q))
                    {
                        query = query.Where(x => x.Title!.Contains(so.Q));
                    }
                    return query;
                });
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(queryFilters.First(), Is.TypeOf<EntityQueryFilter<Course, int, CourseSearchObject>>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Processor()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e => e.Process((item, _) => item.HasAttachment = item.Attachments?.Any()))
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityProcessors.Length, Is.EqualTo(1));
        Assert.That(entityProcessors.OfType<EntityProcessor<Course, EntityIncludes>>(), Is.Not.Empty);

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Primer()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e => e.AddPrimer<CoursePrimer>())
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(primers, Is.Not.Empty);
        Assert.That(primers.First(), Is.TypeOf<CoursePrimer>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityService5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Repository()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e => e.HasRepository<CourseRepository5>())
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(repo2, Is.TypeOf<CourseRepository5>());
        Assert.That(repo3, Is.TypeOf<CourseRepository5>());
        Assert.That(repo5, Is.TypeOf<CourseRepository5>());
        Assert.That(entityService2, Is.TypeOf<CourseRepository5>());
        Assert.That(entityService3, Is.TypeOf<CourseRepository5>());
        Assert.That(entityService5, Is.TypeOf<CourseRepository5>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
    }

    [Test]
    public void With_Manager()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.HasManager<CourseManager5>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var manager2 = sp.GetService<IEntityManager<Course, int>>();
        var manager3 = sp.GetService<IEntityManager<Course, int, CourseSearchObject>>();
        var manager5 = sp.GetService<IEntityManager<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(manager2, Is.TypeOf<CourseManager5>());
        Assert.That(manager3, Is.TypeOf<CourseManager5>());
        Assert.That(manager5, Is.TypeOf<CourseManager5>());
        Assert.That(entityService2, Is.TypeOf<CourseManager5>());
        Assert.That(entityService3, Is.TypeOf<CourseManager5>());
        Assert.That(entityService5, Is.TypeOf<CourseManager5>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_EntityService()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e => e.UseEntityService<CourseService5>())
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityService2, Is.TypeOf<CourseService5>());
        Assert.That(entityService3, Is.TypeOf<CourseService5>());
        Assert.That(entityService5, Is.TypeOf<CourseService5>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void With_Custom_EntityService()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                // define custom EntityService interface
                e.AddTransient<ICourseService5, CourseRepository5>();
                e.UseEntityService<CustomCourseService5>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityService2, Is.TypeOf<CustomCourseService5>());
        Assert.That(entityService3, Is.TypeOf<CustomCourseService5>());
        Assert.That(entityService5, Is.TypeOf<CustomCourseService5>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(repo5, Is.TypeOf<EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
    }

    [Test]
    public void Full_Option()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e =>
            {
                e.UseDefaults();
                e.AddGlobalFilterQueryBuilder<HasAttachmentGlobalQueryFilter>();
            })
            .For<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(e =>
            {
                e.SortBy(query => query.OrderBy(x => x.Title));
                e.Includes((query, _) => query.Include(x => x.Instructors));
                e.AddQueryFilter<CourseQueryFilter1>();
                e.AddPrimer<CoursePrimer>();
                e.HasRepository<CourseRepository5>();
                e.HasManager<CourseManager5>();
                e.AddTransient<ICourseService5, CourseManager5>();
                e.UseEntityService<CustomCourseService5>();
            })
            .BuildServiceProvider();

        var entityNormalizer = sp.GetService<IEntityNormalizer>();
        var globalFilters = sp.GetServices<IGlobalFilteredQueryBuilder>().ToArray();
        var queryFilters = sp.GetServices<IFilteredQueryBuilder<Course, int, CourseSearchObject>>().ToArray();
        var sortableBuilder = sp.GetService<ISortedQueryBuilder<Course, int>>();
        var includableBuilder = sp.GetService<IIncludableQueryBuilder<Course, int, CourseIncludes>>();
        var queryBuilder = sp.GetService<IQueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityProcessors = sp.GetServices<IEntityProcessor<Course, EntityIncludes>>().ToArray();
        var entityReadService2 = sp.GetService<IEntityReadService<Course, int>>();
        var entityReadService3 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject>>();
        var entityReadService5 = sp.GetService<IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var primers = sp.GetServices<IEntityPrimer>().ToArray();
        var entityWriteService = sp.GetService<IEntityWriteService<Course, int>>();
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject>>();
        var repo5 = sp.GetService<IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, CourseSearchObject>>();
        var entityService5 = sp.GetService<IEntityService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();
        var manager2 = sp.GetService<IEntityManager<Course, int>>();
        var manager3 = sp.GetService<IEntityManager<Course, int, CourseSearchObject>>();
        var manager5 = sp.GetService<IEntityManager<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>();

        Assert.That(entityNormalizer, Is.TypeOf<DefaultEntityNormalizer>());
        Assert.That(globalFilters.Length, Is.EqualTo(5));
        Assert.That(globalFilters.OfType<FilterIdsQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasCreatedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasLastModifiedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<HasAttachmentGlobalQueryFilter>(), Is.Not.Empty);
        Assert.That(queryFilters.First(), Is.TypeOf<CourseQueryFilter1>());
        Assert.That(sortableBuilder, Is.TypeOf<SortedQueryBuilder<Course, int>>());
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<Course, int, CourseIncludes>>());
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityProcessors, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(entityReadService5, Is.TypeOf<EntityReadService<ContosoContext, Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>>());
        Assert.That(primers.Length, Is.EqualTo(4));
        Assert.That(primers.OfType<HasCreatedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<HasLastModifiedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<ArchivablePrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<CoursePrimer>(), Is.Not.Empty);
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<CourseRepository5>());
        Assert.That(repo3, Is.TypeOf<CourseRepository5>());
        Assert.That(repo5, Is.TypeOf<CourseRepository5>());
        Assert.That(entityService2, Is.TypeOf<CustomCourseService5>());
        Assert.That(entityService3, Is.TypeOf<CustomCourseService5>());
        Assert.That(entityService5, Is.TypeOf<CustomCourseService5>());
        Assert.That(manager2, Is.TypeOf<CourseManager5>());
        Assert.That(manager3, Is.TypeOf<CourseManager5>());
        Assert.That(manager5, Is.TypeOf<CourseManager5>());
    }
}