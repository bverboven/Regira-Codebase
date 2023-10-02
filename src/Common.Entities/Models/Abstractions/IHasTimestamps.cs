namespace Regira.Entities.Models.Abstractions;

public interface IHasCreated
{
    public DateTime Created { get; set; }
}
public interface IHasLastModified
{
    public DateTime? LastModified { get; set; }
}

public interface IHasTimestamps : IHasCreated, IHasLastModified
{
}