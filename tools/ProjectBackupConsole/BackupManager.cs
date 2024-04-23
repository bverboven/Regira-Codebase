using Regira.IO.Extensions;
using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Compression;
using Regira.IO.Storage.FileSystem;

namespace Regira.ProjectBackupConsole;

public class BackupManager
{
    private readonly IFileService _fileService;
    private readonly ZipBuilder _zipBuilder;
    private readonly string _executingFilename;
    private string? _outputDir;
    public BackupManager(IFileService fileService, ZipBuilder zipBuilder)
    {
        _fileService = fileService;
        _zipBuilder = zipBuilder;
        _executingFilename = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName!;
    }


    public async Task Process()
    {
        var fso = new FileSearchObject
        {
            Type = FileEntryTypes.Files,
            Recursive = true
        };

        var outputDir = new DirectoryInfo(_outputDir ?? Environment.CurrentDirectory);
        var outputFilename = $"{outputDir.Name}-{{DateStamp}}.zip";
        var solutionFile = Directory.GetFiles(outputDir.FullName, "*.sln").FirstOrDefault();
        if (solutionFile != null)
        {
            outputFilename = $"{Path.GetFileNameWithoutExtension(solutionFile)}-{{DateStamp}}.zip";
        }

        var zipFilename = outputFilename
            .Replace("{DateStamp}", DateTime.Now.ToString("yyyyMMdd"));

        var files = (await _fileService.List(fso))
            .Where(file => !IgnoreFile(file))
            .Select(file => new BinaryFileItem
            {
                FileName = file.Replace('\\', '/').Trim('/'),
                Path = Path.Combine(_fileService.RootFolder, file)
            });

        using var zipFile = await _zipBuilder
            .For(files)
            .Build();
        await using var zipStream = zipFile.GetStream();
        await FileSystemUtility.SaveStream(zipFilename, zipStream!);
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
            _executingFilename.EndsWith(path, StringComparison.CurrentCultureIgnoreCase)
            // assets (input)
            || path.Contains("\\assets\\input", StringComparison.CurrentCultureIgnoreCase)
            // assets (output)
            || path.Contains("\\assets\\output", StringComparison.CurrentCultureIgnoreCase)
            // bin & obj
            || path.Contains("\\bin\\", StringComparison.CurrentCultureIgnoreCase)
            || path.Contains("\\obj\\", StringComparison.CurrentCultureIgnoreCase)
            // node_modules
            || path.Contains("node_modules\\", StringComparison.CurrentCultureIgnoreCase)
            // packages
            || path.Contains("packages\\", StringComparison.CurrentCultureIgnoreCase)
            // git
            || path.StartsWith(".git\\", StringComparison.CurrentCultureIgnoreCase)
            || path.Contains("\\.git\\", StringComparison.CurrentCultureIgnoreCase)
            // .vs
            || path.StartsWith(".vs\\", StringComparison.CurrentCultureIgnoreCase)
            || path.Contains("\\.vs\\", StringComparison.CurrentCultureIgnoreCase)
            // .user
            || path.EndsWith(".user", StringComparison.CurrentCultureIgnoreCase);
    }
}