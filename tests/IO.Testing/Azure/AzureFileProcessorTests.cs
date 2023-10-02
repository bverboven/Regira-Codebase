using IO.Testing.Abstractions;
using Microsoft.Extensions.Configuration;
using Regira.IO.Extensions;
using Regira.IO.Storage.Azure;

namespace IO.Testing.Azure;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class AzureFileProcessorTests : FileProcessorTestsBase
{
    private const string BLOB_CONTAINER = "test-container";
    private BinaryBlobService _blobService = null!;
    [SetUp]
    public void Setup()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddUserSecrets(typeof(AzureStorageTests).Assembly, true);
        var configuration = configBuilder.Build();
        var azureConnectionString = configuration["Storage:Azure:ConnectionString"];

        TestFolder = Guid.NewGuid().ToString();
        CreateTestFiles();

        var cf = new AzureConfig
        {
            ConnectionString = azureConnectionString,
            ContainerName = BLOB_CONTAINER
        };
        var cm = new AzureCommunicator(cf);
        _blobService = new BinaryBlobService(cm);
        foreach (var file in TestFiles)
        {
            _blobService.Save(Path.Combine(TestFolder, file.Identifier!), file.GetBytes()!).Wait();
        }
        FileService = _blobService;
    }
    [TearDown]
    public void TearDown()
    {
        foreach (var file in TestFiles)
        {
            _blobService.Delete(Path.Combine(TestFolder, file.Identifier!)).Wait();
        }
        RemoveTestFiles();
    }
}