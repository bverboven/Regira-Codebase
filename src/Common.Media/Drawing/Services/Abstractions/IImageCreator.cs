using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Services.Abstractions;

public interface IImageCreator
{
    public bool CanCreate(object input);
    public IImageFile? Create(object input);
}
public interface IImageCreator<in T> : IImageCreator
    where T : notnull
{
    public bool CanCreate(T input);
    public IImageFile? Create(T input);
}