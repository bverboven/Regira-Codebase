using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Regira.DAL.MySQL.Models;
using Regira.DAL.MySQL.Services;
using Regira.Utilities;

string? sqlInputPath = null;
if (args.Length == 1)
{
    sqlInputPath = args[0];
}
else
{
    var argDic = args.ToDictionary("-");
    if (argDic.ContainsKey("input"))
    {
        sqlInputPath = argDic["i"];
    }
}

if (string.IsNullOrWhiteSpace(sqlInputPath))
{
    Console.WriteLine("Path of SQL dump file? (Empty to look for first *.sql file in current folder)");
    sqlInputPath = Console.ReadLine()!;
}

if (string.IsNullOrWhiteSpace(sqlInputPath))
{
    Console.WriteLine($"Processing directory {AppContext.BaseDirectory}");
    sqlInputPath = Directory.GetFiles(AppContext.BaseDirectory, "*.sql", SearchOption.TopDirectoryOnly).FirstOrDefault();
    Console.WriteLine($"Found <{sqlInputPath ?? "nothing"}>");
}

if (!File.Exists(sqlInputPath))
{
    Console.WriteLine($"File {sqlInputPath} does not exist");
    Console.ReadLine();
    return;
}

// configuration
var host = CreateHostBuilder(args).Build();
var sqlDumpManager = host.Services.GetRequiredService<SQLDumpManager>();

var sqlDump = await File.ReadAllTextAsync(sqlInputPath);
var cleaned = sqlDumpManager.Cleanup(sqlDump);
var outputDir = Path.GetDirectoryName(sqlInputPath)!;
await File.WriteAllTextAsync(Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(sqlInputPath)}-cleaned.sql"), cleaned);

var result = await sqlDumpManager.CorrectQuerySequence(sqlDump);
var resultPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(sqlInputPath)}-corrected-{DateTime.Now:yyyyMMddHHmmss}.sql");
await File.WriteAllTextAsync(resultPath, result.Output);
var failedPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(sqlInputPath)}-failed-{DateTime.Now:yyyyMMddHHmmss}.sql");
await File.WriteAllTextAsync(failedPath, result.Failed);


Console.WriteLine("Target database name? (Press [Enter] to exit)");
var targetDbName = Console.ReadLine();
if (!string.IsNullOrWhiteSpace(targetDbName))
{
    //var correctedSQL = await File.ReadAllTextAsync(resultPath);
    var config = host.Services.GetRequiredService<IConfiguration>();
    var dbSettings = host.Services.GetRequiredService<MySqlSettings>();
    await using var baseDb = new MySqlConnection(dbSettings.BuildConnectionString());
    await baseDb.OpenAsync();
    await baseDb.ExecuteAsync($"CREATE DATABASE {targetDbName};");
    await baseDb.CloseAsync();
    dbSettings.DatabaseName = targetDbName;
    await using var targetDb = new MySqlConnection(dbSettings.BuildConnectionString(new KeyValuePair<string, string>("Allow User Variables", "true")));
    await targetDb.OpenAsync();
    var queries = result.Output.Split(config["Splitter"], StringSplitOptions.RemoveEmptyEntries)
        .Where(x => !x.StartsWith("-- "))
        .ToList();
    foreach (var query in queries)
    {
        await targetDb.ExecuteAsync(query);
    }
    await targetDb.CloseAsync();
}


Console.WriteLine("The End");

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
#if DEBUG
        .AddUserSecrets(typeof(Program).Assembly, true)
#endif
        ;
}

static void ConfigureServices(HostBuilderContext context, IServiceCollection services, params string[] args)
{
    var config = context.Configuration;

    var connectionString = config["ConnectionStrings:MySQLBase"]!;
    var splitter = config["Splitter"];
    services
        .AddTransient(_ => MySqlSettings.FromConnectionString(connectionString))
        .AddTransient(p =>
        {
            var sqlDumpManager = new SQLDumpManager(MySqlSettings.FromConnectionString(connectionString), splitter);
            sqlDumpManager.OnAction += (action, arg) =>
            {
                if (action != "Query" || ((int)arg > 0 && (int)arg % 100 == 0))
                {
                    Console.WriteLine($"{action}: {arg}");
                }
            };
            return sqlDumpManager;
        });
}