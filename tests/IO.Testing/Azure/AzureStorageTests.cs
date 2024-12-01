using IO.Testing.Abstractions;
using Microsoft.Extensions.Configuration;
using Regira.IO.Storage.Azure;
using Regira.IO.Storage.Azure.Utilities;

namespace IO.Testing.Azure;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class AzureStorageTests : FileServiceTestsBase
{
    private const string BLOB_CONTAINER = "test-container";
    [SetUp]
    public void Setup()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddUserSecrets(typeof(AzureStorageTests).Assembly, true);
        var configuration = configBuilder.Build();
        var azureConnectionString = configuration["Storage:Azure:ConnectionString"];
            
        Testfiles = GetTestFiles(SourceFolder).ToArray();
        var cf = new AzureOptions
        {
            ConnectionString = azureConnectionString,
            ContainerName = BLOB_CONTAINER
        };
        var cm = new AzureCommunicator(cf);
        FileService = new BinaryBlobService(cm);
        //ClearTestFolder("XXX");
    }
    [TearDown]
    public void TearDown()
    {
        ClearTestFolder(TestFolder);
        RemoveTestFiles(SourceFolder);
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