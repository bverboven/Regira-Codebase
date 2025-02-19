using Entities.Testing.Infrastructure.Data;
using Entities.Testing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;

namespace Entities.Testing;

[TestFixture]
public class ServiceTests
{
    [Test]
    public void EntityType_only()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Category>();
        using var sp = services.BuildServiceProvider();
        var queryBuilder = sp.GetService<IQueryBuilder<Category, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityService1 = sp.GetService<IEntityService<Category>>();
        var entityService2 = sp.GetService<IEntityService<Category, int>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
    }
    [Test]
    public void EntityType_And_KeyType()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Category, int>();
        using var sp = services.BuildServiceProvider();
        var queryBuilder = sp.GetService<IQueryBuilder<Category, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityService = sp.GetService<IEntityService<Category, int>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityService, Is.Not.Null);
    }
    [Test]
    public void EntityType_And_KeyType_And_SearchObject()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Product, int, ProductSearchObject>();
        using var sp = services.BuildServiceProvider();
        var queryBuilder = sp.GetService<IQueryBuilder<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        var entityService1 = sp.GetService<IEntityService<Product, int>>();
        var entityService2 = sp.GetService<IEntityService<Product, int, ProductSearchObject>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
    }
    [Test]
    public void Complex_Service_Without_Key()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Product, ProductSearchObject, EntitySortBy, EntityIncludes>();
        using var sp = services.BuildServiceProvider();
        var queryBuilder = sp.GetService<IQueryBuilder<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        var entityService0 = sp.GetService<IEntityService<Product>>();
        var entityService1 = sp.GetService<IEntityService<Product, int>>();
        var entityService2 = sp.GetService<IEntityService<Product, int, ProductSearchObject>>();
        var entityService3 = sp.GetService<IEntityService<Product, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        var entityService4 = sp.GetService<IEntityService<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityService0, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
        Assert.That(entityService4, Is.Not.Null);
    }
    [Test]
    public void Complex_Service_With_Key()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>();
        using var sp = services.BuildServiceProvider();
        var queryBuilder = sp.GetService<IQueryBuilder<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        var readService2 = sp.GetService<IEntityReadService<Product, int>>();
        var readService5 = sp.GetService<IEntityReadService<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        var writeService3 = sp.GetService<IEntityWriteService<Product, int>>();
        var entityService1 = sp.GetService<IEntityService<Product, int>>();
        var entityService2 = sp.GetService<IEntityService<Product, int, ProductSearchObject>>();
        var entityService3 = sp.GetService<IEntityService<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(readService2, Is.Not.Null);
        Assert.That(readService5, Is.Not.Null);
        Assert.That(writeService3, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }

    [Test]
    public void Custom_EntityService()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Product>(e => e.UseEntityService<ProductService>());
        using var sp = services.BuildServiceProvider();

        var queryBuilder5 = sp.GetService<IQueryBuilder<Product, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityService1 = sp.GetService<IEntityService<Product>>();
        var entityService2 = sp.GetService<IEntityService<Product, int>>();

        Assert.That(queryBuilder5, Is.Not.Null);
        Assert.That(entityService1, Is.TypeOf<ProductService>());
        Assert.That(entityService2, Is.TypeOf<ProductService>());
    }
    [Test]
    public void Custom_QueryBuilder()
    {
        var services = new ServiceCollection()
            .AddDbContext<ProductContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ProductContext>()
            .For<Product>(e => e.UseQueryBuilder<ProductQueryBuilder>());
        using var sp = services.BuildServiceProvider();

        var queryBuilder5 = sp.GetService<IQueryBuilder<Product, int, SearchObject<int>, EntitySortBy, EntityIncludes>>();
        var entityService1 = sp.GetService<IEntityService<Product>>();
        var entityService2 = sp.GetService<IEntityService<Product, int>>();

        Assert.That(queryBuilder5, Is.TypeOf<ProductQueryBuilder>());
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
    }
}