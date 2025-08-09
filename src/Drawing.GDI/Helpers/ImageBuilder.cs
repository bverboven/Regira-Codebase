//using Regira.Media.Drawing.Abstractions;
//using Regira.Media.Drawing.Core;

//namespace Regira.Drawing.GDI.Helpers;

//public class ImageBuilder(IImageService service)
//{
//    private readonly ICollection<ImageToAdd> _images = new List<ImageToAdd>();
//    private int? _dpi;
//    private IImageFile? _target;


//    public ImageBuilder SetDpi(int dpi)
//    {
//        _dpi = dpi;
//        return this;
//    }
//    public ImageBuilder Add(params ImageToAdd[] images)
//    {
//        foreach (var image in images)
//        {
//            _images.Add(image);
//        }
//        return this;
//    }
//    public ImageBuilder SetTarget(IImageFile target)
//    {
//        _target = target;
//        return this;
//    }

//    public IImageFile Build()
//    {
//        return service.Draw(_images, _target, _dpi ?? ImageConstants.DEFAULT_DPI);
//    }
//}