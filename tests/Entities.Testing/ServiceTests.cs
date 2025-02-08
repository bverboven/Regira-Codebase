using Entities.Testing.Infrastructure.Data;
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
        var queryBuilder1 = sp.GetService<IQueryBuilder<Category>>();
        //var queryBuilder2 = sp.GetService<IQueryBuilder<Category, int>>();
        var entityService1 = sp.GetService<IEntityService<Category>>();
        var entityService2 = sp.GetService<IEntityService<Category, int>>();
        Assert.That(queryBuilder1, Is.Not.Null);
        //Assert.That(queryBuilder2, Is.Not.Null);
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
        var queryBuilder = sp.GetService<IQueryBuilder<Category, int>>();
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
        var queryBuilder = sp.GetService<IQueryBuilder<Product, int, ProductSearchObject>>();
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
        var entityService1 = sp.GetService<IEntityService<Product, int>>();
        var entityService2 = sp.GetService<IEntityService<Product, int, ProductSearchObject>>();
        var entityService3 = sp.GetService<IEntityService<Product, int, ProductSearchObject, EntitySortBy, EntityIncludes>>();
        Assert.That(queryBuilder, Is.Not.Null);
        Assert.That(entityService1, Is.Not.Null);
        Assert.That(entityService2, Is.Not.Null);
        Assert.That(entityService3, Is.Not.Null);
    }
}