﻿using Entities.DependencyInjection.Testing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.DependencyInjection.Testing;

[TestFixture]
public class TestFor2Services
{
    [Test]
    public void With_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.UseDefaults())
            .For<Course, int>()
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

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
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void Without_Defaults()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>()
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_GlobalFilter()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>(e => e.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>())
            .For<Course, int>()
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(globalFilters, Is.Not.Empty);
        Assert.That(globalFilters.Length, Is.EqualTo(1));
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder>(), Is.Not.Empty);

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_Sort()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e =>
            {
                e.SortBy(query => query.OrderBy(x => x.Title));
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(sortableBuilder, Is.Not.Null);
        Assert.That(sortableBuilder, Is.TypeOf<SortedQueryBuilder<Course, int>>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_Include()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e =>
            {
                e.Includes(query => query.Include(x => x.Instructors));
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(includableBuilder, Is.Not.Null);
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<Course, int>>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_FilterQuery()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e =>
            {
                e.AddQueryFilter<CourseQueryFilter1>();
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(queryFilters.First(), Is.TypeOf<CourseQueryFilter1>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_Primer()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e => e.AddPrimer<CoursePrimer>())
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(primers, Is.Not.Empty);
        Assert.That(primers.First(), Is.TypeOf<CoursePrimer>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(entityService3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_Repository()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e => e.HasRepository<CourseRepository2>())
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(repo2, Is.TypeOf<CourseRepository2>());
        Assert.That(repo3, Is.TypeOf<CourseRepository2>());
        Assert.That(entityService2, Is.TypeOf<CourseRepository2>());
        Assert.That(entityService3, Is.TypeOf<CourseRepository2>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
    }

    [Test]
    public void With_Manager()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e =>
            {
                e.HasManager<CourseManager2>();
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();
        var manager2 = sp.GetService<IEntityManager<Course, int>>();
        var manager3 = sp.GetService<IEntityManager<Course, int, SearchObject<int>>>();

        Assert.That(manager2, Is.TypeOf<CourseManager2>());
        Assert.That(manager3, Is.TypeOf<CourseManager2>());
        Assert.That(entityService2, Is.TypeOf<CourseManager2>());
        Assert.That(entityService3, Is.TypeOf<CourseManager2>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_EntityService()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e => e.UseEntityService<CourseServiceA>())
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(entityService2, Is.TypeOf<CourseServiceA>());
        Assert.That(entityService3, Is.TypeOf<CourseServiceA>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
    }

    [Test]
    public void With_Custom_EntityService()
    {
        using var sp = new ServiceCollection()
            .AddDbContext<ContosoContext>()
            .UseEntities<ContosoContext>()
            .For<Course, int>(e =>
            {
                // define custom EntityService interface
                e.AddTransient<ICourseService3A, CourseRepository2>();
                e.UseEntityService<CustomCourseServiceA>();
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();

        Assert.That(entityService2, Is.TypeOf<CustomCourseServiceA>());
        Assert.That(entityService3, Is.TypeOf<CustomCourseServiceA>());

        Assert.That(entityNormalizer, Is.Null);
        Assert.That(globalFilters, Is.Empty);
        Assert.That(queryFilters, Is.Empty);
        Assert.That(sortableBuilder, Is.Null);
        Assert.That(includableBuilder, Is.Null);
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers, Is.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<EntityRepository<Course, int>>());
        Assert.That(repo3, Is.TypeOf<EntityRepository<Course, int>>());
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
            .For<Course, int>(e =>
            {
                e.SortBy(query => query.OrderBy(x => x.Title));
                e.Includes(query => query.Include(x => x.Instructors));
                e.AddQueryFilter<CourseQueryFilter1>();
                e.AddPrimer<CoursePrimer>();
                e.HasRepository<CourseRepository2>();
                e.HasManager<CourseManager2>();
                e.AddTransient<ICourseService3A, CourseManager2>();
                e.UseEntityService<CustomCourseServiceA>();
            })
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
        var repo2 = sp.GetService<IEntityRepository<Course, int>>();
        var repo3 = sp.GetService<IEntityRepository<Course, int, SearchObject<int>>>();
        var entityService2 = sp.GetService<IEntityService<Course, int>>();
        var entityService3 = sp.GetService<IEntityService<Course, int, SearchObject<int>>>();
        var manager2 = sp.GetService<IEntityManager<Course, int>>();
        var manager3 = sp.GetService<IEntityManager<Course, int, SearchObject<int>>>();

        Assert.That(entityNormalizer, Is.TypeOf<DefaultEntityNormalizer>());
        Assert.That(globalFilters.Length, Is.EqualTo(5));
        Assert.That(globalFilters.OfType<FilterIdsQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterArchivablesQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasCreatedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<FilterHasLastModifiedQueryBuilder<int>>(), Is.Not.Empty);
        Assert.That(globalFilters.OfType<HasAttachmentGlobalQueryFilter>(), Is.Not.Empty);
        Assert.That(queryFilters.First(), Is.TypeOf<CourseQueryFilter1>());
        Assert.That(sortableBuilder, Is.TypeOf<SortedQueryBuilder<Course, int>>());
        Assert.That(includableBuilder, Is.TypeOf<IncludableQueryBuilder<Course, int>>());
        Assert.That(queryBuilder, Is.TypeOf<QueryBuilder<Course, int>>());
        Assert.That(primers.Length, Is.EqualTo(4));
        Assert.That(primers.OfType<HasCreatedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<HasLastModifiedDbPrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<ArchivablePrimer>(), Is.Not.Empty);
        Assert.That(primers.OfType<CoursePrimer>(), Is.Not.Empty);
        Assert.That(entityReadService2, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityReadService3, Is.TypeOf<EntityReadService<ContosoContext, Course, int, SearchObject<int>>>());
        Assert.That(entityWriteService, Is.TypeOf<EntityWriteService<ContosoContext, Course, int>>());
        Assert.That(repo2, Is.TypeOf<CourseRepository2>());
        Assert.That(repo3, Is.TypeOf<CourseRepository2>());
        Assert.That(entityService2, Is.TypeOf<CustomCourseServiceA>());
        Assert.That(entityService3, Is.TypeOf<CustomCourseServiceA>());
        Assert.That(manager2, Is.TypeOf<CourseManager2>());
        Assert.That(manager3, Is.TypeOf<CourseManager2>());
    }
}