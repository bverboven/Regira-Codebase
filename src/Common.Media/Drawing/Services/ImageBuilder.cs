using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Media.Drawing.Utilities;

namespace Regira.Media.Drawing.Services;

public class ImageBuilder(IImageService service)
{
    private readonly List<IImageToAddOptions> _items = new();
    private int? _dpi;
    private IImageFile? _target;

    public ImageBuilder Add(params IImageToAddOptions[] items)
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
        var dpi = _dpi ?? ImageConstants.DEFAULT_DPI;
        var target = _target ?? service.Create(
            _items.Max(x => SizeUtility.GetPixels(x.Width, x.DimensionUnit, 0, dpi)),
            _items.Max(x => SizeUtility.GetPixels(x.Height, x.DimensionUnit, 0, dpi))
        );

        foreach (var item in _items)
        {
            var disposeImg = false;
            ImageToAdd image;
            if (item is TextImageToAdd textImage)
            {
                image = GetTextImage(textImage);
                disposeImg = true;
            }
            else
            {
                image = (ImageToAdd)item;
            }
            target = service.Draw([image], target, dpi);

            if (disposeImg)
            {
                image.Image.Dispose();
            }
        }

        return target;
    }

    ImageToAdd GetTextImage(TextImageToAdd item) =>
        new()
        {
            Image = service.CreateTextImage(item.Text, item.TextOptions),
            Margin = item.Margin,
            Bottom = item.Bottom,
            DimensionUnit = item.DimensionUnit,
            Height = item.Height,
            Left = item.Left,
            Opacity = item.Opacity,
            Position = item.Position,
            Right = item.Right,
            Rotation = item.Rotation,
            Top = item.Top,
            Width = item.Width
        };
}