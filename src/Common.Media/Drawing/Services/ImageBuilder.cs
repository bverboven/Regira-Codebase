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
    private IImageLayer? _target;
    
    /// <summary>
    /// Use an existing image as target to draw on
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /// 
    public ImageBuilder SetBaseLayer(IImageFile target)
    {
        _target = new ImageLayer { Source = target };
        return this;
    }
    /// <summary>
    /// Sets the base layer for the image builder using the specified <see cref="IImageLayer"/>.
    /// </summary>
    /// <param name="target">
    /// The <see cref="IImageLayer"/> to be used as the base layer for drawing operations.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="ImageBuilder"/> to allow method chaining.
    /// </returns>
    public ImageBuilder SetBaseLayer(IImageLayer target)
    {
        _target = target;
        return this;
    }
    /// <summary>
    /// Sets the base layer for the image builder using the specified <see cref="CanvasImageOptions"/>.
    /// </summary>
    /// <param name="target">
    /// The <see cref="CanvasImageOptions"/> instance that defines the size, background color, and image format
    /// to be used as the base layer for drawing operations.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="ImageBuilder"/> to allow method chaining.
    /// </returns>
    public ImageBuilder SetBaseLayer(CanvasImageOptions target)
    {
        _target = new ImageLayer<CanvasImageOptions> { Source = target };
        return this;
    }

    /// <summary>
    /// Adds the specified image layers to the image builder.
    /// </summary>
    /// <param name="items">
    /// An array of <see cref="IImageLayer"/> objects to be added to the image.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="ImageBuilder"/> to allow method chaining.
    /// </returns>
    public ImageBuilder Add(params IImageLayer[] items)
    {
        _items.AddRange(items);
        return this;
    }
    /// <summary>
    /// Adds the specified collection of image layers to the image builder.
    /// </summary>
    /// <param name="items">
    /// A collection of <see cref="IImageLayer"/> objects to be added to the image.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="ImageBuilder"/> to allow method chaining.
    /// </returns>
    public ImageBuilder Add(IEnumerable<IImageLayer> items)
    {
        _items.AddRange(items);
        return this;
    }

    /// <summary>
    /// Builds and returns the resulting <see cref="IImageFile"/> by combining the base layer and additional image layers.
    /// </summary>
    /// <returns>
    /// The constructed <see cref="IImageFile"/> containing the combined image layers.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the base layer cannot be converted into an <see cref="IImageFile"/>.
    /// </exception>
    public IImageFile Build()
    {
        var target = _target != null
            ? _imageCreators.GetImageFile(_target) ?? throw new Exception($"Could not create IImageFile from {_target.GetType()}")
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