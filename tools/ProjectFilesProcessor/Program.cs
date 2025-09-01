using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regira.ProjectFilesProcessor;
using Regira.ProjectFilesProcessor.Services;
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
    var host = Host.CreateDefaultBuilder(args)
        .AddConfiguration()
        .AddServices()
        .UseSerilog(ConfigureSerilog)
        .Build();

    // initialization
    using var scope = host.Services.CreateScope();
    var p = scope.ServiceProvider;

    var logger = p.GetRequiredService<ILogger<Program>>();
    var pm = p.GetRequiredService<ProjectManager>();

    logger.LogInformation("Start processing projectfiles");
    await pm.Init();

    pm.CheckAndUpdateVersions();

    foreach (var projectNode in pm.ProjectTree!)
    {
        logger.LogInformation($"{string.Empty.PadLeft(projectNode.Level * 2, '.')}{projectNode.Value.Id}"
                              + $" {(projectNode.Value.Version > projectNode.Value.CurrentVersion ? $"(v{projectNode.Value.CurrentVersion} -> v{projectNode.Value.Version})" : $"(v{projectNode.Value.Version})")}");
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
        var pendingTree = pm.ProjectTree.FindAll(pn => pendingProjects.Any(pp => pn.Value.Id == pp.Id));
        foreach (var projectNode in pendingTree)
        {
            logger.LogInformation($"{string.Empty.PadLeft(projectNode.Level * 2, '.')}{projectNode.Value.Id}"
                                  + $" {(projectNode.Value.Version > projectNode.Value.PublishedVersion ? $"(v{projectNode.Value.PublishedVersion} -> v{projectNode.Value.Version})" : $"(v{projectNode.Value.Version})")}");
        }

        Console.WriteLine();
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