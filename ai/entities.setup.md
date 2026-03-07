# Regira Entities — Project Setup

> **AI Agent Rule**: Follow this guide to scaffold a new Regira Entities API project from scratch.
> Always combine with [`entities.namespaces.md`](./entities.namespaces.md) for exact `using` directives
> and [`entities.examples.md`](./entities.examples.md) for complete working code.

---

## Defaults

**Defaults (unless instructed otherwise):**
- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository` (unless complex logic requires wrapping)
- **Many-to-many relationships**: prefer option A

---

## Checklist

1. Create an ASP.NET Core Web API project targeting the latest .NET version (or the solution's target).
2. Add `NuGet.Config` with the Regira feed.
3. Add required packages to `.csproj`.
4. Create `YourDbContext` deriving from `DbContext`.
5. Configure `Program.cs`.
6. Create the DI extension method (`AddEntityServices`).
7. Add entities — see [Entity Implementation Workflow](./entities.instructions.md#entity-implementation-workflow).

---

## NuGet Feed

**Package source URL:**
```xml
<add key="Regira" value="https://packages.regira.com/v3/index.json" />
```

**NuGet.Config** (add to project root):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
    <add key="Regira" value="https://packages.regira.com/v3/index.json" />
  </packageSources>
</configuration>
```

---

## Packages

**Required:**
- `Regira.Entities.DependencyInjection`: Core entities framework with DI extensions
- Mapping:
    - `Regira.Entities.Mapping.Mapster`: Mapster integration (default)
    - OR `Regira.Entities.Mapping.AutoMapper`: AutoMapper integration

**Optional:** (when using entity attachments)
- `Regira.IO.Storage` — File storage (local file system or SFTP)
- `Regira.IO.Storage.Azure` — Azure Blob Storage support for attachments

---

## Step 1: Project Files

Sample files with [serilog](https://serilog.net) configuration and EF Core setup. Adjust as needed for your project structure and preferences.
Basic JSON configuration is included for System.Text.Json, but you can replace with Newtonsoft.Json if preferred.
Replace Wildcard versions (`Version="*"`) with specific versions as needed. Always check for the latest stable versions of the packages, unless requested otherwise.

### *.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="*" />
    <PackageReference Include="Scalar.AspNetCore" Version="*" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="*" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="*" />
    <PackageReference Include="Serilog.Sinks.File" Version="*" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json

> This template is intended for ASP.NET Core **API projects**. Replace `xxx` with the actual port numbers.

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "scalar",
      "applicationUrl": "https://localhost:xxx;http://localhost:xxx",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### appsettings.json
```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### Program.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)
               .AddNormalizerInterceptors(sp)
               .AddAutoTruncateInterceptors());

    builder.Services.AddEntityServices();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Host failed");
}
finally
{
    Log.CloseAndFlush();
}
```

> **Note:** `AddPrimerInterceptors(sp)` and `AddNormalizerInterceptors(sp)` require the `IServiceProvider` (`sp`) from the `AddDbContext` factory overload. Always use the `(sp, options) => ...` signature.

---

## Step 2: Create DbContext

> **→ See:** [`entities.examples.md`](./entities.examples.md) — §2 DbContext

- Derive from `DbContext`
- Call `modelBuilder.SetDecimalPrecisionConvention(18, 2)` in `OnModelCreating` for global decimal precision
- Add `DbSet<TEntity>` per entity incrementally
- Configure relationships per entity in `OnModelCreating`

---

## Step 3: Create the DI Extension Method

Create `Extensions/ServiceCollectionExtensions.cs`.

> **→ See:** [`entities.examples.md`](./entities.examples.md) — §3 DI Extension Methods

- Call `options.UseDefaults()` to register primers, global query filters, and normalizer services
- Call `options.UseMapsterMapping()` (default) or `options.UseAutoMapper()` for DTO mapping
- Create one `Add{EntityNameInPlural}()` extension method per entity on `IEntityServiceCollection<TContext>` for composability
