using Regira.Dimensions;
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
            DimensionUnit = dto.DimensionUnit ?? LengthUnit.Points,
            Size = new Size2D((float)(dto.Width ?? 0), (float)(dto.Height ?? 0)),
            Margin = dto.Margin ?? 0,
            PositionType = dto.PositionType ?? ImagePosition.Absolute,
            Position = new Position2D((float?)dto.Top, (float?)dto.Left, (float?)dto.Bottom, (float?)dto.Right),
            Rotation = dto.Rotation ?? 0,
            Opacity = dto.Opacity ?? 1
        };
    }
    public static IImageToAddOptions ToImageToAdd(this TextImageInputDto dto)
    {
        return new TextImageToAdd
        {
            Text = dto.Text,
            TextOptions = new TextImageOptions
            {
                FontName = dto.Options?.FontName,
                FontSize = dto.Options?.FontSize,
                TextColor = dto.Options?.TextColor,
                BackgroundColor = dto.Options?.BackgroundColor,
                Padding = dto.Options?.Padding
            },
            DimensionUnit = dto.DimensionUnit ?? LengthUnit.Points,
            Size = new Size2D((float)(dto.Width ?? 0), (float)(dto.Height ?? 0)),
            Margin = dto.Margin ?? 0,
            PositionType = dto.PositionType ?? ImagePosition.Absolute,
            Position = new Position2D((float?)dto.Top, (float?)dto.Left, (float?)dto.Bottom, (float?)dto.Right),
            Rotation = dto.Rotation ?? 0,
            Opacity = dto.Opacity ?? 1
        };
    }
}