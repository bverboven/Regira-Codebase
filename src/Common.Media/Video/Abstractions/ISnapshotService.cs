using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.Media.Drawing.Abstractions;

namespace Regira.Media.Video.Abstractions;

public interface ISnapshotService
{
    Task<IImageFile?> Snapshot(IBinaryFile input, Size2D? size = null, TimeSpan? time = null);
}