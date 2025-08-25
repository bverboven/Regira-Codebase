using System.IO.Compression;
using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Compression;

namespace Regira.ProjectBackupConsole;

public class BackupManager(IFileService fileService, string slnDir)
{
    private readonly string _executingFilename = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName!;
    private string? _outputDir;


    public async Task Process()
    {
        var fso = new FileSearchObject
        {
            Type = FileEntryTypes.Files,
            FolderUri = slnDir,
            Recursive = true
        };

        var outputDir = new DirectoryInfo(_outputDir ?? Environment.CurrentDirectory);
        var outputFilename = Path.Combine(outputDir.FullName, $"{outputDir.Name}-{{DateStamp}}.zip");
        var zipFilename = outputFilename.Replace("{DateStamp}", DateTime.Now.ToString("yyyyMMdd"));

        var files = (await fileService.List(fso))
            .Where(file => !IgnoreFile(file))
            .Select(file => new BinaryFileItem
            {
                FileName = file.Replace('\\', '/').Trim('/'),
                Path = Path.Combine(fileService.Root, file)
            })
            .ToArray();

        var zipStream = new MemoryStream();
        if (await fileService.Exists(zipFilename))
        {
            await using var ts = await fileService.GetStream(zipFilename);
            await ts!.CopyToAsync(zipStream);
        }

        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
        {
            zipArchive.AddFiles(files);
            // !!! stream is updated when disposing zipArchive !!!
        }

        zipStream.Position = 0;
        await fileService.Save(zipFilename, zipStream);
    }

    public BackupManager ToDir(string outputDir)
    {
        _outputDir = outputDir;
        return this;
    }
    public bool IgnoreFile(string path)
    {
        return
            // ignore self
            _executingFilename.EndsWith(path, StringComparison.InvariantCultureIgnoreCase)
            // assets (input)
            //|| path.Contains("\\assets\\input", StringComparison.InvariantCultureIgnoreCase)
            // assets (output)
            //|| path.Contains("\\assets\\output", StringComparison.InvariantCultureIgnoreCase)
            // bin & obj
            || path.Contains("\\bin\\", StringComparison.InvariantCultureIgnoreCase)
            || path.Contains("\\obj\\", StringComparison.InvariantCultureIgnoreCase)
            // node_modules
            || path.Contains("\\node_modules\\", StringComparison.InvariantCultureIgnoreCase)
            // packages
            || path.Contains("\\packages\\", StringComparison.InvariantCultureIgnoreCase)
            // git
            //|| path.StartsWith(".git\\", StringComparison.InvariantCultureIgnoreCase)
            //|| path.Contains("\\.git\\", StringComparison.InvariantCultureIgnoreCase)
            // .vs
            || path.StartsWith(".vs\\", StringComparison.InvariantCultureIgnoreCase)
            || path.Contains("\\.vs\\", StringComparison.InvariantCultureIgnoreCase)
            // .user
            || path.EndsWith(".user", StringComparison.InvariantCultureIgnoreCase);
    }
}