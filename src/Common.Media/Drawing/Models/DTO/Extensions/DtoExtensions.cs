using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;
using Regira.Media.Drawing.Utilities;

namespace Regira.Media.Drawing.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static ImageToAddOptions ToImageToAddOptions(this ImageToAddOptionsDto dto)
    {
        return new ImageToAddOptions
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
    
    public static ImageToAdd ToImageToAdd(this ImageToAddDto dto)
    {
        return new ImageToAdd
        {
            Source = dto.Image.ToBinaryFile().ToImageFile(),
            Options = dto.DrawOptions?.ToImageToAddOptions(),
        };
    }

    public static TextImageOptions ToTextImageOptions(this TextImageInputDto dto)
        => new()
        {
            Text = dto.Text,
            FontName = dto.TextOptions?.FontName ?? TextImageDefaults.FontName,
            FontSize = dto.TextOptions?.FontSize ?? TextImageDefaults.FontSize,
            TextColor = dto.TextOptions?.TextColor ?? TextImageDefaults.TextColor,
            BackgroundColor = dto.TextOptions?.BackgroundColor ?? TextImageDefaults.BackgroundColor,
            Padding = dto.TextOptions?.Padding ?? TextImageDefaults.Padding
        };
    public static IImageToAdd ToImageToAdd(this TextImageInputDto dto)
    {
        return new ImageToAdd<TextImageOptions>
        {
            Source = dto.ToTextImageOptions(),
            Options = dto.DrawOptions?.ToImageToAddOptions()
        };
    }

    public static CanvasImageOptions ToCanvasImageOptions(this CanvasImageInputDto dto)
        => new()
        {
            Size = new Size2D(dto.Width, dto.Height),
            BackgroundColor = dto.CanvasOptions?.BackgroundColor ?? ImageDefaults.BackgroundColor,
            ImageFormat = dto.CanvasOptions?.ImageFormat ?? ImageDefaults.Format
        };
    public static IImageToAdd ToImageToAdd(this CanvasImageInputDto dto)
    {
        return new ImageToAdd<CanvasImageOptions>
        {
            Source = dto.ToCanvasImageOptions(),
            Options = dto.DrawOptions?.ToImageToAddOptions()
        };
    }
}