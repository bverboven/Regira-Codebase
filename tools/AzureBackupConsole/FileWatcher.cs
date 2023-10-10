using Regira.IO.Storage.FileSystem;

namespace Regira.AzureBackupService;

public class FileWatcher : IDisposable
{
    public class Options
    {
        public string? Filter { get; set; }
        public string SourceDirectory { get; set; } = null!;
    }

    private readonly FileSystemWatcher _watcher;
    public event Action<string>? OnFileCreated;
    public FileWatcher(Options options)
    {
        if (string.IsNullOrWhiteSpace(options.SourceDirectory))
        {
            throw new ArgumentNullException(nameof(options.SourceDirectory));
        }

        _watcher = new FileSystemWatcher(options.SourceDirectory);

        _watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        _watcher.Filter = options.Filter ?? "*.*";
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;
    }


    public void Start()
    {
        _watcher.Created += OnCreated;
    }
    public void Stop()
    {
        _watcher.Created -= OnCreated;
    }


    void OnCreated(object sender, FileSystemEventArgs e)
    {
        _ = HandleFileCreated(e.FullPath);
    }

    async Task HandleFileCreated(string path, int attempt = 1)
    {
        if (attempt <= 10 && FileSystemUtility.IsFileLocked(path, FileAccess.ReadWrite))
        {
            await Task.Delay(attempt * 2500);
            await HandleFileCreated(path, attempt + 1);
            return;
        }

        OnFileCreated?.Invoke(path);
    }


    public void Dispose()
    {
        _watcher.Dispose();
    }
}