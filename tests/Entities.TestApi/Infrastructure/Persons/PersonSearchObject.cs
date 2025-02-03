using Regira.Entities.Models;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonSearchObject : SearchObject
{
    public string? GivenName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}