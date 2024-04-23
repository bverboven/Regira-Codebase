using Regira.Drawing.SkiaSharp.Services;
using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.Media.FFMpeg;
using Regira.System;

var compressor = new VideoManager();
var snapshooter = new SnapshotService(new ImageService(), new ProcessHelper());

var files = Directory.GetFiles("./", "*.mp4", SearchOption.TopDirectoryOnly);

var outputDir = "output";

foreach (var file in files)
{
    using var mp4File = new BinaryFileItem { Path = file };

    // Snapshot
    Console.WriteLine($"Creating snapshot of file {Path.GetFileName(file)}");
    try
    {
        using var img = await snapshooter.Snapshot(mp4File, new Regira.Dimensions.Size2D(640, 480), TimeSpan.FromSeconds(.5));
        var outputFilename = Path.Combine(outputDir, $"{Path.GetFileName(file)}.jpeg");
        await img!.SaveAs(outputFilename);
        Console.WriteLine($"Snapshot at {outputFilename}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Snapshot {file} failed: {ex.Message}");
    }

    // Compressing
    Console.WriteLine($"Compressing file {Path.GetFileName(file)}");
    try
    {
        using var compressedFile = await compressor.Compress(mp4File);
        var outputFilename = Path.Combine(outputDir, Path.GetFileName(file));
        Directory.CreateDirectory(outputDir);
        await compressedFile!.SaveAs(outputFilename);
        Console.WriteLine($"Compressed file {Path.GetFileName(file)} at {outputFilename}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Compressing {file} failed: {ex.Message}");
    }
}

Console.WriteLine("Finished");