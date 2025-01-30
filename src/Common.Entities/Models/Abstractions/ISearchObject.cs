namespace Regira.Entities.Models.Abstractions;

public interface ISearchObject : ISearchObject<int>;

public interface ISearchObject<TKey>
{
    TKey? Id { get; set; }
    ICollection<TKey>? Ids { get; set; }
    ICollection<TKey>? Exclude { get; set; }
    string? Q { get; set; }

    DateTime? MinCreated { get; set; }
    DateTime? MaxCreated { get; set; }
    DateTime? MinLastModified { get; set; }
    DateTime? MaxLastModified { get; set; }

    bool? IsArchived { get; set; }
}