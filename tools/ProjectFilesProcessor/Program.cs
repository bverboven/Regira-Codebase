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
using Serilog;
using Serilog.Events;
using System.Reflection;

// basic logger (configuration and services not loaded yet)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // configuration
    var host = CreateHostBuilder(args)
        .UseSerilog(ConfigureSerilog)
        .Build();

    // initialization
    using var scope = host.Services.CreateScope();
    var p = scope.ServiceProvider;

    var logger = p.GetRequiredService<ILogger<Program>>();
    var options = p.GetRequiredService<ProjectOptions>();
    var pm = p.GetRequiredService<ProjectManager>();

    logger.LogInformation("Start processing projectfiles");
    await pm.Init();
    
    pm.CheckAndUpdateVersions();

    foreach (var projectNode in pm.ProjectTree!)
    {
        logger.LogInformation($"{string.Empty.PadLeft(projectNode.Level * 2, '.')}{projectNode.Value.Id}"
                              + $" {(projectNode.Value.Version > projectNode.Value.PublishedVersion ? $"(v{projectNode.Value.PublishedVersion} -> v{projectNode.Value.Version})" : $"(v{projectNode.Value.Version})")}");
    }

    Console.WriteLine();
    Console.WriteLine("Press enter to start updating .csproj files");
    Console.ReadLine();

    // update project files
    await pm.SaveProjectFiles();
    Console.WriteLine();

    var pendingProjects = pm.GetPendingProjects();
    if (pendingProjects.Any())
    {
        Console.WriteLine($"Push {pendingProjects.Count} NuGet packages? (Y/N)");
        var pushNuGet = "Y".Equals(Console.ReadLine(), StringComparison.InvariantCultureIgnoreCase);
        if (pushNuGet)
        {
            pm.PushPending();
        }
    }
    else
    {
        logger.LogInformation("No projects to publish");
    }

    // update nuget-push file
    var commands = pm.GetBatchCommand();
    var nuGetPushPath = Path.Combine(options.SourceDirectory, "nuget-push.txt");
    await File.WriteAllLinesAsync(nuGetPushPath, commands);

    Console.WriteLine();
    logger.LogInformation("Finished processing projectfiles");
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

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(SetupConfig)
        .ConfigureServices((ctx, services) => ConfigureServices(ctx, services, args));
}
static void SetupConfig(HostBuilderContext context, IConfigurationBuilder builder)
{
    builder.Sources.Clear();
    // add configuration
    builder
        .AddJsonFile("appsettings.json", true, true)
        .AddUserSecrets(typeof(Program).Assembly, true);
}
static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration configuration)
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File($"logs/{Assembly.GetExecutingAssembly().GetName().Name}.log", LogEventLevel.Warning, rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true, retainedFileCountLimit: 7);
}
static void ConfigureServices(HostBuilderContext context, IServiceCollection services, params string[] args)
{
    var config = context.Configuration;

    services
        .AddHttpClient<BaGetService>(c => c.BaseAddress = new Uri(config["RegiraPackages:PackagesUri"]!));

    services
        .Configure<ProjectOptions>(config.GetSection("RegiraPackages"))
        .AddTransient(p =>
        {
            var options = p.GetRequiredService<IOptions<ProjectOptions>>().Value;
            ProcessOptions(options, args);
            return options;
        })
        .AddTransient(p =>
        {
            var options = p.GetRequiredService<ProjectOptions>();
            return new NuGetHelper(new NuGetHelper.Options { PackagesPushUri = $"{options.PackagesUri}index.json" });
        })
        .AddTransient<ITextFileService>(p =>
        {
            var options = p.GetRequiredService<ProjectOptions>();
            return new TextFileService(options.SourceDirectory);
        })
        .AddTransient<ProjectParser>()
        .AddTransient<ProjectService>()
        .AddTransient<IProcessHelper>(p =>
        {
            var logger = p.GetRequiredService<ILogger<ProcessHelper>>();
            return new ProcessHelper(new ProcessHelper.Options
            {
                OnOutputDataReceived = (_, e) =>
                {
                    logger.LogInformation($"Data received: {e.Data}");
                }
            });
        })
        .AddTransient<ProjectManager>();
}
static void ProcessOptions(ProjectOptions options, params string[] args)
{
    var rootIndex = Array.IndexOf(args, "-root");
    var rootPath = Path.Combine(AppContext.BaseDirectory, "../../../../../");
    if (rootIndex != -1 && rootIndex + 1 < args.Length)
    {
        rootPath = args[rootIndex + 1];
    }
    var root = new DirectoryInfo(rootPath);
    options.SourceDirectory = root.FullName;
}