using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;
using Regira.Media.Drawing.Models;
using Regira.Media.Drawing.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Regira.Media.Drawing.Models.Color;
using ImageFile = Regira.Media.Drawing.Models.ImageFile;
using SharpImage = SixLabors.ImageSharp.Image;

namespace Drawing.ImageSharp.Services
{
    public class ImageService : IImageService
    {
        public IImageFile? Parse(Stream? stream)
        {
            if (stream == null)
            {
                return null;
            }

            var options = new DecoderOptions();
            using var image = SharpImage.Load<Rgba32>(options, stream);
            //var format = options.Configuration.ImageFormatsManager.ImageFormats.FirstOrDefault()?.ToImageFormat();
            return image.ToImageFile(stream);
        }
        public IImageFile? Parse(byte[]? bytes)
        {
            if (bytes?.Any() != true)
            {
                return null;
            }

            var options = new DecoderOptions();
            using var image = SharpImage.Load<Rgba32>(options, bytes);
            //var format = options.Configuration.ImageFormatsManager.ImageFormats.FirstOrDefault()?.ToImageFormat();
            return image.ToImageFile(bytes);
        }
        public IImageFile? Parse(IMemoryFile file) => file.HasStream() ? Parse(file.Stream) : Parse(file.GetBytes());

        public ImageFormat GetFormat(IImageFile input)
            => ImageSharpUtility.GetFormat(input.GetStream()!)
                .ToImageFormat();

        public IImageFile ChangeFormat(IImageFile input, ImageFormat targetFormat)
        {
            using var image = input.ToSharpImage();
            using var ms = new MemoryStream();
            var encoder = targetFormat.ToImageSharpFormat().GetEncoder();
            image.SaveAsync(ms, encoder);
            using var convertedImage = SharpImage.Load(ms);
            return convertedImage.ToImageFile(ms);
        }

        public IImageFile CropRectangle(IImageFile input, int[] rect)
        {
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Crop(new Rectangle(rect[0], rect[1], rect[2], rect[3])));
            return image.ToImageFile();
        }

        public IImageFile Resize(IImageFile input, Size2D wantedSize, int quality = 100)
        {
            var targetSize = SizeUtility.CalculateSize((float[])input.Size!, new[] { wantedSize.Width, wantedSize.Height });
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Resize((int)targetSize.Width, (int)targetSize.Height));
            return image.ToImageFile();
        }
        public IImageFile ResizeFixed(IImageFile input, Size2D size, int quality = 100)
        {
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Resize((int)size.Width, (int)size.Height));
            return image.ToImageFile();
        }

        public IImageFile Rotate(IImageFile input, double angle, string? background = null)
        {
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Rotate((float)angle));
            return image.ToImageFile();
        }

        public IImageFile FlipHorizontal(IImageFile input)
        {
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Flip(FlipMode.Horizontal));
            return image.ToImageFile();
        }
        public IImageFile FlipVertical(IImageFile input)
        {
            using var image = input.ToSharpImage();
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            return image.ToImageFile();
        }

        public Color GetPixelColor(IImageFile input, int x, int y)
        {
            throw new NotImplementedException();
        }
        public IImageFile MakeTransparent(IImageFile input, int[]? rgb = null)
        {
            using var image = input.ToSharpImage();

            rgb ??= [245, 245, 245];
            if (rgb is { Length: 3 })
            {
                var targetColor = new Rgba32((byte)rgb[0], (byte)rgb[1], (byte)rgb[2], 255);

                image.Mutate(ctx =>
                {
                    ctx.ProcessPixelRowsAsVector4(new PixelRowOperation(span =>
                    {
                    }));
                });
            }

            return image.ToImageFile();
        }
        public IImageFile RemoveAlpha(IImageFile input)
        {
            using var image = input.ToSharpImage();


            image.Mutate(ctx =>
            {
                ctx.ProcessPixelRowsAsVector4(new PixelRowOperation(span =>
                {
                }));
            });

            return image.ToImageFile();
        }

        public IImageFile CreateTextImage(string input, TextImageOptions? options = null)
        {
            throw new NotImplementedException();
        }
        public IImageFile Draw(IEnumerable<ImageToAdd> imagesToAdd, IImageFile? target = null, int dpi = ImageConstants.DEFAULT_DPI)
        {
            throw new NotImplementedException();
        }
    }
}

public static class ImageSharpUtility
{
    public static IImageFormat GetFormat(Stream stream)
    {
        return SharpImage.DetectFormat(stream);
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

    public static ImageFile ToImageFile(this SharpImage image, Stream? stream = null)
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
    public static ImageFile ToImageFile(this SharpImage image, byte[] bytes)
    {
        var format = image.Metadata.DecodedImageFormat?.ToImageFormat();

        return new ImageFile
        {
            Bytes = bytes,
            Size = new[] { image.Width, image.Height },
            Format = format
        };
    }
    public static SharpImage ToSharpImage(this IImageFile file)
    {
        return file.HasStream()
            ? SharpImage.Load(file.Stream!)
            : SharpImage.Load(file.GetBytes());
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