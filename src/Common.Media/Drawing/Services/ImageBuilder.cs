using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class ImageBuilder(IImageService service, IEnumerable<IImageCreator> imageCreators)
{
    // ReSharper disable PossibleMultipleEnumeration
    private readonly IImageCreator[] _imageCreators = imageCreators.Concat([AggregateImageCreator.Create(service, imageCreators)]).ToArray();
    // ReSharper restore PossibleMultipleEnumeration
    private readonly List<IImageLayer> _items = new();
    private int? _dpi;
    private object? _target;

    public ImageBuilder Add(params IImageLayer[] items)
    {
        _items.AddRange(items);
        return this;
    }
    public ImageBuilder SetDpi(int dpi)
    {
        _dpi = dpi;
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
            ? GetImageFile(new ImageLayer<object> { Source = _target })
            : null;

        target = _items.Select(ToImageToAdd)
            .Aggregate(
                target,
                (img, imageLayer) => service.Draw([imageLayer], img, _dpi)
            );

        return target!;
    }

    protected ImageLayer ToImageToAdd(IImageLayer item)
    {
        if (item is ImageLayer imageLayer)
        {
            return imageLayer;
        }

        return new ImageLayer
        {
            Source = GetImageFile(item),
            Options = item.Options
        };
    }
    protected IImageFile GetImageFile(IImageLayer item)
    {
        var image = item.Source as IImageFile;

        if (image == null)
        {
            var imageCreator = _imageCreators.FirstOrDefault(s => s.CanCreate(item.Source));
            if (imageCreator == null)
            {
                throw new Exception($"ImageCreator not found for type {item.Source.GetType()}");
            }

            image = imageCreator.Create(item.Source);
            if (image == null)
            {
                throw new Exception($"Could not create IImageFile from {item.Source.GetType()}");
            }
        }

        return image;
    }
}