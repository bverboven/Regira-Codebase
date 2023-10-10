using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regira.IO.Models;

namespace Regira.AzureBackupService;

public class Worker : BackgroundService
{
    private readonly FileWatcher _watcher;
    private readonly BackupManager _backupManager;
    private readonly ILogger<Worker> _logger;
    public Worker(FileWatcher watcher, BackupManager backupManager, ILogger<Worker> logger)
    {
        _watcher = watcher;
        _backupManager = backupManager;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _watcher.Start();
        return Task.CompletedTask;
    }

    private void Watcher_OnScanned(string file)
    {
        try
        {
            _logger.LogInformation($"Processing file {file}");
            var fileItem = new BinaryFileItem(file);
            _backupManager.Backup(fileItem).Wait();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Processing file {file} failed");
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher.OnFileCreated += Watcher_OnScanned;
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher.Stop();
        return base.StopAsync(cancellationToken);
    }
}