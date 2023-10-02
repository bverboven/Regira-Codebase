using IO.Testing.Abstractions;
using Microsoft.Extensions.Configuration;
using Regira.IO.Extensions;
using Regira.IO.Storage.SSH;

namespace IO.Testing.SSH;

[TestFixture]
[Ignore("wait for proper config")]
[Parallelizable(ParallelScope.Self)]
public class SshFileProcessorTests : FileProcessorTestsBase
{
    [SetUp]
    public void Setup()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddUserSecrets(GetType().Assembly, true);
        var configuration = configBuilder.Build();
        var sshSection = configuration.GetSection("Storage:SSH");
        var config = new SftpConfig
        {
            ContainerName = sshSection["ContainerName"],
            Host = sshSection["Host"],
            Port = int.Parse(sshSection["Port"]!),
            UserName = sshSection["Username"],
            Password = sshSection["Password"]
        };

        FileService = new SftpService(new SftpCommunicator(config));
        TestFolder = Guid.NewGuid().ToString();
        CreateTestFiles();
        foreach (var file in TestFiles)
        {
            FileService.Save(Path.Combine(TestFolder, file.Identifier!), file.GetBytes()!).Wait();
        }
    }
    [TearDown]
    public void TearDown()
    {
        RemoveTestFiles();
    }
}