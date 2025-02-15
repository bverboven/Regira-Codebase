using Entities.Testing.Infrastructure.Normalizers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace Entities.Testing;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
internal class DbContextNormalizerTests
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
    public async Task Test_Order_Of_Registration()
    {
        IServiceCollection services = new ServiceCollection();
        var sp = services
             .AddDbContext<ContosoContext>(db =>
             {
                 db.UseSqlite(_connection);
                 db.AddNormalizerInterceptors(services);
             })
             .UseEntities<ContosoContext>()
             .For<Department>(e =>
             {
                 e.AddNormalizer<Department1Normalizer>();
                 e.AddNormalizer<Department2Normalizer>();
             })
             .BuildServiceProvider();

        var item = Departments.Biology;

        var dbContext = sp.GetRequiredService<ContosoContext>();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Departments.Add(item);
        dbContext.ChangeTracker.DetectChanges();
        await dbContext.SaveChangesAsync();

        Assert.That(item.NormalizedContent, Is.EqualTo("DEPARTMENT_2 DEPARTMENT_1"));
    }

    [Test]
    public async Task Apply_Normalizing_Interceptors()
    {
        IServiceCollection services = new ServiceCollection();
        var sp = services
            .AddDbContext<ContosoContext>(db =>
            {
                db.UseSqlite(_connection);
                db.AddNormalizerInterceptors(services);
            })
            .UseEntities<ContosoContext>(e =>
            {
                e.AddDefaultEntityNormalizer();
                e.AddNormalizer<PersonNormalizer, Person>();
                e.AddNormalizer<InstructorNormalizer, Instructor>();
            })
            .BuildServiceProvider();

        var dbContext = sp.GetRequiredService<ContosoContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var john = People.John;
        var jane = People.Jane;
        var francois = People.Francois;
        var bob = People.Bob;
        var bill = People.Bill;

        bob.Courses = Courses.All.Where(x => x.Instructors!.Contains(People.Bob)).ToList();
        bill.Courses = Courses.All.Where(x => x.Instructors!.Contains(People.Bill)).ToList();

        dbContext.AddRange(john, jane, francois, bob, bill);

        await dbContext.SaveChangesAsync();

        Assert.That(john.NormalizedTitle, Is.EqualTo("Doe John"));
        Assert.That(john.NormalizedContent, Is.EqualTo("PERSON John Doe This is a male test person johndoeemailcom"));

        Assert.That(jane.NormalizedTitle, Is.EqualTo("Doe Jane"));
        Assert.That(jane.NormalizedContent, Is.EqualTo("PERSON Jane Doe This is a female test person 001 234 567 890"));

        Assert.That(francois.NormalizedTitle, Is.EqualTo("Du sacre Coeur Francois"));
        Assert.That(francois.NormalizedContent, Is.EqualTo("PERSON Francois Du sacre Coeur Le poete parisien du xiiie siecle Rutebeuf se fait gravement l echo de la faiblesse humaine de l incertitude et de la pauvrete a l oppose des valeurs courtoises Creme brulee"));

        Assert.That(bob.NormalizedTitle, Is.EqualTo("Kennedy Robert"));
        Assert.That(bob.NormalizedContent, Is.EqualTo("PERSON Robert Kennedy He s an American politician and lawyer known for his roles as US Attorney General and Senator his advocacy for civil rights and social justice and his tragic assassination in 1968 while campaigning for the presidency INSTRUCTING 6 COURSES"));

        Assert.That(bill.NormalizedTitle, Is.EqualTo("Nixon Richard"));
        Assert.That(bill.NormalizedContent, Is.EqualTo("PERSON Richard Nixon He s the 37th President of the United States remembered for his foreign policy achievements and his involvement in the Watergate scandal which led to his resignation in 1974 INSTRUCTING 6 COURSES"));
    }
}