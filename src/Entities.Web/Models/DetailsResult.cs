using Regira.Entities.Web.Models.Abstractions;

namespace Regira.Entities.Web.Models;

public class DetailsResult<T> : IEntityResult
{
    public long? Duration { get; set; }
    public T Item { get; set; } = default!;
}