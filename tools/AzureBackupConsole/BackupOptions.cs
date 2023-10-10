namespace Regira.AzureBackupService;

public class BackupOptions
{
    public string AzureConnectionString { get; set; } = null!;
    public string BackupContainer { get; set; } = "backups";
}