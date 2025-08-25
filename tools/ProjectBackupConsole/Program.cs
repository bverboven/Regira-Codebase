using Regira.IO.Storage.FileSystem;
using Regira.ProjectBackupConsole;

var rootIndex = Array.IndexOf(args, "-root");
var srcDir = Environment.CurrentDirectory;
if (rootIndex != -1 && rootIndex + 1 < args.Length)
{
    srcDir = args[rootIndex + 1];
}
var outputDir = srcDir;
var outputDirIndex = Array.IndexOf(args, "-output-dir");
if (outputDirIndex != -1 && outputDirIndex + 1 < args.Length)
{
    outputDir = args[outputDirIndex + 1];
}

var slnFiles = Directory.GetFiles(srcDir, "*.sln", SearchOption.AllDirectories);
var pkgJsonFiles = Directory.GetFiles(srcDir, "*.json", SearchOption.AllDirectories)
    .Where(f => Path.GetFileNameWithoutExtension(f) == "package");
var backupFolders = slnFiles.Concat(pkgJsonFiles)
    .Select(f => Path.GetDirectoryName(f)!)
    .Distinct()
    .OrderBy(x => x)
    .ToArray();

var fileService = new BinaryFileService(new FileSystemOptions { RootFolder = srcDir });
foreach (var backupFolder in backupFolders)
{
    Console.WriteLine($"Zipping {backupFolder}");

    var bm = new BackupManager(fileService, backupFolder);
    await bm
        .ToDir(outputDir)
        .Process();
}


Console.WriteLine("Finished");