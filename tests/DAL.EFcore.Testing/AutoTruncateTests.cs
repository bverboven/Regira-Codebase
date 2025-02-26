using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.DAL.EFcore.Services;
using Testing.Library.Contoso;
using Testing.Library.Data;

namespace DAL.EFcore.Testing;

[TestFixture]
public class AutoTruncateTests
{
    [Test]
    public Task Test_AutoTruncate_Extension()
    {
        var dbContext = new ContosoContext(
            new DbContextOptionsBuilder<ContosoContext>().UseSqlite("Filename=:memory:").Options
        );
        var item = new Person
        {
            GivenName = "Adolph Blaine Charles David Earl Frederick Gerald Hubert Irvin John Kenneth Lloyd Martin Nero Oliver Paul Quincy Randolph Sherman Thomas Uncas Victor William Xerxes Yancy Zeus",
            LastName = "Wolfeschlegel­steinhausen­bergerdorff­welche­vor­altern­waren­gewissenhaft­schafers­wessen­schafe­waren­wohl­gepflege­und­sorgfaltigkeit­beschutzen­vor­angreifen­durch­ihr­raubgierig­feinde­welche­vor­altern­zwolfhundert­tausend­jahres­voran­die­erscheinen­von­der­erste­erdemensch­der­raumschiff­genacht­mit­tungstein­und­sieben­iridium­elektrisch­motors­gebrauch­licht­als­sein­ursprung­von­kraft­gestart­sein­lange­fahrt­hinzwischen­sternartig­raum­auf­der­suchen­nachbarschaft­der­stern­welche­gehabt­bewohnbar­planeten­kreise­drehen­sich­und­wohin­der­neue­rasse­von­verstandig­menschlichkeit­konnte­fortpflanzen­und­sich­erfreuen­an­lebenslanglich­freude­und­ruhe­mit­nicht­ein­furcht­vor­angreifen­vor­anderer­intelligent­geschopfs­von­hinzwischen­sternartig­raum"
        };
        var description = $"His full name is {item.GivenName} {item.LastName}";
        item.Description = description;

        dbContext.Persons.Add(item);

        Assert.That(item.GivenName.Length > 32, Is.True);
        Assert.That(item.LastName.Length > 64, Is.True);
        Assert.That(item.Description, Is.EqualTo(description));

        dbContext.AutoTruncateStringsToMaxLengthForEntries();

        Assert.That(item.GivenName.Length, Is.EqualTo(32));
        Assert.That(item.LastName.Length, Is.EqualTo(64));
        Assert.That(item.Description, Is.EqualTo(description));

        return Task.CompletedTask;
    }

    [Test]
    public async Task Test_AutoTruncate_Interceptor()
    {
        var services = new ServiceCollection()
            .AddDbContext<ContosoContext>(db =>
            {
                db
                    .UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "test.db")}") // no memory db
                    .AddAutoTruncateInterceptors();
            });
        var sp = services.BuildServiceProvider();

        var dbContext = sp.GetRequiredService<ContosoContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var item = new Person
        {
            GivenName = "Adolph Blaine Charles David Earl Frederick Gerald Hubert Irvin John Kenneth Lloyd Martin Nero Oliver Paul Quincy Randolph Sherman Thomas Uncas Victor William Xerxes Yancy Zeus",
            LastName = "Wolfeschlegel­steinhausen­bergerdorff­welche­vor­altern­waren­gewissenhaft­schafers­wessen­schafe­waren­wohl­gepflege­und­sorgfaltigkeit­beschutzen­vor­angreifen­durch­ihr­raubgierig­feinde­welche­vor­altern­zwolfhundert­tausend­jahres­voran­die­erscheinen­von­der­erste­erdemensch­der­raumschiff­genacht­mit­tungstein­und­sieben­iridium­elektrisch­motors­gebrauch­licht­als­sein­ursprung­von­kraft­gestart­sein­lange­fahrt­hinzwischen­sternartig­raum­auf­der­suchen­nachbarschaft­der­stern­welche­gehabt­bewohnbar­planeten­kreise­drehen­sich­und­wohin­der­neue­rasse­von­verstandig­menschlichkeit­konnte­fortpflanzen­und­sich­erfreuen­an­lebenslanglich­freude­und­ruhe­mit­nicht­ein­furcht­vor­angreifen­vor­anderer­intelligent­geschopfs­von­hinzwischen­sternartig­raum"
        };
        var description = $"His full name is {item.GivenName} {item.LastName}";
        item.Description = description;

        dbContext.Persons.Add(item);

        Assert.That(item.GivenName.Length > 32, Is.True);
        Assert.That(item.LastName.Length > 64, Is.True);
        Assert.That(item.Description, Is.EqualTo(description));

        await dbContext.SaveChangesAsync();

        Assert.That(item.GivenName.Length, Is.EqualTo(32));
        Assert.That(item.LastName.Length, Is.EqualTo(64));
        Assert.That(item.Description, Is.EqualTo(description));

        await dbContext.Database.EnsureDeletedAsync();
    }
}