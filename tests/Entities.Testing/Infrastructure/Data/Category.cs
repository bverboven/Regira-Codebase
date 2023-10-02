using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Entities.Testing.Infrastructure.Data;

public class Category : IEntity<int>, IHasCode, IHasNormalizedTitle, IHasDescription, ISortable, IHasTimestamps
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped]
    public int? NumberOfProducts { get; set; }

    public ICollection<Product>? Products { get; set; }
}