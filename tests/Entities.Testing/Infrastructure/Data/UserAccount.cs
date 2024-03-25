using Regira.Entities.Models.Abstractions;

namespace Entities.Testing.Infrastructure.Data;

public class UserAccount : IHasEncryptedPassword
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? EncryptedPassword { get; set; }
}
