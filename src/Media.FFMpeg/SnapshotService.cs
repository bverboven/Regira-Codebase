using FFMpegCore;
using Regira.Dimensions;
using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Core;
using Regira.Media.Drawing.Utilities;
using Regira.System;
using Regira.System.Abstractions;

namespace Regira.Media.FFMpeg;

public class SnapshotService(IImageService imageService, IProcessHelper? processHelper = null)
{
    readonly IProcessHelper _processHelper = processHelper ?? new ProcessHelper();

    public async Task<IImageFile?> Snapshot(IBinaryFile input, Size2D? size = null, TimeSpan? time = null)
    {
        var inputPath = input.GetPath();
        if (!size.HasValue || size.Value.Width == 0 || size.Value.Height == 0)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
            size = new Size2D(mediaInfo.PrimaryVideoStream!.Width, mediaInfo.PrimaryVideoStream!.Height);
        }

        var tempFile = $"{Path.GetTempFileName()}.bmp";
        var ss = time?.ToString()?.Substring(0, 12);
        var cmd = $@"ffmpeg -i ""{inputPath}"" -ss {ss} -update 1 -frames:v 1 ""{tempFile}""";
        var result = _processHelper.ExecuteCommand(cmd);

        //var success = await FFMegService.SnapshotAsync(inputPath, tempFile, new Size((int)size.Value.Width, (int)size.Value.Height), time);
        if (result.ExitCode != 0)
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