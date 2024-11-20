using IO.Testing.Abstractions;
using Regira.IO.Storage.FileSystem;
using static Regira.IO.Storage.FileSystem.BinaryFileService;

namespace IO.Testing.FileSystem;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class FsFileProcessorTests : FileProcessorTestsBase
{
    [SetUp]
    public void Setup()
    {
        TestFolder = Guid.NewGuid().ToString();
        CreateTestFiles();
        FileService = new BinaryFileService(new FileServiceOptions { RootFolder = AssetsDir });
    }
    [TearDown]
    public void TearDown()
    {
        RemoveTestFiles();
    }
}