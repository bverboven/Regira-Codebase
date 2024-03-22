using Regira.Entities.Models.Abstractions;

namespace Entities.Testing.Infrastructure.Data
{
    public class User : IEntity<string>
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
    }
}
