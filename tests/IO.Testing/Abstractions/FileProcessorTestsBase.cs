using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Helpers;
using Regira.Utilities;
[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace IO.Testing.Abstractions;

public abstract class FileProcessorTestsBase
{
    protected readonly string AssetsDir;
    protected string TestFolder;
    protected BinaryFileItem[] TestFiles;
    protected IFileService FileService;

    protected FileProcessorTestsBase()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        AssetsDir = new DirectoryInfo(Path.Combine(assemblyDir, "../../../", "assets")).FullName;
    }


    [Test]
    public void Recursive_Directories()
    {
        var fileProcessor = new FileProcessor(FileService);
        var filesWithRecursiveSearchObject = new List<string>();
        fileProcessor
            .ProcessFiles(new FileSearchObject
            {
                FolderUri = TestFolder,
                Recursive = true
            }, async (f, _) => { filesWithRecursiveSearchObject.Add(f); }, false);

        var filesFromRecursiveDirectories = new List<string>();
        fileProcessor
            .ProcessFiles(new FileSearchObject
            {
                FolderUri = TestFolder
            }, async (f, _) => { filesFromRecursiveDirectories.Add(f); }, true);
        //var str = $"new [] {{{string.Join($",{Environment.NewLine}", filesFromRecursiveDirectories.Select(x => $@"@""{x}"""))}}}";
        CollectionAssert.AreEquivalent(filesWithRecursiveSearchObject, filesFromRecursiveDirectories);
    }
    [Test]
    public void Recursive_DirectoriesAsync()
    {
        var fileProcessor = new FileProcessor(FileService);
        var filesWithRecursiveSearchObject = new List<string>();
        fileProcessor
            .ProcessFiles(new FileSearchObject
            {
                FolderUri = TestFolder,
                Recursive = true
            }, async (f, s) => { filesWithRecursiveSearchObject.Add(f); }, false)
            .Wait();

        var filesFromRecursiveDirectories = new List<string>();
        fileProcessor
            .ProcessFiles(new FileSearchObject
            {
                FolderUri = TestFolder
            }, async (f, s) => { filesFromRecursiveDirectories.Add(f); }, true)
            .Wait();
        //var str = $"new [] {{{string.Join($",{Environment.NewLine}", filesFromRecursiveDirectories.Select(x => $@"@""{x}"""))}}}";
        CollectionAssert.AreEquivalent(filesWithRecursiveSearchObject, filesFromRecursiveDirectories);
    }


    public void CreateTestFiles()
    {
        TestFiles = TestFilesCreator.Create(Path.Combine(AssetsDir, TestFolder))
            .ToArray();
    }
    public void RemoveTestFiles()
    {
        Directory.Delete(Path.Combine(AssetsDir, TestFolder), true);
    }
}