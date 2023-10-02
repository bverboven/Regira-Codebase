using Regira.Entities.Web.Models.Abstractions;

namespace Regira.Entities.Web.Models;

public class SearchResult<T> : IEntityResult
{
    public long? Duration { get; set; }
    public int Count { get; set; }
    public IList<T> Items { get; set; } = null!;
}