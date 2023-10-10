using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Storage.Abstractions;

namespace Regira.AzureBackupService;

public class BackupManager
{
    private readonly IFileService _azureService;
    public BackupManager(IFileService azureService)
    {
        _azureService = azureService;
    }


    public async Task<string> Backup(INamedFile file)
    {
        var bytes = file.GetBytes() ?? (file as IBinaryFile)?.GetBytes();
        if (bytes?.Any() != true)
        {
            throw new Exception($"Could not get contents of {file}");
        }

        var uri = await _azureService.Save(file.FileName!, bytes, file.ContentType);
        return uri;
    }
}