using IO.Testing.Helpers;
using Microsoft.Extensions.Configuration;
using Regira.IO.Extensions;
using Regira.IO.Storage.Azure;

namespace IO.Testing.Azure;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class AzureStorageTests
{
    public StorageTestHelper.StorageTestContext<BinaryBlobService> StorageTestContext { get; set; }
    [SetUp]
    public async Task Setup()
    {
        StorageTestContext = StorageTestHelper.CreateDecoratedFileService((_, _) =>
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets(typeof(AzureStorageTests).Assembly, true);
            var configuration = configBuilder.Build();
            var azureConnectionString = configuration["Storage:Azure:ConnectionString"];
            var cf = new AzureOptions
            {
                ConnectionString = azureConnectionString,
                ContainerName = "test-container"
            };
            var cm = new AzureCommunicator(cf);
            return new BinaryBlobService(cm);
        });
        // create initial files on Azure
        foreach (var file in StorageTestContext.SourceFiles)
        {
            await StorageTestContext.FileService.Save(file.Identifier!, file.GetBytes()!, file.ContentType);
        }
    }

    [TearDown]
    public async Task TearDown() => await StorageTestContext.DisposeAsync();

    [Test]
    public async Task List() => await StorageTestContext.Test_List();
    [Test]
    public async Task GetBytes() => await StorageTestContext.Test_GetBytes();
    [Test]
    public async Task Filter_By_Folder() => await StorageTestContext.Test_Filter_By_Folder();
    [Test]
    public async Task Filter_By_Extension() => await StorageTestContext.Test_Filter_By_Extension();
    [Test]
    public async Task Filter_Recursive() => await StorageTestContext.Test_Filter_Recursive();
    [Test]
    public async Task Filter_By_EntryType() => await StorageTestContext.Test_Filter_By_EntryType();

    [Test]
    public async Task Add_File() => await StorageTestContext.Test_Add_File();
    [Test]
    public async Task Update_File() => await StorageTestContext.Test_Update_File();
    [Test]
    public async Task Remove_File() => await StorageTestContext.Test_Remove_File();
}