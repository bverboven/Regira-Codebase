using MongoDB.Driver;
using NUnit.Framework.Legacy;
using Regira.DAL.MongoDB;
using Regira.Serializing.Newtonsoft.Json;
using Regira.Utilities;
[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace DAL.MongoDB.Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class RepositoryTests : IDisposable
{
    //private readonly string _personId;
    private readonly MongoDbCommunicator _mongoCommunicator;
    private readonly PersonRepository _personRepo;
    private readonly ConfigRepository _configRepo;
    private readonly MongoDbSettings _mongoSettings;
    public RepositoryTests()
    {
        //_personId = "test-person";
        var serializer = new JsonSerializer();
        _mongoSettings = new MongoDbSettings("localhost", $"Test-{Guid.NewGuid()}");
        _mongoCommunicator = new MongoDbCommunicator(_mongoSettings);
        _personRepo = new PersonRepository(_mongoCommunicator, serializer);
        _configRepo = new ConfigRepository(_mongoCommunicator, serializer);
    }

    [Test]
    public async Task TestMakeConnection()
    {
        var person = new Person { Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        var collections = await _mongoCommunicator.ListCollectionNames().ToListAsync();
        ClassicAssert.IsNotEmpty(collections);

        _personRepo.Delete(person).Wait();
    }


    [Test]
    public async Task TestCreatePersonAutogenerateId()
    {
        var person = new Person { Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        var affected = await _personRepo.Save(person);
        ClassicAssert.AreEqual(1, affected);
        ClassicAssert.IsNotNull(person.Id);

        await _personRepo.Delete(person);
    }
    [Test]
    public async Task TestCreatePersonWithId()
    {
        var person = new Person { Id = Guid.NewGuid().ToString(), Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        var affected = await _personRepo.Save(person);
        ClassicAssert.AreEqual(1, affected);
        ClassicAssert.IsNotNull(person.Id);

        await _personRepo.Delete(person);
    }

    [Test]
    public async Task TestCreateConfig()
    {
        var item = new Config { ConfigId = "123456", Key = "MyKey", Value = "Testing" };
        var affected = await _configRepo.Save(item);
        ClassicAssert.AreEqual(1, affected);

        await _configRepo.Delete(item);
    }

    [Test]
    public async Task TestListPersons()
    {
        var person = new Person { Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        var persons = (await _personRepo.List(new { person.Id })).AsList();
        ClassicAssert.IsNotEmpty(persons);
        ClassicAssert.AreEqual(1, persons.Count);

        await _personRepo.Delete(person);
    }
    [Test]
    public async Task TestGetPersonsWithGivenId()
    {
        var personId = Guid.NewGuid().ToString();
        var person = new Person { Id = personId, Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        var fetchedPerson = await _personRepo.Details(personId);
        ClassicAssert.IsNotNull(fetchedPerson);

        await _personRepo.Delete(fetchedPerson!);
    }

    [Test]
    public async Task UpdatePerson()
    {
        var person = new Person { Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        ClassicAssert.AreEqual(new DateTime(1980, 5, 6), person.BirthDate);
        // ReSharper disable once PossibleInvalidOperationException
        person.BirthDate = person.BirthDate.Value.AddYears(-1);
        var affected = await _personRepo.Save(person);
        ClassicAssert.AreEqual(1, affected);
        var person2 = (await _personRepo.List(new { person.Id })).First();
        ClassicAssert.AreEqual(new DateTime(1979, 5, 6), person2.BirthDate);

        await _personRepo.Delete(person);
    }

    [Test]
    public async Task UpdatePersonWithGivenId()
    {
        var personId = Guid.NewGuid().ToString();
        var person = new Person { Id = personId, Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        ClassicAssert.AreEqual(new DateTime(1980, 5, 6), person.BirthDate);
        // ReSharper disable once PossibleInvalidOperationException
        person.BirthDate = person.BirthDate.Value.AddYears(-1);
        var affected = await _personRepo.Save(person);
        ClassicAssert.AreEqual(1, affected);
        var person2 = await _personRepo.Details(personId);
        ClassicAssert.AreEqual(new DateTime(1979, 5, 6), person2!.BirthDate);

        await _personRepo.Delete(person);
    }

    [Test]
    public async Task DeletePerson()
    {
        var person = new Person { Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        var affected = await _personRepo.Delete(person);
        ClassicAssert.AreEqual(1, affected);
        var persons = await _personRepo.List(new { person.Id });
        ClassicAssert.IsEmpty(persons);
    }
    [Test]
    public async Task DeletePersonWithGivenId()
    {
        var person = new Person { Id = Guid.NewGuid().ToString(), Title = "B.Verboven", BirthDate = new DateTime(1980, 5, 6) };
        await _personRepo.Save(person);

        var affected = await _personRepo.Delete(person);
        ClassicAssert.AreEqual(1, affected);
        var persons = await _personRepo.List(new { person.Id });
        ClassicAssert.IsEmpty(persons);
    }


    public void Dispose()
    {
        var client = new MongoClient(_mongoSettings.ToMongoClientSettings());
        client.DropDatabase(_mongoSettings.DatabaseName);
    }
}