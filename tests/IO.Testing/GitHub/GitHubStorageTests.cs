using IO.Testing.Helpers;
using Microsoft.Extensions.Configuration;
using Regira.IO.Extensions;
using Regira.IO.Storage.GitHub;
using Regira.Serializing.Newtonsoft.Json;
using System.Net;
namespace IO.Testing.GitHub;

[TestFixture]
public class GitHubStorageTests
{
    public StorageTestHelper.StorageTestContext<GitHubService> StorageTestContext { get; set; } = null!;
    private GitHubCommunicator _communicator = null!;

    [SetUp]
    public async Task Setup()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(GitHubStorageTests).Assembly, true)
            .Build();

        _communicator = new GitHubCommunicator(new GitHubOptions
        {
            Uri = config["Storage:GitHub:Uri"]!,
            Key = config["Storage:GitHub:Key"],
            Branch = config["Storage:GitHub:Branch"] ?? "main",
            ContentPath = $"test-write/{Guid.NewGuid():D}"
        });

        StorageTestContext = StorageTestHelper.CreateDecoratedFileService((_, _) =>
            new GitHubService(_communicator, new JsonSerializer()));

        foreach (var file in StorageTestContext.SourceFiles)
        {
            await StorageTestContext.FileService.Save(file.Identifier!, file.GetBytes()!, file.ContentType);
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            await StorageTestContext.DisposeAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests or HttpStatusCode.NotFound)
        {
            // GitHub secondary rate limit hit during remote cleanup.
            // Local folder and file handles were already released before the remote call threw.
            // The unique GUID test folder is orphaned in the repo but is harmless.
        }
        finally
        {
            _communicator.Dispose();
        }
    }

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
