using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Helpers;
using Regira.Utilities;
[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace IO.Testing.Abstractions;

public abstract class FileProcessorTestsBase
{
    protected readonly string AssetsDir = null!;
    protected string TestFolder = null!;
    protected BinaryFileItem[] TestFiles = null!;
    protected IFileService FileService = null!;

    protected FileProcessorTestsBase()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        AssetsDir = new DirectoryInfo(Path.Combine(assemblyDir, "../../../", "assets")).FullName;
    }


    [Test]
    public async Task Recursive_Directories()
    {
        var fileProcessor = new FileProcessor(FileService);
        var filesWithRecursiveSearchObject = new List<string>();
        await fileProcessor
              .ProcessFiles(new FileSearchObject
              {
                  FolderUri = TestFolder,
                  Recursive = true
              }, (f, _) => { filesWithRecursiveSearchObject.Add(f); return Task.CompletedTask; }, false);

        var filesFromRecursiveDirectories = new List<string>();
        await fileProcessor
              .ProcessFiles(new FileSearchObject
              {
                  FolderUri = TestFolder
              }, (f, _) => { filesFromRecursiveDirectories.Add(f); return Task.CompletedTask; }, true);
        //var str = $"new [] {{{string.Join($",{Environment.NewLine}", filesFromRecursiveDirectories.Select(x => $@"@""{x}"""))}}}";
        Assert.That(filesFromRecursiveDirectories, Is.EquivalentTo(filesWithRecursiveSearchObject));
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
            }, (f, s) => { filesWithRecursiveSearchObject.Add(f); return Task.CompletedTask; }, false)
            .Wait();

        var filesFromRecursiveDirectories = new List<string>();
        fileProcessor
            .ProcessFiles(new FileSearchObject
            {
                FolderUri = TestFolder
            }, (f, s) => { filesFromRecursiveDirectories.Add(f); return Task.CompletedTask; }, true)
            .Wait();
        //var str = $"new [] {{{string.Join($",{Environment.NewLine}", filesFromRecursiveDirectories.Select(x => $@"@""{x}"""))}}}";
        Assert.That(filesFromRecursiveDirectories, Is.EquivalentTo(filesWithRecursiveSearchObject));
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