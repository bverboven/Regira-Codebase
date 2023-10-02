using FFMpegCore;
using FFMpegCore.Pipes;
using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.GDI.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using System.Drawing.Imaging;
using FFMegService = FFMpegCore.FFMpeg;

namespace Regira.Video.FFMpeg;

public class VideoManager : IVideoManager
{
    // https://github.com/rosenbjerg/FFMpegCore


    private const int DEFAULT_FRAMERATE = 30;
    public async Task<VideoSettings?> GetInfo(IBinaryFile input)
    {
        var inputPath = input.GetPath();
        var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
        var videoStream = mediaInfo.PrimaryVideoStream!;
        return new VideoSettings
        {
            FrameRate = (int)videoStream.FrameRate,
            Size = new Size2D(videoStream.Width, videoStream.Height)
        };
    }

    public async Task<IMemoryFile?> Compress(IBinaryFile input, VideoSettings? options = null)
    {
        options ??= new();

        var inputPath = input.GetPath();
        var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
        VideoStream? videoStream = null;
        if (options.FrameRate.HasValue != true || options.Size.HasValue != true)
        {
            videoStream = mediaInfo.PrimaryVideoStream!;
        }

        options.FrameRate ??= (int)(videoStream!.FrameRate * .9);
        options.Size ??= new Size2D((videoStream!.Width * .5f), (videoStream.Height * .5f));

        var size = options.Size;
        var frameRate = options.FrameRate ?? DEFAULT_FRAMERATE;

        var ms = new MemoryStream();
        var outputPipe = new StreamPipeSink(ms);

        await FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToPipe(outputPipe, o =>
            {
                o
                    .WithConstantRateFactor(frameRate)
                    .WithVideoCodec("vp9")
                    .ForceFormat("webm")
                    .WithVideoFilters(filterOptions =>
                    {
                        filterOptions.Scale((int)size.Value.Width, (int)size.Value.Height);
                    })
                    .WithFastStart();
            })
            .ProcessAsynchronously();

        ms.Seek(0, SeekOrigin.Begin);

        return ms.ToMemoryFile();
    }

    public async Task<IImageFile?> Snapshot(IBinaryFile input, Size2D? size = null, TimeSpan? time = null)
    {
        time ??= TimeSpan.FromSeconds(10);
        var inputPath = input.GetPath();
        if (!size.HasValue || size.Value.Width == 0 || size.Value.Height == 0)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
            size = new Size2D(mediaInfo.PrimaryVideoStream!.Width, mediaInfo.PrimaryVideoStream!.Height);
        }

        var gdiSize = size.Value.ToSize();
        using var bitmap = await FFMegService.SnapshotAsync(inputPath, gdiSize, time);
        using var jpeg = GdiUtility.ChangeFormat(bitmap, ImageFormat.Jpeg);
        using var resized = GdiUtility.Resize(jpeg, gdiSize);
        return resized.ToImageFile(ImageFormat.Jpeg);
    }
}