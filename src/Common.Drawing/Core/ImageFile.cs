using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.Enums;

namespace Regira.Drawing.Core;

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