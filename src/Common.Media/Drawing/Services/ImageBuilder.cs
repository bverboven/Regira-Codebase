using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;

namespace Regira.Media.Drawing.Services;

public class ImageBuilder(IImageService service, IEnumerable<IImageCreator> imageCreators)
{
    private readonly List<IImageToAdd> _items = new();
    private int? _dpi;
    private IImageFile? _target;

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
    public ImageBuilder SetTarget(IImageFile target)
    {
        _target = target;
        return this;
    }
    public IImageFile Build()
    {
        var target = _target;

        foreach (var item in _items)
        {
            ImageToAdd? imageToAdd = item as ImageToAdd;
            if (imageToAdd == null)
            {
                var imageCreator = imageCreators.FirstOrDefault(s => s.CanCreate(item.Source));
                if (imageCreator == null)
                {
                    throw new Exception($"ImageCreator not found for type {item.Source.GetType()}");
                }
                var image = imageCreator.Create(item.Source);
                if (image == null)
                {
                    throw new Exception($"Could not create IImageFile from {item.Source.GetType()}");
                }

                imageToAdd = new ImageToAdd
                {
                    Source = image,
                    Options = item.Options
                };
            }
            target = service.Draw([imageToAdd], target, _dpi);
        }

        return target!;
    }
}