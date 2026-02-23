---
name: Regira Database
description: >
  Sets up the project structure, DbContext, EF Core interceptors,
  entity relationships, and database migrations for Regira Entities.
tools:
  - codebase
  - editFiles
  - runCommands
handoffs:
  - label: "Project scaffolded → create models"
    agent: regira-models
    prompt: "Project files are ready. Create the entity models, DTOs, and SearchObjects."
    send: false
  - label: "DbContext updated → create migration"
    agent: regira-database
    prompt: "Run: dotnet ef migrations add <MigrationName> && dotnet ef database update"
    send: false
---

# Regira Entities — Database Agent

You manage the **database layer**: project scaffolding, DbContext, EF Core interceptors,
entity relationships, and migrations.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## File Locations

```
YourProject/
├── YourProject.csproj
├── NuGet.Config
├── appsettings.json
├── Program.cs
└── Data/
    └── AppDbContext.cs
```

---

## 1 — NuGet.Config

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

## 2 — Project File (`.csproj`)

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

    <PackageReference Include="Regira.Entities.DependencyInjection" Version="*" />
    <PackageReference Include="Regira.Entities.Mapping.Mapster" Version="*" />

    <!-- SQLite (default) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />

    <!-- Swap to SQL Server if needed: -->
    <!-- <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="*" /> -->

    <!-- Attachments (add only when needed): -->
    <!-- <PackageReference Include="Regira.IO.Storage" Version="*" /> -->
    <!-- <PackageReference Include="Regira.IO.Storage.Azure" Version="*" /> -->
  </ItemGroup>
</Project>
```

---

## 3 — appsettings.json

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
      "Override": { "Microsoft": "Information", "System": "Information" }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
          "retainedFileCountLimit": 31
        }
      }
    ],
    "Enrich": [ "FromLogContext" ]
  }
}
```

---

## 4 — Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Regira.Entities.EFcore.Primers;       // AddPrimerInterceptors
using Regira.Entities.EFcore.Normalizing;   // AddNormalizerInterceptors
using Regira.DAL.EFcore.Services;           // AddAutoTruncateInterceptors
using Regira.DAL.EFcore.Extensions;         // SetDecimalPrecisionConvention

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
    builder.Services.AddOpenApi();

    // IMPORTANT: use (sp, options) => so interceptors can resolve services from DI
    builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)          // runs Primers on SaveChanges
               .AddNormalizerInterceptors(sp)       // runs Normalizers on SaveChanges
               .AddAutoTruncateInterceptors());      // truncates strings to [MaxLength]

    builder.Services.AddEntityServices();           // Extensions/ServiceCollectionExtensions.cs

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) { Log.Error(ex, "Host failed to start"); }
finally { Log.CloseAndFlush(); }
```

### Interceptor Rules

| Method | Needs `sp`? | Effect |
|--------|------------|--------|
| `AddPrimerInterceptors(sp)` (`Regira.Entities.EFcore.Primers`) | ✓ Yes | Runs `IEntityPrimer` implementations |
| `AddNormalizerInterceptors(sp)` (`Regira.Entities.EFcore.Normalizing`) | ✓ Yes | Runs `IEntityNormalizer` implementations |
| `AddAutoTruncateInterceptors()` (`Regira.DAL.EFcore.Services`) | ✗ No | Truncates strings to `[MaxLength]` |

> **Never** use `options =>` when you need `sp`. Always use `(sp, options) =>`.

---

## 5 — DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Add one DbSet per entity
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    // Attachments (added by Attachments Agent):
    // public DbSet<Attachment> Attachments { get; set; } = null!;
    // public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply decimal precision globally — do NOT set per property
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Relationships
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Relationship Patterns

```csharp
// One-to-many (parent → children)
entity.HasOne(c => c.Parent)
      .WithMany(p => p.Children)
      .HasForeignKey(c => c.ParentId);

// One-to-many without nav on parent
entity.HasOne(c => c.Parent)
      .WithMany()
      .HasForeignKey(c => c.ParentId);

// Self-referencing tree
entity.HasOne(c => c.Parent)
      .WithMany(c => c.Children)
      .HasForeignKey(c => c.ParentId)
      .IsRequired(false);

// Attachment relationship (set by Attachments Agent)
entity.HasMany(e => e.Attachments)
      .WithOne()
      .HasForeignKey(e => e.ObjectId)
      .HasPrincipalKey(e => e.Id);
```

### Adding Indexes

```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.HasIndex(e => e.CategoryId);   // FK columns
    entity.HasIndex(e => e.IsArchived);   // frequently filtered
    entity.HasIndex(e => e.Created);      // date ranges
});
```

---

## 6 — Checklist: Adding a New Entity

- [ ] Add `DbSet<{Entity}> {Entities} { get; set; } = null!;`
- [ ] Configure relationships in `OnModelCreating`
- [ ] Add indexes on FK and frequently filtered columns
- [ ] Create and apply migration

---

## 7 — Migrations

```bash
# Create migration
dotnet ef migrations add Add_{Entity}

# Apply to database
dotnet ef database update

# Preview SQL without applying
dotnet ef migrations script

# Roll back (update DB first, then remove the migration file)
dotnet ef database update {PreviousMigrationName}
dotnet ef migrations remove
```

**Naming conventions:**

| Action | Example name |
|--------|-------------|
| New entity | `Add_Product` |
| New column | `Product_AddSku` |
| Remove column | `Product_RemoveLegacyCode` |
| New relationship | `Product_AddCategoryRelation` |
