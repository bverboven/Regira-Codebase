using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.IO.Abstractions;

namespace Regira.Video.FFMpeg;

public interface IVideoManager
{
    Task<VideoSettings?> GetInfo(IBinaryFile input);
    Task<IMemoryFile?> Compress(IBinaryFile input, VideoSettings? options = null);
    Task<IImageFile?> Snapshot(IBinaryFile input, Size2D? size = null, TimeSpan? time = null);
}