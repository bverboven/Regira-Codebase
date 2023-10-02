namespace Regira.Normalizing.Abstractions;

public interface IObjectNormalizer
{
    INormalizer DefaultNormalizer { get; }
    void HandleNormalize(object? item, bool recursive = true);
}