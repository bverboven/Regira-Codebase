namespace Regira.Entities.Models.Abstractions;

public interface IHasTitle
{
    public string? Title { get; }
}
public interface IHasNormalizedTitle : IHasTitle
{
    string? NormalizedTitle { get; set; }
}