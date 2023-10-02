using Regira.Dimensions;
using Regira.Drawing.Enums;
using Regira.IO.Abstractions;

namespace Regira.Drawing.Abstractions;

public interface IImageFile : IMemoryFile
{
    public Size2D? Size { get; set; }
    ImageFormat? Format { get; set; }
}