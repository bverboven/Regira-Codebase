using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testing.Library.Contoso;

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
    private PeopleContext _dbContext = null!;
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _dbContext = new PeopleContext(new DbContextOptionsBuilder<PeopleContext>().UseSqlite(_connection).Options);
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
        _dbContext.Dispose();
    }

    [Test]
    public async Task ApplyDbContextNormalization()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<PeopleContext>(db => db.UseSqlite(_connection));

        var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<PeopleContext>();
        await dbContext.Database.EnsureCreatedAsync();

        _dbContext.AddRange(John, Jane, Francois, Bob, Bill);

        await _dbContext.SaveChangesAsync();
    }
}
