using ProjectBackupConsole;
using Regira.IO.Storage.Compression;
using Regira.IO.Storage.FileSystem;

var rootIndex = Array.IndexOf(args, "-root");
var rootPath = Environment.CurrentDirectory;
if (rootIndex != -1 && rootIndex + 1 < args.Length)
{
    rootPath = args[rootIndex + 1];
}
var outputDir = rootPath;
var outputDirIndex = Array.IndexOf(args, "-output-dir");
if (outputDirIndex != -1 && outputDirIndex + 1 < args.Length)
{
    outputDir = args[outputDirIndex + 1];
}

var rootDir = new DirectoryInfo(rootPath);

var fileService = new BinaryFileService(rootDir.FullName);
var zipBuilder = new ZipBuilder();

Console.WriteLine($"Zipping {rootDir.FullName}");

var bm = new BackupManager(fileService, zipBuilder);
await bm
    .ToDir(outputDir)
    .Process();

Console.WriteLine("Finished");