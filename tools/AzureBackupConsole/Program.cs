using AzureBackupConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Regira.System.Hosting.WindowsService;
using Serilog;
using System.Reflection;

// basic logger (configuration and services not loaded yet)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information($"Starting {Assembly.GetExecutingAssembly().GetName().Name}");

    await Run(args);

    Log.Information("Finished");
}
catch (Exception ex)
{
    Log.Error(ex, "Host failed");
}
finally
{
    Console.WriteLine("Press enter to exit");
    Console.ReadLine();

    Log.CloseAndFlush();
}


static Task Run(string[] args)
{
    // configuration
    var host = Host.CreateDefaultBuilder(args)
            .AddConfiguration()
            .AddServices(args)
            .AddSerilog()
            // Windows Service
            .UseWindowsService()
            .Build()
            // create bat-files
            .AddWindowsServiceInstaller();

    // initialization
    using (var scope = host.Services.CreateScope())
    {
        var isRunningAsWindowsService = WindowsServiceHelpers.IsWindowsService();
        var p = scope.ServiceProvider;

        var config = p.GetRequiredService<IConfiguration>();
        var options = p.GetRequiredService<BackupOptions>();
        var logger = p.GetRequiredService<ILogger<Program>>();

        logger.LogInformation($"Watching directory: {config["Backups:SourceDirectory"]!}");
        logger.LogInformation($"Backup container: {options.BackupContainer}");
        logger.LogInformation($"Running as service: {isRunningAsWindowsService}");
    }

    return host.RunAsync();
}