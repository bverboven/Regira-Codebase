using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Models;

public record ImageToAdd<T> : IImageToAdd<T>
    where T : class
{
    public T Source { get; set; } = null!;
    object IImageToAdd.Source
    {
        get => Source;
        set => Source = (T)value;
    }

    public ImageToAddOptions? Options { get; set; } = new();
}

public record ImageToAdd : ImageToAdd<IImageFile>;