using FFMpegCore;
using FFMpegCore.Pipes;
using Regira.Dimensions;
using Regira.Drawing.Abstractions;
using Regira.Drawing.Core;
using Regira.Drawing.Utilities;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using System.Drawing;
using FFMegService = FFMpegCore.FFMpeg;

namespace Regira.Video.FFMpeg;

public class VideoManager(IImageService imageService) : IVideoManager
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
        var inputPath = input.GetPath();
        if (!size.HasValue || size.Value.Width == 0 || size.Value.Height == 0)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
            size = new Size2D(mediaInfo.PrimaryVideoStream!.Width, mediaInfo.PrimaryVideoStream!.Height);
        }

        var tempFile = Path.GetTempFileName();
        var success = await FFMegService.SnapshotAsync(inputPath, tempFile, new Size((int)size.Value.Width, (int)size.Value.Height), time);
        if (!success)
        {
            throw new Exception("Internal error while creating snapshot");
        }
        using var img = new ImageFile();
        img.Load(tempFile);
        if (img.Length <= 0)
        {
            throw new Exception("Empty file");
        }
        using var jpeg = imageService.ChangeFormat(img, Drawing.Enums.ImageFormat.Jpeg);
        using var resized = imageService.Resize(jpeg, size.Value);

        File.Delete(tempFile);

        return resized;
    }
}