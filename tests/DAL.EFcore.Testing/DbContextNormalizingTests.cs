using DAL.EFcore.Testing.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Normalizing;
using Regira.Normalizing.Abstractions;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace DAL.EFcore.Testing;

[TestFixture]
internal class DbContextNormalizingTests
{
    private Person John = null!;
    private Student Jane = null!;
    private Student Francois = null!;
    private Instructor Bob = null!;
    private Instructor Bill = null!;
    private SqliteConnection _connection = null!;
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        John = new Student
        {
            GivenName = "John",
            LastName = "Doe",
            Description = "This is a male test person",
            Email = "john.doe@email.com"
        };
        Jane = new Student
        {
            GivenName = "Jane",
            LastName = "Doe",
            Description = "This is a female test person",
            Phone = "001 234 567 890"
        };
        Francois = new Student
        {
            GivenName = "François",
            LastName = "Du sacre Cœur",
            Description = "Le poète parisien du xiiie siècle Rutebeuf se fait gravement l'écho de la faiblesse humaine, de l'incertitude et de la pauvreté à l'opposé des valeurs courtoises. Crème brûlée"
        };
        Bob = new Instructor
        {
            GivenName = "Robert",
            LastName = "Kennedy",
            Description = "He's an American politician and lawyer, known for his roles as U.S. Attorney General and Senator, his advocacy for civil rights and social justice, and his tragic assassination in 1968 while campaigning for the presidency.",
            Courses = [new() { Title = "Going to the moon" }]
        };
        Bill = new Instructor
        {
            GivenName = "Richard",
            LastName = "Nixon",
            Description = "He's the 37th President of the United States, remembered for his foreign policy achievements and his involvement in the Watergate scandal, which led to his resignation in 1974.",
            Courses = [new() { Title = "Befriending China" }, new() { Title = "How to cheat" }]
        };
    }
    [TearDown]
    public void TearDown()
    {
        _connection.Close();
    }

    [Test]
    public void Find_Normalizers_For_Entity()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ContosoContext>(db => db.UseSqlite(_connection));
        var serviceProvider = services.BuildServiceProvider();

        using var spScope = serviceProvider.CreateScope();
        var sp = spScope.ServiceProvider;

        var container = new ObjectNormalizerContainer(sp);
        container.Register<Person>((_) => new PersonNormalizer());
        container.Register<IHasCourses>((_) => new ItemWithCoursesNormalizer());
        container.Register<Department>(p => new DepartmentNormalizer(p.GetRequiredService<ContosoContext>(), new ItemWithCoursesNormalizer()));

        var personNormalizers = container.FindAll<Person>();
        Assert.That(personNormalizers, Is.Not.Empty);
        Assert.That(personNormalizers.Count, Is.EqualTo(1));

        var instructorNormalizers = container.FindAll<Instructor>();
        Assert.That(instructorNormalizers, Is.Not.Empty);
        Assert.That(instructorNormalizers.Count, Is.EqualTo(2));
    }
    [Test]
    public void Find_Exclusive_Normalizers()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<ContosoContext>(db => db.UseSqlite(_connection));
        var serviceProvider = services.BuildServiceProvider();

        using var spScope = serviceProvider.CreateScope();
        var sp = spScope.ServiceProvider;

        var container = new ObjectNormalizerContainer(sp);
        container.Register<Person>((_) => new PersonNormalizer());
        container.Register<IHasCourses>((_) => new ItemWithCoursesNormalizer());
        container.Register<Department>(p => new DepartmentNormalizer(p.GetRequiredService<ContosoContext>(), new ItemWithCoursesNormalizer()));

        var departmentNormalizer = container.FindAll<Department>();
        Assert.That(departmentNormalizer, Is.Not.Empty);
        Assert.That(departmentNormalizer.Count, Is.EqualTo(1));
    }
    [Test]
    public void Extract_Normalizers_By_Container()
    {
        IServiceCollection services = new ServiceCollection();
        services
            .AddDbContext<ContosoContext>(db => db.UseSqlite(_connection))
            .AddTransient<IObjectNormalizer<Person>, PersonNormalizer>()
            .AddTransient<IObjectNormalizer<IHasCourses>, ItemWithCoursesNormalizer>()
            .AddTransient<IObjectNormalizer<Department>, DepartmentNormalizer>()
            .AddObjectNormalizingContainer();
        var serviceProvider = services.BuildServiceProvider();

        using var spScope = serviceProvider.CreateScope();
        var sp = spScope.ServiceProvider;

        var container = sp.GetRequiredService<ObjectNormalizerContainer>();

        var personNormalizers = container.FindAll<Person>();
        Assert.That(personNormalizers, Is.Not.Empty);
        Assert.That(personNormalizers.Count, Is.EqualTo(1));

        var instructorNormalizers = container.FindAll<Instructor>();
        Assert.That(instructorNormalizers, Is.Not.Empty);
        Assert.That(instructorNormalizers.Count, Is.EqualTo(2));

        var departmentNormalizer = container.FindAll<Department>();
        Assert.That(departmentNormalizer, Is.Not.Empty);
        Assert.That(departmentNormalizer.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Apply_DbContext_Normalization()
    {
        IServiceCollection services = new ServiceCollection();
        services
            .AddDbContext<ContosoContext>(db => db.UseSqlite(_connection))
            .AddTransient<IObjectNormalizer<Person>, PersonNormalizer>()
            .AddTransient<IObjectNormalizer<IHasCourses>, ItemWithCoursesNormalizer>()
            .AddTransient<IObjectNormalizer<Department>, DepartmentNormalizer>()
            .AddObjectNormalizingContainer();
        var serviceProvider = services.BuildServiceProvider();

        using var spScope = serviceProvider.CreateScope();
        var sp = spScope.ServiceProvider;

        var dbContext = sp.GetRequiredService<ContosoContext>();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.AddRange(John, Jane, Francois, Bob, Bill);

        await dbContext.ApplyNormalizers();

        Assert.That(John.NormalizedTitle, Is.EqualTo("Doe John"));
        Assert.That(John.NormalizedContent, Is.EqualTo("John Doe This is a male test person johndoeemailcom"));

        Assert.That(Jane.NormalizedTitle, Is.EqualTo("Doe Jane"));
        Assert.That(Jane.NormalizedContent, Is.EqualTo("Jane Doe This is a female test person 001 234 567 890"));

        Assert.That(Francois.NormalizedTitle, Is.EqualTo("Du sacre Coeur Francois"));
        Assert.That(Francois.NormalizedContent, Is.EqualTo("Francois Du sacre Coeur Le poete parisien du xiiie siecle Rutebeuf se fait gravement l echo de la faiblesse humaine de l incertitude et de la pauvrete a l oppose des valeurs courtoises Creme brulee"));

        Assert.That(Bob.NormalizedTitle, Is.EqualTo("Kennedy Robert"));
        Assert.That(Bob.NormalizedContent, Is.EqualTo("Robert Kennedy He s an American politician and lawyer known for his roles as US Attorney General and Senator his advocacy for civil rights and social justice and his tragic assassination in 1968 while campaigning for the presidency"));

        Assert.That(Bill.NormalizedTitle, Is.EqualTo("Nixon Richard"));
        Assert.That(Bill.NormalizedContent, Is.EqualTo("Richard Nixon He s the 37th President of the United States remembered for his foreign policy achievements and his involvement in the Watergate scandal which led to his resignation in 1974"));
    }
}
