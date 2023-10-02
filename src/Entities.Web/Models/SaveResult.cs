using Regira.Entities.Web.Models.Abstractions;

namespace Regira.Entities.Web.Models;

public class SaveResult<T> : IEntityResult
{
    public long? Duration { get; set; }
    public bool IsNew { get; set; }
    public int Affected { get; set; }
    public T Item { get; set; } = default!;
}