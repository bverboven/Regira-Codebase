using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

public abstract class ImageCreatorBase<T> : IImageCreator<T>
    where T : class
{
    bool IImageCreator.CanCreate(object input)
        => input is T item && CanCreate(item);
    public virtual bool CanCreate(T input) => true;

    IImageFile? IImageCreator.Create(object input)
        => TryCreate((T)input);
    protected virtual IImageFile? TryCreate(T input)
        => CanCreate(input) ? Create(input) : null;

    public abstract IImageFile? Create(T input);
}