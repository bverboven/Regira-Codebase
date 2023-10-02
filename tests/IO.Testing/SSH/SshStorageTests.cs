using IO.Testing.Abstractions;
using Microsoft.Extensions.Configuration;
using Regira.IO.Storage.SSH;
using FileNameUtility = Regira.IO.Storage.SSH.FileNameUtility;

namespace IO.Testing.SSH;

[TestFixture]
[Ignore("wait for proper config")]
[Parallelizable(ParallelScope.Self)]
public class SshStorageTests : FileServiceTestsBase
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
            
        Testfiles = GetTestFiles(SourceFolder).ToArray();
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