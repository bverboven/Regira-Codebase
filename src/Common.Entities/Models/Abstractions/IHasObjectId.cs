namespace Regira.Entities.Models.Abstractions;

public interface IHasObjectId : IHasObjectId<int>
{
}
public interface IHasObjectId<TKey>
{
    TKey ObjectId { get; set; }
}