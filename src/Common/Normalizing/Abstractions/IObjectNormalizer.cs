namespace Regira.Normalizing.Abstractions;

public interface IObjectNormalizer
{
    INormalizer DefaultNormalizer { get; }
    Task HandleNormalizeMany(IEnumerable<object?> instances, bool recursive = true);
    void HandleNormalize(object? item, bool recursive = true);
}