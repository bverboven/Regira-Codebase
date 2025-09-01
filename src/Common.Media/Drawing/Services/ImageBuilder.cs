using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Media.Drawing.Utilities;

namespace Regira.Media.Drawing.Services;

public class ImageBuilder(IImageService service, IEnumerable<IImageCreator> imageCreators)
{
    // ReSharper disable PossibleMultipleEnumeration
    private readonly IImageCreator[] _imageCreators = imageCreators.Concat([AggregateImageCreator.Create(service, imageCreators)]).ToArray();
    // ReSharper restore PossibleMultipleEnumeration
    private readonly List<IImageLayer> _items = [];
    private object? _target;

    public ImageBuilder Add(params IImageLayer[] items)
    {
        _items.AddRange(items);
        return this;
    }
    /// <summary>
    /// Use an existing image as target to draw on
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public ImageBuilder SetTargetImage(IImageFile target)
    {
        _target = target;
        return this;
    }
    /// <summary>
    /// Target requires a matching <see cref="IImageCreator"/>> to be found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <returns></returns>
    public ImageBuilder SetTargetObject<T>(T target)
    {
        _target = target;
        return this;
    }
    public IImageFile Build()
    {
        var target = _target != null
            ? _imageCreators.GetImageFile(new ImageLayer<object> { Source = _target }) ?? throw new Exception($"Could not create IImageFile from {_target.GetType()}")
            : null;

        var result = _items.Select(ToImageLayer)
            .Aggregate(
                target,
                (img, imageLayer) => service.Draw([imageLayer], img)
            )!;

        return result;
    }

    /// <summary>
    /// Ensures that the source of the layer is converted into an <see cref="IImageFile"/> using the available <see cref="IImageCreator"/> implementations.
    /// </summary>
    /// <param name="item">
    /// The <see cref="IImageLayer"/> instance to be converted.
    /// </param>
    /// <returns>
    /// An <see cref="ImageLayer"/> instance containing the source and options from the provided <paramref name="item"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the source object of the provided <paramref name="item"/> cannot be converted into an <see cref="IImageFile"/>.
    /// </exception>
    protected ImageLayer ToImageLayer(IImageLayer item)
    {
        if (item is ImageLayer imageLayer)
        {
            return imageLayer;
        }

        return new ImageLayer
        {
            Source = _imageCreators.GetImageFile(item) ?? throw new Exception($"Could not create IImageFile from {item.Source.GetType()}"),
            Options = item.Options
        };
    }
}