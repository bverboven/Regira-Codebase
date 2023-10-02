using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Entities.Testing.Infrastructure.Data;

public class Product : IEntity<int>, IHasNormalizedTitle, IHasDescription, IHasTimestamps
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public Category? Category { get; set; }
}