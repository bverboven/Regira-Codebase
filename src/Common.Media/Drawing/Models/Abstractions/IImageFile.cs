using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.Media.Drawing.Enums;

namespace Regira.Media.Drawing.Models.Abstractions;

public interface IImageFile : IMemoryFile
{
    public Size2D? Size { get; set; }
    ImageFormat? Format { get; set; }
}