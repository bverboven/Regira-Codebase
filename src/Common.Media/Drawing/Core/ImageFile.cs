using Regira.Dimensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Core;

public class ImageFile : IImageFile
{
    public string? ContentType { get; set; }
    public long Length { get; set; }
    public byte[]? Bytes { get; set; }
    public Stream? Stream { get; set; }
    public Size2D? Size { get; set; }
    public ImageFormat? Format { get; set; }


    public void Dispose()
    {
        Stream?.Dispose();
    }
}