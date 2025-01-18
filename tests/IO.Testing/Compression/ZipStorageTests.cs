using System.Text;
using IO.Testing.Abstractions;
using Regira.Collections;
using Regira.IO.Abstractions;
using Regira.IO.Models;
using Regira.IO.Storage;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Compression;

namespace IO.Testing.Compression;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ZipStorageTests
{
    protected Guid UniqueIdentifier { get; set; }
    protected IMemoryFile SourceZip { get; set; } = null!;
    protected string SourceFolder => Path.Combine("Assets", "Input", UniqueIdentifier.ToString("D"));
    public IList<BinaryFileItem> Sourcefiles { get; private set; } = null!;
    public ZipFileService FileService { get; private set; } = null!;

    [SetUp]
    public void Setup()
    {
        UniqueIdentifier = Guid.NewGuid();
        Sourcefiles = FileServiceTestUtility.CreateSourceFiles(SourceFolder);
        SourceZip = ZipUtility.Zip(Sourcefiles);
        FileService = new ZipFileService(new ZipFileCommunicator { SourceFile = SourceZip });
    }
    [TearDown]
    public void TearDown()
    {
        FileServiceTestUtility.RemoveSourceFiles(SourceFolder);
        SourceZip.Dispose();
        Sourcefiles.Dispose();
        FileService.Dispose();
    }

    [Test]
    public async Task List()
    {
        var files = await FileService.List();
        Assert.That(files, Is.Not.Empty);
    }
    [Test]
    public async Task Filter_By_Folder()
    {
        var so = new FileSearchObject
        {
            FolderUri = "dir2"
        };
        var files = await FileService.List(so);
        Assert.That(files, Is.Not.Empty);
        var filteredSourceFiles = Sourcefiles.Where(x => x.Identifier!.Contains("dir2\\"));
        Assert.That(files.Count(), Is.EqualTo(filteredSourceFiles.Count()));
    }
    [Test]
    public async Task Filter_By_Extension()
    {
        var so = new FileSearchObject
        {
            Recursive = true,
            Extensions = new[] { ".png", ".txt", ".sql" }
        };
        var files = await FileService.List(so);
        Assert.That(files, Is.Not.Empty);
        foreach (var file in files)
        {
            var pathWithoutQuery = file.Split('?').First();
            Assert.That(so.Extensions.Any(e => pathWithoutQuery.EndsWith(e, StringComparison.CurrentCultureIgnoreCase)), Is.True);
        }
    }
    [Test]
    public async Task GetBytes()
    {
        var fso = new FileSearchObject
        {
            Type = FileEntryTypes.Files,
            FolderUri = "dir2"
        };
        var files = await FileService.List(fso);
        Assert.That(files, Is.Not.Empty);
        foreach (var file in files.Take(1))
        {
            var bytes = await FileService.GetBytes(file);
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes!.Length > 0, Is.True);
            var sourceFileName = Path.GetFileName(file);
            var sourceFile = Sourcefiles.Single(x => sourceFileName.Equals(x.FileName, StringComparison.InvariantCultureIgnoreCase));
            Assert.That(bytes.Length, Is.EqualTo(sourceFile.Length));
        }
    }

    [Test]
    public async Task Add_File()
    {
        var newIdentifier = "dir2/dir2.2/file2.2.3.txt";
        var newBytes = Encoding.UTF8.GetBytes(newIdentifier);// File Identifier used as it's content
        var savedUri = await FileService.Save(newIdentifier, newBytes);
        Assert.That(savedUri, Is.Not.Empty);
        var savedBytes = await FileService.GetBytes(newIdentifier);
        Assert.That(savedBytes, Is.EquivalentTo(newBytes));
    }
    [Test]
    public async Task Update_File()
    {
        var identifier = Sourcefiles.Skip(5).First().Identifier!;
        var sourceBytes = await FileService.GetBytes(identifier);
        var updatedBytes = Encoding.UTF8.GetBytes($"{identifier} updated");
        await FileService.Save(identifier, updatedBytes);
        var savedBytes = await FileService.GetBytes(identifier);
        Assert.That(savedBytes, Is.Not.EquivalentTo(sourceBytes!));
        Assert.That(savedBytes, Is.EquivalentTo(updatedBytes));
    }
    [Test]
    public async Task Remove_File()
    {
        var identifier = Sourcefiles.Skip(5).First().Identifier!;
        var exists = await FileService.Exists(identifier);
        Assert.That(exists, Is.True);

        await FileService.Delete(identifier);
        var stillExists = await FileService.Exists(identifier);
        Assert.That(stillExists, Is.False);
    }
}