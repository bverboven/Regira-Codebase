using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.FileSystem;
using Regira.ProjectFilesProcessor.Models;
using Regira.ProjectFilesProcessor.Services;
using Regira.System;
using Regira.System.Abstractions;

namespace Regira.ProjectFilesProcessor;

public static class HostExtensions
{
    public static IHostBuilder AddConfiguration(this IHostBuilder builder)
    {
        return builder.ConfigureAppConfiguration((_, config) =>
        {
            config.Sources.Clear();
            // add configuration
            config
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true, true)
#if DEBUG
                .AddUserSecrets(typeof(Program).Assembly, true)
#endif
                ;
        });
    }
    public static IHostBuilder AddServices(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            var config = context.Configuration;

            services
                .Configure<ProjectOptions>(config.GetSection("RegiraPackages"))
                .AddTransient(p => p.GetRequiredService<IOptions<ProjectOptions>>().Value)
                .AddTransient(p =>
                {
                    var options = p.GetRequiredService<ProjectOptions>();
                    return new NuGetHelper.Options
                    {
                        ApiKey = options.ApiKey,
                        PackagesPushUri = $"{options.PackagesUri.TrimEnd('/')}/index.json"
                    };
                })
                .AddTransient<NuGetHelper>()
                .AddTransient<ITextFileService>(p =>
                {
                    var projectOptions = p.GetRequiredService<ProjectOptions>();
                    return new TextFileService(new BinaryFileService.FileServiceOptions
                    {
                        RootFolder = projectOptions.SourceDirectory ?? new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "../../../../../")).FullName
                    });
                })
                .AddTransient<ProjectParser>()
                .AddTransient<ProjectService>()
                .AddTransient<IProcessHelper>(p =>
                {
                    var logger = p.GetRequiredService<ILogger<ProcessHelper>>();
                    return new ProcessHelper(new ProcessHelper.Options
                    {
                        OnOutputDataReceived = (_, e) => { logger.LogInformation($"Data received: {e.Data}"); }
                    });
                })
                .AddTransient<ProjectManager>();

            services
                .AddHttpClient<BaGetService>((p, c) => c.BaseAddress = new Uri(p.GetRequiredService<ProjectOptions>().PackagesUri.TrimEnd('/') + '/'));
        });
    }
}