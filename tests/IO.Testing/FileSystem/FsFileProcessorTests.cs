using IO.Testing.Abstractions;
using Regira.IO.Storage.FileSystem;

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
        FileService = new BinaryFileService(AssetsDir);
    }
    [TearDown]
    public void TearDown()
    {
        RemoveTestFiles();
    }
}