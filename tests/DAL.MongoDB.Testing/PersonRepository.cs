using Regira.DAL.MongoDB;
using Regira.DAL.MongoDB.Abstractions;
using Regira.Serializing.Abstractions;

namespace DAL.MongoDB.Testing;

public class Person
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class PersonRepository : MongoDbRepositoryBase<Person>
{
    public PersonRepository(MongoDbCommunicator communicator, ISerializer serializer)
        : base(communicator, serializer, p => p.Id, (p, id) => p.Id = id, "persons")
    {
    }
}