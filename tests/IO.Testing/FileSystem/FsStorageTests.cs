using IO.Testing.Abstractions;
using Regira.IO.Storage.FileSystem;
using static Regira.IO.Storage.FileSystem.BinaryFileService;

namespace IO.Testing.FileSystem;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class FsStorageTests : FileServiceTestsBase
{
    [SetUp]
    public void Setup()
    {
        Testfiles = GetTestFiles(SourceFolder).ToArray();
        FileService = new BinaryFileService(new FileServiceOptions { RootFolder = AssetsDir });
    }
    [TearDown]
    public void TearDown()
    {
        RemoveTestFiles(SourceFolder);
        ClearTestFolder(TestFolder);
    }


    protected override string GetFullIdentifier(string folder, string identifier)
    {
        return FileNameUtility.Combine(folder, identifier);
    }
    protected override string GetCleanFileName(string filename)
    {
        return FileNameUtility.GetCleanFileName(filename);
    }
}