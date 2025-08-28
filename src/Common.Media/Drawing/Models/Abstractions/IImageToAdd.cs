namespace Regira.Media.Drawing.Models.Abstractions;

public interface IImageToAdd
{
    public object Source { get; set; }
    public ImageToAddOptions? Options { get; set; }
}
public interface IImageToAdd<T> : IImageToAdd
{
    public new T Source { get; set; }
}