using Regira.Dimensions;
using Regira.Media.Drawing.Constants;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models.Abstractions;

namespace Regira.Media.Drawing.Models.DTO.Extensions;

public static class DtoExtensions
{
    public static ImageToAdd ToImageToAdd(this ImageToAddDto dto)
    {
        return new ImageToAdd
        {
            Image = new ImageFile
            {
                Bytes = dto.Image
            },
            DimensionUnit = dto.DimensionUnit ?? DrawImageDefaults.DimensionUnit,
            Size = new Size2D(dto.Width ?? 0, dto.Height ?? 0),
            Margin = dto.Margin ?? DrawImageDefaults.Margin,
            PositionType = dto.PositionType ?? ImagePosition.Absolute,
            Position = new Position2D(dto.Top, dto.Left, dto.Bottom, dto.Right),
            Rotation = dto.Rotation ?? DrawImageDefaults.Rotation,
            Opacity = dto.Opacity ?? DrawImageDefaults.Opacity
        };
    }
    public static IImageToAddOptions ToImageToAdd(this TextImageInputDto dto)
    {
        return new TextImageToAdd
        {
            Text = dto.Text,
            TextOptions = new TextImageOptions
            {
                FontName = dto.Options?.FontName ?? TextImageDefaults.FontName,
                FontSize = dto.Options?.FontSize ?? TextImageDefaults.FontSize,
                TextColor = dto.Options?.TextColor ?? TextImageDefaults.TextColor,
                BackgroundColor = dto.Options?.BackgroundColor ?? TextImageDefaults.BackgroundColor,
                Padding = dto.Options?.Padding ?? TextImageDefaults.Padding
            },
            DimensionUnit = dto.DimensionUnit ?? LengthUnit.Points,
            Size = new Size2D(dto.Width ?? 0, dto.Height ?? 0),
            Margin = dto.Margin ?? 0,
            PositionType = dto.PositionType ?? ImagePosition.Absolute,
            Position = new Position2D(dto.Top, dto.Left, dto.Bottom, dto.Right),
            Rotation = dto.Rotation ?? 0,
            Opacity = dto.Opacity ?? 1
        };
    }
}