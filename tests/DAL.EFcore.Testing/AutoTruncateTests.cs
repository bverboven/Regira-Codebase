using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Testing.Library.Contoso;

namespace DAL.EFcore.Testing;

[TestFixture]
public class AutoTruncateTests
{

    private SqliteConnection _connection = null!;
    private PeopleContext _dbContext = null!;
    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _dbContext = new PeopleContext(new DbContextOptionsBuilder<PeopleContext>().UseSqlite(_connection).Options);
    }
    [TearDown]
    public void TearDown()
    {
        _connection.Close();
        _dbContext.Dispose();
    }

    [Test]
    public Task Test_AutoTruncate()
    {
        var item = new Person
        {
            GivenName = "Adolph Blaine Charles David Earl Frederick Gerald Hubert Irvin John Kenneth Lloyd Martin Nero Oliver Paul Quincy Randolph Sherman Thomas Uncas Victor William Xerxes Yancy Zeus",
            LastName = "Wolfeschlegel­steinhausen­bergerdorff­welche­vor­altern­waren­gewissenhaft­schafers­wessen­schafe­waren­wohl­gepflege­und­sorgfaltigkeit­beschutzen­vor­angreifen­durch­ihr­raubgierig­feinde­welche­vor­altern­zwolfhundert­tausend­jahres­voran­die­erscheinen­von­der­erste­erdemensch­der­raumschiff­genacht­mit­tungstein­und­sieben­iridium­elektrisch­motors­gebrauch­licht­als­sein­ursprung­von­kraft­gestart­sein­lange­fahrt­hinzwischen­sternartig­raum­auf­der­suchen­nachbarschaft­der­stern­welche­gehabt­bewohnbar­planeten­kreise­drehen­sich­und­wohin­der­neue­rasse­von­verstandig­menschlichkeit­konnte­fortpflanzen­und­sich­erfreuen­an­lebenslanglich­freude­und­ruhe­mit­nicht­ein­furcht­vor­angreifen­vor­anderer­intelligent­geschopfs­von­hinzwischen­sternartig­raum"
        };
        var description = $"His full name is {item.GivenName} {item.LastName}";
        item.Description = description;

        _dbContext.Persons.Add(item);

        Assert.IsTrue(item.GivenName.Length > 32);
        Assert.IsTrue(item.LastName.Length > 64);
        Assert.That(item.Description, Is.EqualTo(description));

        _dbContext.AutoTruncateStringsToMaxLengthForEntries();

        Assert.That(item.GivenName.Length, Is.EqualTo(32));
        Assert.That(item.LastName.Length, Is.EqualTo(64));
        Assert.That(item.Description, Is.EqualTo(description));

        return Task.CompletedTask;
    }
}