using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Utilities;

namespace Regira.Media.Drawing.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static ImageLayerOptions ToImageLayerOptions(this ImageLayerOptionsDto dto)
    {
        return new ImageLayerOptions
        {
            DimensionUnit = dto.DimensionUnit ?? DrawImageDefaults.DimensionUnit,
            Size = new Size2D(dto.Width ?? 0, dto.Height ?? 0),
            Margin = dto.Margin ?? DrawImageDefaults.Margin,
            PositionType = dto.Position ?? ImagePosition.Absolute,
            Position = new Position2D(dto.Top, dto.Left, dto.Bottom, dto.Right),
            Rotation = dto.Rotation ?? DrawImageDefaults.Rotation,
            Opacity = dto.Opacity ?? DrawImageDefaults.Opacity
        };
    }
    
    public static ImageLayer ToImageLayer(this ImageLayerDto dto)
    {
        return new ImageLayer
        {
            Source = dto.Image.ToBinaryFile().ToImageFile(),
            Options = dto.DrawOptions?.ToImageLayerOptions(),
        };
    }

    public static LabelImageOptions ToTextImageOptions(this LabelImageLayerDto dto)
        => new()
        {
            Text = dto.Text,
            FontName = dto.LabelOptions?.FontName ?? LabelImageDefaults.FontName,
            FontSize = dto.LabelOptions?.FontSize ?? LabelImageDefaults.FontSize,
            TextColor = dto.LabelOptions?.TextColor ?? LabelImageDefaults.TextColor,
            BackgroundColor = dto.LabelOptions?.BackgroundColor ?? LabelImageDefaults.BackgroundColor,
            Padding = dto.LabelOptions?.Padding ?? LabelImageDefaults.Padding
        };
    public static IImageLayer ToImageLayer(this LabelImageLayerDto dto)
    {
        return new ImageLayer<LabelImageOptions>
        {
            Source = dto.ToTextImageOptions(),
            Options = dto.DrawOptions?.ToImageLayerOptions()
        };
    }

    public static CanvasImageOptions ToCanvasImageOptions(this CanvasImageLayerDto dto)
        => new()
        {
            Size = new Size2D(dto.Width, dto.Height),
            BackgroundColor = dto.CanvasOptions?.BackgroundColor ?? ImageDefaults.BackgroundColor,
            ImageFormat = dto.CanvasOptions?.ImageFormat ?? ImageDefaults.Format
        };
    public static IImageLayer ToImageLayer(this CanvasImageLayerDto dto)
    {
        return new ImageLayer<CanvasImageOptions>
        {
            Source = dto.ToCanvasImageOptions(),
            Options = dto.DrawOptions?.ToImageLayerOptions()
        };
    }
}