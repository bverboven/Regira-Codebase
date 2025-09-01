using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Dimensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Utilities;
using Regira.Utilities;

namespace Regira.Media.Drawing.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static ImageLayerOptions ToImageLayerOptions(this ImageLayerOptionsDto dto, ImageSize targetSize, int? dpi = null)
    {
        dpi ??= ImageLayerDefaults.Dpi;
        var dimUnit = dto.DimensionUnit ?? ImageLayerDefaults.DimensionUnit;

        return new ImageLayerOptions
        {
            Size = DrawImageUtility.CalculateSize(new Size2D(dto.Width ?? 0, dto.Height ?? 0), dimUnit, targetSize, dpi),
            Margin = DimensionsUtility.GetPixels(dto.Margin ?? ImageLayerDefaults.Margin, dimUnit, Math.Max(targetSize.Width, targetSize.Height), dpi.Value),
            Position = dto.Position ?? ImagePosition.Absolute,
            Offset = DrawImageUtility.CalculateEdgeOffset(new Position2D(dto.Top, dto.Left, dto.Bottom, dto.Right), dimUnit, targetSize, dpi),
            Rotation = dto.Rotation ?? ImageLayerDefaults.Rotation,
            Opacity = dto.Opacity ?? ImageLayerDefaults.Opacity
        };
    }
    public static ImageLayer ToImageLayer(this ImageLayerDto dto, ImageSize targetSize, int? dpi)
    {
        return new ImageLayer
        {
            Source = dto.Image.ToBinaryFile().ToImageFile(),
            Options = dto.DrawOptions?.ToImageLayerOptions(targetSize, dpi),
        };
    }

    public static LabelImageOptions ToLabelImageOptions(this LabelImageLayerDto dto, ImageSize targetSize, int? dpi)
    {
        dpi ??= ImageLayerDefaults.Dpi;
        var dimUnit = dto.LabelOptions?.DimensionUnit ?? ImageLayerDefaults.DimensionUnit;

        return new LabelImageOptions
        {
            Text = dto.Text,
            FontSize = DimensionsUtility.GetPixels(dto.LabelOptions?.FontSize ?? LabelImageDefaults.FontSize, dimUnit, Math.Max(targetSize.Width, targetSize.Height), dpi.Value),
            Padding = DimensionsUtility.GetPixels(dto.LabelOptions?.Padding ?? LabelImageDefaults.Padding, dimUnit, Math.Max(targetSize.Width, targetSize.Height), dpi.Value),
            FontName = dto.LabelOptions?.FontName ?? LabelImageDefaults.FontName,
            TextColor = dto.LabelOptions?.TextColor ?? LabelImageDefaults.TextColor,
            BackgroundColor = dto.LabelOptions?.BackgroundColor ?? LabelImageDefaults.BackgroundColor,
        };
    }
    public static IImageLayer ToImageLayer(this LabelImageLayerDto dto, ImageSize targetSize, int? dpi)
    {
        return new ImageLayer<LabelImageOptions>
        {
            Source = dto.ToLabelImageOptions(targetSize, dpi),
            Options = dto.DrawOptions?.ToImageLayerOptions(targetSize, dpi)
        };
    }

    public static CanvasImageOptions ToCanvasImageOptions(this CanvasImageLayerDto dto, ImageSize targetSize, int? dpi)
    {
        dpi ??= ImageLayerDefaults.Dpi;
        var dimUnit = dto.DimensionUnit ?? ImageLayerDefaults.DimensionUnit;

        return new CanvasImageOptions
        {
            Size = DrawImageUtility.CalculateSize(new Size2D(dto.Width, dto.Height), dimUnit, targetSize, dpi)!.Value,
            BackgroundColor = dto.CanvasOptions?.BackgroundColor ?? ImageDefaults.BackgroundColor,
            ImageFormat = dto.CanvasOptions?.ImageFormat ?? ImageDefaults.Format
        };
    }
    public static IImageLayer ToImageLayer(this CanvasImageLayerDto dto, ImageSize targetSize, int? dpi)
    {
        return new ImageLayer<CanvasImageOptions>
        {
            Source = dto.ToCanvasImageOptions(targetSize, dpi),
            Options = dto.DrawOptions?.ToImageLayerOptions(targetSize, dpi)
        };
    }
}