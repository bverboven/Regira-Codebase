using Entities.Testing.Infrastructure.Data;
using Entities.Testing.Infrastructure.Primers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Legacy;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Entities.Testing;

[TestFixture]
public class PrimerTests
{
    private SqliteConnection _connection = null!;
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
    }
    [TearDown]
    public void TearDown()
    {
        _connection.Close();
    }


    [Test]
    public async Task Execute_EntityPrimer_By_ExtensionMethod()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Product>), _ => new ProductPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var item = new Product
        {
            Title = "Product (test)"
        };
        dbContext.Products.Add(item);

        await dbContext.ApplyPrimers();

        Assert.That(item.Description, Is.EqualTo(ProductPrimer.DescriptionMessage));
        Assert.That(item.Created, Is.EqualTo(DateTime.MinValue));
        Assert.That(item.LastModified, Is.Null);
    }

    [Test]
    public async Task ApplyTo_Interface()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasEncryptedPassword>), _ => new PasswordPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var account = new UserAccount
        {
            Username = "TestUser",
            Password = "Testing Primer"
        };

        dbContext.UserAccounts.Add(account);

        await dbContext.ApplyPrimers();

        ClassicAssert.IsNotNull(account.EncryptedPassword);
        Assert.That(account.EncryptedPassword, Is.EqualTo(account.Password.Base64Encode()));
        Assert.That(account.EncryptedPassword!.Base64Decode(), Is.EqualTo(account.Password));
    }

    [Test]
    public async Task Fill_Timestamps()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasTimestamps>), _ => new TimestampPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var created = DateTime.Now;
        var item = new Product
        {
            Title = "Product (test)",
            Created = created
        };
        dbContext.Products.Add(item);

        await dbContext.ApplyPrimers();
        await dbContext.SaveChangesAsync();

        Assert.That(item.Created, Is.EqualTo(created));
        Assert.That(item.LastModified, Is.Null);

        dbContext.Entry(item).State = EntityState.Modified;

        await dbContext.ApplyPrimers();
        Assert.That(item.Created, Is.EqualTo(created));
        Assert.That(item.LastModified, Is.Not.Null);
        await dbContext.SaveChangesAsync();

        Assert.That(item.LastModified, Is.GreaterThan(item.Created));
    }
    [Test]
    public async Task Fill_ChildCollection_Timestamps()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection).EnableSensitiveDataLogging());
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasTimestamps>), _ => new TimestampPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();

        var created = DateTime.Now.AddDays(-1);

        // use different scopes for insert & update to simulate disconnected update and prevent tracking issues
        using (var insertScope = sp.CreateScope())
        {
            var dbContext = insertScope.ServiceProvider.GetRequiredService<ProductContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var item = new Category
            {
                Title = "Category (test)",
                Created = created,
                Products = new List<Product>
                {
                    new() { Title = "Product (1)", Created = created },
                    new() { Title = "Product (2)" }
                }
            };
            dbContext.Categories.Add(item);

            await dbContext.ApplyPrimers();
            await dbContext.SaveChangesAsync();
        }

        using (var updateScope = sp.CreateScope())
        {
            var dbContext = updateScope.ServiceProvider.GetRequiredService<ProductContext>();
            var modifiedItem = new Category
            {
                Id = 1,
                Title = "Category (updated)",
                Products = new List<Product>
                {
                    new() { Id = 1, Title = "Product (1)", Description = "updated"},
                    new() { Title = "Product (3)", Created = created }
                }
            };

            var originalItem = await dbContext.Categories
                .Include(x => x.Products)
                .AsNoTrackingWithIdentityResolution()
                .SingleAsync(x => x.Id == 1);

            //dbContext
            //    .PrepareEntityChildrenUpdate(originalItem, modifiedItem, x => x.Products!, (x, products) => x.Products = products)
            //    .Submit((original, modified) =>
            //    {
            //        // Primer takes care of this
            //        //modified.Created = original.Created;
            //    });
            dbContext.UpdateEntityChildCollection(originalItem, modifiedItem, x => x.Products, (x, products) => x.Products = products);

            //foreach (var originalProduct in original.Products!)
            //{
            //    var modifiedProduct = modifiedItem.Products?.FirstOrDefault(p => p.Id == originalProduct.Id);
            //    if (modifiedProduct == null)
            //    {
            //        dbContext.Entry(originalProduct).State = EntityState.Deleted;
            //    }
            //    else
            //    {
            //        modifiedProduct.Created = originalProduct.Created;
            //        dbContext.Entry(originalProduct).State = EntityState.Detached;
            //        dbContext.Update(modifiedProduct);
            //    }
            //}
            //original.Products = modifiedItem.Products;

            dbContext.Update(originalItem);
            dbContext.Entry(originalItem).CurrentValues.SetValues(modifiedItem);
            
            await dbContext.ApplyPrimers();

            Assert.That(originalItem.Title, Is.EqualTo(modifiedItem.Title));
            Assert.That(originalItem.Created, Is.EqualTo(created));
            Assert.That(originalItem.LastModified, Is.Not.Null);

            await dbContext.SaveChangesAsync();

            Assert.That(originalItem.Products?.Count, Is.EqualTo(2));
            foreach (var product in originalItem.Products!)
            {
                Assert.That(product.Created, Is.EqualTo(created));
                if (product.Id == 1)
                {
                    Assert.That(product.LastModified, Is.Not.Null);
                    Assert.That(product.LastModified, Is.GreaterThan(product.Created));
                }
                Assert.That(product.CategoryId, Is.EqualTo(originalItem.Id));
            }
        }
    }

    [Test]
    public async Task Execute_Base_Primers()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasTimestamps>), _ => new TimestampPrimer(), ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IEntity>), _ => new NormalizingPrimer(), ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Product>), _ => new ProductPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var dbPrimer = sp.GetRequiredService<EntityPrimerContainer>();
        var item = new Product
        {
            Title = "Product (test)"
        };
        dbContext.Products.Add(item);

        await dbPrimer.ApplyPrimers();
        await dbContext.SaveChangesAsync();

        Assert.That(item.Id, Is.GreaterThan(0));
        Assert.That(item.Description, Is.EqualTo(ProductPrimer.DescriptionMessage));
        Assert.That(item.Created, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(item.LastModified, Is.Null);

        var item1 = await dbContext.Products.FindAsync(item.Id);
        var newTitle = "Product (modified)";
        item1!.Title = newTitle;

        await dbPrimer.ApplyPrimers();
        await dbContext.SaveChangesAsync();

        Assert.That(item1.Title, Is.EqualTo(newTitle));
        Assert.That(item1.NormalizedTitle, Is.EqualTo("Product modified"));
        Assert.That(item1.LastModified, Is.Not.Null);
    }

    [Test]
    public async Task Execute_Base_Primers_In_Order()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IEntity>), _ => new NormalizingPrimer(), ServiceLifetime.Transient));
        // overwrites Normalized title using Uppercase Normalizer
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasNormalizedTitle>), _ => new TitlePrimer(), ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Product>), _ => new ProductPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var dbPrimer = sp.GetRequiredService<EntityPrimerContainer>();
        var item = new Product
        {
            Title = "Product (test)"
        };
        dbContext.Products.Add(item);

        await dbPrimer.ApplyPrimers();

        Assert.That(item.Description, Is.EqualTo(ProductPrimer.DescriptionMessage));
        Assert.That(item.NormalizedTitle, Is.EqualTo("PRODUCT TEST"));
    }

    [Test]
    public async Task Execute_Matching_Primers()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Category>), p => new CategoryPrimer(
            p.GetRequiredService<ProductContext>(),
            new TimestampPrimer(),
            new NormalizingPrimer()
        ), ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Product>), _ => new ProductPrimer(), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();

        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var dbPrimer = sp.GetRequiredService<EntityPrimerContainer>();
        var item = new Category
        {
            Title = "Category (test)"
        };
        dbContext.Categories.Add(item);

        await dbPrimer.ApplyPrimers();

        Assert.That(item.NormalizedTitle, Is.EqualTo("Category test"));
        Assert.That(item.Created, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(item.LastModified, Is.Null);
        Assert.That(item.Description, Is.Null);
    }

    [Test]
    public async Task ApplyTo_Many_Entries()
    {
        var items = Enumerable.Range(0, 10).Select((_, i) => new Category { Title = $"Category #{i + 1}" }).ToArray();

        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ProductContext>(db => db.UseSqlite(_connection));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<IHasTimestamps>), _ => new TimestampPrimer(), ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IEntityPrimer<Category>), p => new CategoryPrimer(
            p.GetRequiredService<ProductContext>(),
            p.GetRequiredService<IEntityPrimer<IHasTimestamps>>(),
            new NormalizingPrimer()
        ), ServiceLifetime.Transient));
        services.RegisterPrimerContainer<ProductContext>();
        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ProductContext>();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Categories.AddRange(items);

        var dbPrimer = sp.GetRequiredService<EntityPrimerContainer>();
        await dbPrimer.ApplyPrimers();

        for (var i = 1; i < items.Length; i++)
        {
            Assert.That(items[i - 1].SortOrder, Is.LessThan(items[i].SortOrder));
        }
    }
}