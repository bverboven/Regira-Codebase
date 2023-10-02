using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Extensions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.Testing;

[TestFixture]
public class ServiceTests
{
    [Test]
    public async Task Details()
    {
        using var scope = GetServiceScope();
        var p = scope.ServiceProvider;
        var dbContext = p.GetRequiredService<ContosoContext>();
        await dbContext.Database.GetDbConnection().OpenAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var service = p.GetRequiredService<IEntityService<Department>>();
        await service.Add(new Department { Title = "Dept #1" });
        await service.SaveChanges();

        var details = await service.Details(1);
        Assert.That(details, Is.Not.Null);
        var details0 = await service.Details(0);
        Assert.That(details0, Is.Null);

        await dbContext.Database.GetDbConnection().CloseAsync();
    }


    public IServiceScope GetServiceScope()
    {
        var services = new ServiceCollection()
            .AddDbContext<ContosoContext>((_, db) => db.UseSqlite("Filename=:memory:"))
            .UseEntities<ContosoContext>()
            .For<Course>()
            .For<Department>();

        return services.BuildServiceProvider().CreateScope();
    }
}