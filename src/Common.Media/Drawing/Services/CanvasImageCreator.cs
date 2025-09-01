using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Utilities;

namespace Regira.Media.Drawing.Services;

public class CanvasImageCreator(IImageService service) : ImageCreatorBase<CanvasImageOptions>
{
    public override IImageFile Create(CanvasImageOptions input)
    {
        var widthPx = DimensionsUtility.GetPixels(input.Size.Width, input.DimensionUnit ?? ImageLayerDefaults.DimensionUnit, input.Dpi ?? ImageLayerDefaults.Dpi);
        var heightPx = DimensionsUtility.GetPixels(input.Size.Height, input.DimensionUnit ?? ImageLayerDefaults.DimensionUnit, input.Dpi ?? ImageLayerDefaults.Dpi);
        return service.Create(new Size2D(widthPx, heightPx), input.BackgroundColor, input.ImageFormat);
    }
}