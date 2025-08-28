using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class ImageBuilder(IImageService service, IEnumerable<IImageCreator> imageCreators)
{
    private readonly List<IImageToAdd> _items = new();
    private int? _dpi;
    private object? _target;

    public ImageBuilder Add(params IImageToAdd[] items)
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
            ? GetImageFile(new ImageToAdd<object> { Source = _target })
            : null;

        target = _items.Select(ToImageToAdd)
            .Aggregate(
                target,
                (img, imageToAdd) => service.Draw([imageToAdd], img, _dpi)
            );

        return target!;
    }

    protected ImageToAdd ToImageToAdd(IImageToAdd item)
    {
        if (item is ImageToAdd imageToAdd)
        {
            return imageToAdd;
        }

        return new ImageToAdd
        {
            Source = GetImageFile(item),
            Options = item.Options
        };
    }
    protected IImageFile GetImageFile(IImageToAdd item)
    {
        var image = item.Source as IImageFile;

        if (image == null)
        {
            var imageCreator = imageCreators.FirstOrDefault(s => s.CanCreate(item.Source));
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