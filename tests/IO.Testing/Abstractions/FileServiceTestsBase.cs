using NUnit.Framework.Legacy;
using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.Utilities;

namespace IO.Testing.Abstractions;

public abstract class FileServiceTestsBase
{
    protected IFileService FileService = null!;
    protected readonly string AssetsDir;
    protected string TestFolder => $"testing/{UniqueIdentifier}";
    protected string SourceFolder => Path.Combine(AssetsDir, "source", UniqueIdentifier);
    protected BinaryFileItem[] Testfiles = null!;
    protected string UniqueIdentifier { get; init; }

    protected FileServiceTestsBase()
    {
        var assemblyDir = AssemblyUtility.GetAssemblyDirectory()!;
        AssetsDir = new DirectoryInfo(Path.Combine(assemblyDir, "../../../", "Assets")).FullName;
        UniqueIdentifier = Guid.NewGuid().ToString();
    }

    [Test]
    public virtual void Expect_Empty_Folder()
    {
        var files = GetStorageFiles(TestFolder);
        CollectionAssert.IsEmpty(files);
    }
    [Test]
    public virtual void Create_Files()
    {
        var savedFiles = SaveTestFiles(TestFolder);
        ClassicAssert.AreEqual(Testfiles.Length, savedFiles.Length);
        var files = GetStorageFiles(TestFolder);
        ClassicAssert.AreEqual(Testfiles.Length, files.Count());
        foreach (var file in Testfiles)
        {
            var identifier = GetFullIdentifier(TestFolder, file.Identifier!);
            ClassicAssert.IsTrue(FileService.Exists(identifier).Result);
        }
    }
    [Test]
    public virtual void Update_Files()
    {
        var createdFiles = SaveTestFiles(TestFolder);
        ClassicAssert.AreEqual(Testfiles.Length, createdFiles.Length);
        // save again (update)
        var updatedFiles = SaveTestFiles(TestFolder);
        ClassicAssert.AreEqual(createdFiles.Length, updatedFiles.Length);
        var files = GetStorageFiles(TestFolder);
        ClassicAssert.AreEqual(Testfiles.Length, files.Count());
        foreach (var file in Testfiles)
        {
            var identifier = GetFullIdentifier(TestFolder, file.Identifier!);
            ClassicAssert.IsTrue(FileService.Exists(identifier).Result);
        }
    }
    [Test]
    public virtual void Remove_Files()
    {
        var savedFiles = SaveTestFiles(TestFolder);
        var files1 = GetStorageFiles(TestFolder);
        CollectionAssert.IsNotEmpty(files1);
        foreach (var file in savedFiles)
        {
            FileService.Delete(file).Wait();
        }
        var files2 = GetStorageFiles(TestFolder);
        CollectionAssert.IsEmpty(files2);
    }
    [Test]
    public virtual void Check_Bytes()
    {
        SaveTestFiles(TestFolder);
        foreach (var file in Testfiles)
        {
            var testBytes = file.Bytes;
            var identifier = GetFullIdentifier(TestFolder, file.Identifier!);
            var blobBytes = FileService.GetBytes(identifier).Result;
            CollectionAssert.AreEqual(testBytes, blobBytes);
        }
    }
    [Test]
    public virtual async Task Filter_By_Folder()
    {
        SaveTestFiles(TestFolder);
        var so = new FileSearchObject
        {
            FolderUri = $@"{TestFolder}/dir2/dir2.2"
        };
        var files = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(files);
        var sourceFiles = Directory.GetFiles(Path.Combine(SourceFolder, "dir2", "dir2.2"));
        CollectionAssert.AreEquivalent(sourceFiles.Select(Path.GetFileName), files.Select(Path.GetFileName));
    }
    [Test]
    public virtual async Task Filter_Recursive()
    {
        SaveTestFiles(TestFolder);
        var so = new FileSearchObject
        {
            FolderUri = TestFolder,
            Recursive = true,
        };
        var files = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(files);
        var sourceFiles = Directory.GetFiles(SourceFolder, "", SearchOption.AllDirectories);
        var sourceFolders = Directory.GetDirectories(SourceFolder, "", SearchOption.AllDirectories);
        ClassicAssert.AreEqual(sourceFiles.Concat(sourceFolders).Count(), files.Count);
        CollectionAssert.AreEquivalent(sourceFiles.Concat(sourceFolders).Select(Path.GetFileName), files.Select(Path.GetFileName));
    }
    [Test]
    public virtual async Task Filter_By_EntryType()
    {
        SaveTestFiles(TestFolder);
        // files
        var so = new FileSearchObject
        {
            FolderUri = TestFolder,
            Recursive = true,
            Type = FileEntryTypes.Files
        };
        var files = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(files);
        // folders
        so = new FileSearchObject
        {
            FolderUri = TestFolder,
            Recursive = true,
            Type = FileEntryTypes.Directories
        };
        var folders = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(folders);
        // both
        so = new FileSearchObject
        {
            FolderUri = TestFolder,
            Recursive = true,
            Type = FileEntryTypes.All
        };
        var both = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(both);
        CollectionAssert.AreEquivalent(folders.Concat(files), both);
    }
    [Test]
    public virtual async Task Filter_By_Extension()
    {
        SaveTestFiles(TestFolder);
        var so = new FileSearchObject
        {
            FolderUri = TestFolder,
            Recursive = true,
            Extensions = new[] { ".log", ".text" }
        };
        var files = (await FileService.List(so)).AsList();
        CollectionAssert.IsNotEmpty(files);
        var sourceFiles = Directory.GetFiles(SourceFolder, "", SearchOption.AllDirectories).Where(f => so.Extensions.Any(f.EndsWith)).ToArray();
        var sourceFolders = Directory.GetDirectories(SourceFolder, "", SearchOption.AllDirectories)
            .Where(d => sourceFiles.Any(f => f.StartsWith(d)))
            .Distinct();
        CollectionAssert.AreEquivalent(sourceFolders.Concat(sourceFiles).Select(Path.GetFileName), files.Select(Path.GetFileName));
    }


    protected BinaryFileItem[] GetTestFiles(string folder)
    {
        return TestFilesCreator.Create(folder)
            .ToArray();
    }
    public void RemoveTestFiles(string folder)
    {
        Directory.Delete(folder, true);
    }
    protected string[] GetStorageFiles(string folder)
    {
        return FileService
            .List(new FileSearchObject
            {
                FolderUri = folder,
                Recursive = true,
                Type = FileEntryTypes.Files
            })
            .Result
            .ToArray();
    }
    protected string[] SaveTestFiles(string folder)
    {
        var savedFiles = new List<string>();
        foreach (var file in Testfiles)
        {
            var identifier = GetFullIdentifier(folder, file.Identifier!);
            var savedUri = FileService.Save(identifier, File.ReadAllBytes(file.Path!)).Result;
            savedFiles.Add(savedUri);
        }
        return savedFiles.ToArray();
    }
    protected void ClearTestFolder(string folder)
    {
        var files = GetStorageFiles(folder);
        foreach (var file in files)
        {
            FileService.Delete(file).Wait();
        }
        FileService.Delete(folder).Wait();
    }
    protected string GetFileIdentifier(string path)
    {
        return path.Substring(AssetsDir.Length).TrimStart("\\/".ToCharArray());
    }
    protected abstract string GetFullIdentifier(string folder, string identifier);
    protected abstract string GetCleanFileName(string filename);
}