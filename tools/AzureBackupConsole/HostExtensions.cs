using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Regira.IO.Storage.Azure;
using Serilog;
using Serilog.Events;

namespace Regira.AzureBackupService;

public static class HostExtensions
{
    public static IHostBuilder AddConfiguration(this IHostBuilder builder)
    {
        return builder.ConfigureAppConfiguration((_, config) =>
        {
            config.Sources.Clear();
            // add configuration
            config
                .AddJsonFile("appsettings.json", true, true)
#if DEBUG
                .AddUserSecrets(typeof(Program).Assembly, true)
#endif
                ;
        });
    }
    public static IHostBuilder AddServices(this IHostBuilder builder, params string[] args)
    {
        return builder.ConfigureServices((ctx, services) => ConfigureServices(ctx, services, args));
    }
    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"logs/{Assembly.GetExecutingAssembly().GetName().Name}.log", LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true, retainedFileCountLimit: 7);
        });
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services, params string[] args)
    {
        var _ = args;
        var config = context.Configuration;
        services.Configure<AzureConfig>(config.GetSection("Storage:Azure"))
            .AddTransient(p =>
            {
                var azureConfig = p.GetRequiredService<IOptions<AzureConfig>>().Value;
                return new BackupOptions
                {
                    AzureConnectionString = azureConfig.ConnectionString!
                };
            })
            .AddTransient(p =>
            {
                var options = p.GetRequiredService<BackupOptions>();
                var azureConfig = new AzureConfig
                {
                    ConnectionString = options.AzureConnectionString,
                    ContainerName = options.BackupContainer
                };
                return new AzureCommunicator(azureConfig);
            })
            .AddTransient(_ =>
            {
                var options = new FileWatcher.Options
                {
                    Filter = null,
                    SourceDirectory = config["Backups:SourceDirectory"]!
                };
                return new FileWatcher(options);
            })
            .AddTransient(p =>
            {
                var azureCommunicator = p.GetRequiredService<AzureCommunicator>();
                var fileService = new BinaryBlobService(azureCommunicator);
                return new BackupManager(fileService);
            })
            .AddHostedService<Worker>();
    }
}