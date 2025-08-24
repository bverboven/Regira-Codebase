﻿using Regira.Dimensions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using System.Drawing;
using System.Reflection;
using GdiColor = System.Drawing.Color;
using GdiImageFormat = System.Drawing.Imaging.ImageFormat;
using RegiraColor = Regira.Media.Drawing.Models.Color;
using RegiraImageFormat = Regira.Media.Drawing.Enums.ImageFormat;

namespace Regira.Drawing.GDI.Utilities;

public static class ConversionUtility
{
    public static Size ToGdiSize(this Size2D size)
    {
        return new Size((int)size.Width, (int)size.Height);
    }
    public static IImageFile ToImageFile(this Image img, GdiImageFormat format)
    {
        // keep format required, or file size will be huge
        using var stream = new MemoryStream();
        if (!img.RawFormat.Equals(format))
        {
            using var img2 = GdiUtility.ChangeFormat(img, format);
            img2.Save(stream, format);
        }
        else
        {
            img.Save(stream, format);
        }

        var bytes = stream.ToArray();
        var imgFormat = format.ToImageFormat();

        return new ImageFile
        {
            Bytes = bytes,
            Size = new Size2D(img.Width, img.Height),
            Format = imgFormat,
            ContentType = $"image/{imgFormat.ToString().ToLower()}",
            Length = bytes.Length
        };
    }
    public static Image ToBitmap(this IImageFile file)
    {
        if (file.HasStream())
        {
            return Image.FromStream(file.Stream!);
        }
        using var stream = new MemoryStream(file.GetBytes()!);
        return Image.FromStream(stream);
    }

    public static RegiraImageFormat ToImageFormat(this GdiImageFormat format)
    {
#if NETSTANDARD2_0
        return (RegiraImageFormat)Enum.Parse(typeof(RegiraImageFormat), format.ToString());
#else
        return Enum.Parse<RegiraImageFormat>(format.ToString());
#endif
    }
    public static GdiImageFormat ToGdiImageFormat(this RegiraImageFormat format)
    {
        // https://stackoverflow.com/questions/45448734/how-can-i-convert-a-string-to-an-imageformat-class-property
        var prop = typeof(GdiImageFormat).GetProperty(format.ToString(), BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (prop == null)
        {
            throw new NotSupportedException($"Can't parse {format} to System.Drawing.ImageFormat");
        }
        return (GdiImageFormat)prop.GetValue(null)!;
    }

    // Color
    public static RegiraColor ToColor(this GdiColor color)
        => new(color.R, color.G, color.B, color.A);
    public static GdiColor ToGdiColor(this RegiraColor color)
        => GdiColor.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
}