using Azure.Storage.Blobs;

namespace Regira.IO.Storage.Azure;

public class AzureCommunicator(AzureOptions config)
{
    public bool IsOpened { get; set; }

    internal BlobContainerClient? Container { get; set; }

    private readonly string _containerName = config.ContainerName ?? throw new ArgumentNullException(nameof(config.ContainerName));
    private readonly string _connectionString = config.ConnectionString ?? throw new ArgumentNullException(nameof(config.ConnectionString));


    public async Task Open()
    {
        if (IsOpened)
        {
            return;
        }

        Container = new BlobContainerClient(_connectionString, _containerName);
        if (!await Container.ExistsAsync())
        {
            await Container.CreateAsync();
        }

        IsOpened = true;
    }
}