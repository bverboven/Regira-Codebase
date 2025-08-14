using Regira.IO.Extensions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Models.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

public static class ImageSharpUtility
{
    public static IImageFormat GetFormat(Stream stream)
    {
        return Image.DetectFormat(stream);
    }
    public static IImageFormat ToImageSharpFormat(this ImageFormat format)
    {
        var mgr = new ImageFormatManager();
        if (mgr.TryFindFormatByFileExtension($".{format}", out var imageSharpFormat))
        {
            return imageSharpFormat;
        }

        return PngFormat.Instance;
    }
    public static ImageFormat ToImageFormat(this IImageFormat format)
    {
        return Enum.Parse<ImageFormat>(format.Name, true);
    }

    public static ImageFile ToImageFile(this Image image, Stream? stream = null)
    {
        var format = image.Metadata.DecodedImageFormat?.ToImageFormat();

        if (stream == null)
        {
            stream = new MemoryStream();
            image.Save(stream, image.Metadata.DecodedImageFormat!);
        }

        return new ImageFile
        {
            Stream = stream,
            Size = new[] { image.Width, image.Height },
            Format = format
        };
    }
    public static ImageFile ToImageFile(this Image image, byte[] bytes)
    {
        var format = image.Metadata.DecodedImageFormat?.ToImageFormat();

        return new ImageFile
        {
            Bytes = bytes,
            Size = new[] { image.Width, image.Height },
            Format = format
        };
    }
    public static Image ToSharpImage(this IImageFile file)
    {
        return file.HasStream()
            ? Image.Load(file.Stream!)
            : Image.Load(file.GetBytes());
    }

    public static IImageEncoder GetEncoder(this IImageFormat format)
    {
        return format switch
        {
            JpegFormat => new JpegEncoder(),
            BmpFormat => new BmpEncoder(),
            GifFormat => new GifEncoder(),
            TiffFormat => new TiffEncoder(),
            WebpFormat => new WebpEncoder(),
            _ => new PngEncoder()
        };
    }
}