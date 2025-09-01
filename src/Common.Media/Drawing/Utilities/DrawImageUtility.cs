using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Services.Abstractions;
using Regira.Utilities;

namespace Regira.Media.Drawing.Utilities;

public static class DrawImageUtility
{
    public static IImageFile? Create<T>(this IEnumerable<IImageCreator> services, T input)
        where T : class
        => services.FirstOrDefault(s => s.CanCreate(input))?.Create(input);

    public static IImageFile? GetImageFile(this IEnumerable<IImageCreator> services, IImageToAdd item)
        => item.Source as IImageFile ?? services.Create(item.Source);

    public static Coordinate2D GetCoordinate(ImageToAddOptions options, Size2D targetSize, Size2D imageSize, int? dpi)
    {
        dpi ??= DrawImageDefaults.Dpi;

        var inputPosition = options.Position ?? new Position2D();
        int? imgLeft = inputPosition.Left.HasValue ? DimensionsUtility.GetPixels(inputPosition.Left.Value, options.DimensionUnit, (int)targetSize.Width, dpi.Value) : null;
        int? imgRight = inputPosition.Right.HasValue ? DimensionsUtility.GetPixels(inputPosition.Right.Value, options.DimensionUnit, (int)targetSize.Width, dpi.Value) : null;
        int? imgTop = inputPosition.Top.HasValue ? DimensionsUtility.GetPixels(inputPosition.Top.Value, options.DimensionUnit, (int)targetSize.Height, dpi.Value) : null;
        int? imgBottom = inputPosition.Bottom.HasValue ? DimensionsUtility.GetPixels(inputPosition.Bottom.Value, options.DimensionUnit, (int)targetSize.Height, dpi.Value) : null;
        int imgMargin = options.Margin > 0 ? DimensionsUtility.GetPixels(options.Margin, options.DimensionUnit, (int)Math.Max(targetSize.Width, targetSize.Height), dpi.Value) : 0;

        float left = 0;
        if (options.PositionType.HasFlag(ImagePosition.HCenter))
        {
            left = targetSize.Width / 2 - imageSize.Width / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Right) || Math.Abs(left) < float.Epsilon && inputPosition.Right.HasValue)
        {
            left = targetSize.Width - imageSize.Width - imgMargin;
        }
        else
        {
            left += imgMargin;
        }

        float top = 0;
        if (options.PositionType.HasFlag(ImagePosition.VCenter))
        {
            top = targetSize.Height / 2 - imageSize.Height / 2f;
        }
        else if (options.PositionType.HasFlag(ImagePosition.Bottom) || Math.Abs(top) < float.Epsilon && inputPosition.Bottom.HasValue)
        {
            top = targetSize.Height - imageSize.Height - imgMargin;
        }
        else
        {
            top += imgMargin;
        }

        left += (imgLeft ?? 0) - (imgRight ?? 0);
        top += (imgTop ?? 0) - (imgBottom ?? 0);

        return new[] { left, top };
    }
}