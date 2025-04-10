using Regira.Dimensions;
using Regira.Drawing.GDI.Utilities;
using Regira.Media.Drawing.Core;
using System.Drawing;

namespace Regira.Drawing.GDI.Helpers;

public class ImageBuilder
{
    private readonly ICollection<ImageToAdd> _images = new List<ImageToAdd>();
    private int? _dpi;
    private Image? _target;
    private readonly DrawHelper _helper = new();
    private Size2D? _targetSize;


    public ImageBuilder SetDpi(int dpi)
    {
        _dpi = dpi;
        return this;
    }
    public ImageBuilder Add(params ImageToAdd[] images)
    {
        foreach (var image in images)
        {
            _images.Add(image);
        }
        return this;
    }
    public ImageBuilder SetTarget(Image target)
    {
        if (_targetSize != null)
        {
            throw new Exception("SetTarget cannot be combined with SetTargetSize");
        }
        _target = target;
        return this;
    }
    public ImageBuilder SetTargetSize(Size2D size)
    {
        if (_target != null)
        {
            throw new Exception("SetTargetSize cannot be combined with SetTarget");
        }
        _targetSize = size;
        return this;
    }

    public Image Build()
    {
        var target = _target ?? (_targetSize != null ? GdiUtility.Create((int)_targetSize.Value.Width, (int)_targetSize.Value.Height) : _helper.CreateSizedCanvas(_images));
        return _helper.Draw(_images, target, _dpi ?? ImageConstants.DEFAULT_DPI);
    }
}