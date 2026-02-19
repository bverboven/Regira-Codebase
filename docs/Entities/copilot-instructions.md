# Regira Entities AI Agent Instructions

You are an expert .NET developer specializing in the Regira Entities framework. Your role is to help create new API projects and add/modify entities in existing projects using the Regira Entities framework.

**Defaults (unless instructed otherwise):**
- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository` (unless complex logic requires wrapping)

Always prefer clear, conventional patterns over clever solutions. Default to the more feature-rich options when in doubt.

---

## Quick Agent Playbook

Use this as the primary checklist.

### Create a New Regira API Project

1. Create an ASP.NET Core Web API project targeting the latest .NET version (or the solution's target).
2. Add `NuGet.Config` with the Regira feed (see below).
3. Add the required Regira and EF Core packages to the project file.
4. Create a `YourDbContext` deriving from `DbContext` and configure it.
5. In `Program.cs`:
   - Register `YourDbContext` via `AddDbContext<YourDbContext>(...)`
   - Inside the DbContext configuration, add any interceptors (primers, normalizers, auto-truncate) as needed.
   - Call `UseEntities<YourDbContext>(...)` on `builder.Services`, preferably via an extension method.
   - Inside `UseEntities` config, call `.UseDefaults()` by default, then add mapping and any global services.
6. Add entities using the workflow below.

### Add a New Entity to an Existing Project

1. Add the entity class and implement the appropriate interfaces.
2. Add `SearchObject`, `SortBy`, `Includes`, and DTOs as needed.
3. Add optional query builder / processor / prepper classes.
4. Register the entity on the `EntityServiceCollection` using `.For<TEntity,...>(...)`.
5. Add an API controller inheriting from the full `EntityControllerBase` variant.
6. Add `DbSet<TEntity>` to `YourDbContext` and configure relationships.
7. Create and apply an EF migration as needed.

### Modify an Existing Entity

1. Update the entity class and related `DbSet`/relationships.
2. Update DTOs and mapping configuration.
3. Update `SearchObject`, enums, and query builders if filters/sorting change.
4. Adjust processors, preppers, and normalizers if behavior changes.
5. Create and apply a migration when the schema changes.

---

## NuGet Packages

**Package Source:**
```xml
<add key="Regira" value="https://packages.regira.com/v3/index.json" />
```

**NuGet.Config (add to project root):**
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

**Required Packages:**
- `Regira.Entities.DependencyInjection` — Core entities framework with DI extensions
- `Regira.Entities.Mapping.Mapster` — Mapster integration (default)
  - OR `Regira.Entities.Mapping.AutoMapper` — AutoMapper integration
- `Microsoft.EntityFrameworkCore.Sqlite` — EF Core SQLite provider (default)
  - OR `Microsoft.EntityFrameworkCore.SqlServer` — SQL Server
- `Microsoft.EntityFrameworkCore.Design` — EF Core design-time tools (migrations)

**Optional Packages:**
- `Regira.IO.Storage` — File storage (local file system or SFTP)
- `Regira.IO.Storage.Azure` — Azure Blob Storage support for attachments

**Project File (`*.csproj`) example:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <!-- OpenAPI -->
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="*" />
    <PackageReference Include="Scalar.AspNetCore" Version="*" />

    <!-- Logging -->
    <PackageReference Include="Serilog.Extensions.Hosting" Version="*" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="*" />
    <PackageReference Include="Serilog.Sinks.File" Version="*" />

    <!-- Regira Entities -->
    <PackageReference Include="Regira.Entities.DependencyInjection" Version="*" />
    <PackageReference Include="Regira.Entities.Mapping.Mapster" Version="*" />

    <!-- EF Core (SQLite default) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />
  </ItemGroup>
</Project>
```

---

## Namespace Reference

**Always use exact namespaces — never guess.**

### Core Entity Interfaces & Models
```csharp
using Regira.Entities.Models;                              // SearchObject, EntitySortBy
using Regira.Entities.Models.Abstractions;                 // IEntity<TKey>, IEntityWithSerial, IHasTimestamps,
                                                           // IHasTitle, IHasDescription, IHasCode,
                                                           // IArchivable, ISortable, IHasCreated,
                                                           // IHasLastModified, IHasAttachments,
                                                           // IHasNormalizedContent, IHasObjectId
```

### Services & Query Builders
```csharp
using Regira.Entities.Services.Abstractions;               // IEntityService<...>
using Regira.Entities.EFcore.QueryBuilders.Abstractions;   // FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
                                                           // GlobalFilteredQueryBuilderBase<TEntity>
                                                           // GlobalFilteredQueryBuilderBase<TEntity, TKey>
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders; // FilterIdsQueryBuilder,
                                                           // FilterArchivablesQueryBuilder,
                                                           // FilterHasCreatedQueryBuilder,
                                                           // FilterHasLastModifiedQueryBuilder,
                                                           // FilterHasNormalizedContentQueryBuilder
```

### Processors & Preppers
```csharp
using Regira.Entities.Services.Abstractions;               // IEntityProcessor<TEntity, TIncludes>
                                                           // IEntityPrepper<TEntity>, EntityPrepperBase<TEntity>
```

### Primers (EF Core Interceptors)
```csharp
using Regira.Entities.EFcore.Primers;                      // ArchivablePrimer, HasCreatedDbPrimer,
                                                           // HasLastModifiedDbPrimer, AutoTruncatePrimer
using Regira.Entities.EFcore.Primers.Abstractions;         // EntityPrimerBase<T>, IEntityPrimer<T>
```

### Normalizing
```csharp
using Regira.Normalizing;                                  // [NormalizedAttribute]
using Regira.Normalizing.Abstractions;                     // INormalizer, IQKeywordHelper, IObjectNormalizer
using Regira.Entities.EFcore.Normalizing;                  // DefaultEntityNormalizer
using Regira.Entities.EFcore.Normalizing.Abstractions;     // IEntityNormalizer<T>, EntityNormalizerBase<T>
```

### Custom/Wrapping Services
```csharp
using Regira.Entities.Services;                            // EntityWrappingServiceBase<TEntity, TKey, ...>
using Regira.Entities.Exceptions;                          // EntityInputException<T>
```

### Dependency Injection
```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;    // UseEntities(), UseDefaults()
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;  // IEntityServiceCollection
```

### Mapping
```csharp
using Regira.Entities.Mapping.Mapster;    // UseMapsterMapping()
// OR
using Regira.Entities.Mapping.AutoMapper; // UseAutoMapper()

using Regira.Entities.Mapping.Abstractions; // IEntityMapper, IEntityAfterMapper,
                                            // EntityAfterMapperBase<TSource, TTarget>
```

### EF Core Extensions & Interceptors
```csharp
using Regira.Entities.EFcore.Primers;     // AddPrimerInterceptors(sp)
using Regira.Entities.EFcore.Normalizing; // AddNormalizerInterceptors(sp)
using Regira.DAL.EFcore.Services;         // AddAutoTruncateInterceptors()
using Regira.DAL.EFcore.Extensions;       // SetDecimalPrecisionConvention()
using Regira.DAL.Paging;                  // PagingInfo
```

### Attachments
```csharp
using Regira.Entities.Attachments.Models;          // EntityAttachment<TKey, TObjectKey>, Attachment
using Regira.Entities.Web.Attachments.Abstractions;// EntityAttachmentControllerBase
using Regira.IO.Storage.FileSystem;                // BinaryFileService, FileSystemOptions
using Regira.IO.Storage.Abstractions;              // IFileService
// Azure Blob Storage:
using Regira.IO.Storage.Azure;                     // BinaryBlobService, AzureOptions, AzureCommunicator
// SFTP:
using Regira.IO.Storage.Sftp;                      // SftpService
```

### Controllers
```csharp
using Regira.Entities.Web.Controllers.Abstractions; // EntityControllerBase<...>
using Microsoft.AspNetCore.Mvc;                     // [ApiController], [Route]
```

### Common System/EF Namespaces
```csharp
using System.ComponentModel.DataAnnotations; // [Required], [MaxLength], [Range]
using Microsoft.EntityFrameworkCore;         // DbContext, Include(), OrderBy()
```

---

## Core Understanding

### Framework Architecture

**Regira Entities** is a generic, extensible framework for managing data entities in .NET with standardized CRUD operations.

**Key Components:**
- **Entity Models**: POCO classes implementing `IEntity<TKey>`
- **Services**: `IEntityService` (default: `EntityRepository` backed by DbContext)
- **Controllers**: API endpoints inheriting from `EntityControllerBase`
- **DTOs**: Separate read (`TDto`) and write (`TInputDto`) models
- **Pipeline Services**: QueryBuilders, Processors, Preppers, Primers, AfterMappers

### Generic Type System

| Type | Required | Purpose | Example |
|------|----------|---------|---------|
| `TEntity` | ✓ | The entity class | `Product` |
| `TKey` | ✓* | Primary key type (*default: `int`) | `Guid`, `int` |
| `TSearchObject` | ○ | Advanced filtering | `ProductSearchObject` |
| `TSortBy` | ○ | Sorting enum | `ProductSortBy` |
| `TInclude` | ○ | Navigation properties enum | `ProductIncludes` |
| `TDto` | ○ | Read/display model | `ProductDto` |
| `TInputDto` | ○ | Create/update model | `ProductInputDto` |

### Processing Pipelines

**Read Pipeline:**
```
EntitySet → QueryBuilders (Filters → Sorting → Paging → Includes) → Processors → Mapping → AfterMapping*
```
*AfterMapping is only executed in API controllers

**Write Pipeline:**
```
Input → Mapping* → AfterInput* → Preppers → SaveChanges → Primers (Interceptors) → Submit
```
*Only executed in API controllers

---

## Decision-Making Guidelines

### When to Use Inline vs Separate Classes

**Use INLINE configuration when:**
- Simple logic (< 10 lines)
- Entity-specific, not reusable
- Rapid prototyping

**Use SEPARATE classes when:**
- Complex logic
- Needs dependency injection (DbContext, services)
- Reusable across entities
- Production-quality code requiring testability

### Choosing Base Controller

```csharp
// Minimal (no search or sorting)
EntityControllerBase<TEntity, TDto, TInputDto>

// With search
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>

// Full-featured (recommended for complex scenarios)
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

### Service Layer Decisions

**Use default `EntityRepository` when:**
- Standard CRUD is sufficient
- Custom logic fits in QueryBuilders / Processors / Preppers / Primers

**Create custom wrapping service when:**
- Caching layer is needed
- Security/authorization logic needed at service level
- Auditing or logging around operations
- Multiple data sources need to be combined
- Complex validation spanning multiple entities

---

## Project Creation Workflow

### Step 1: Project Files

**appsettings.json:**
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

**Program.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Regira.DAL.EFcore.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
    builder.Services.AddOpenApi();

    // DbContext — SQLite default
    builder.Services.AddDbContext<YourDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)          // Enables Primer interceptors
               .AddNormalizerInterceptors(sp)       // Enables Normalizer interceptors
               .AddAutoTruncateInterceptors());      // Auto-truncates strings to MaxLength

    // Regira Entities — register via extension method
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

### Step 2: Create DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

    // DbSets added per entity
    // public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply decimal precision globally (avoids setting it on every property)
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Entity configurations added per entity
    }
}
```

### Step 3: Create the DI Extension Method

Create `Extensions/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.UseEntities<YourDbContext>(options =>
        {
            // Registers default primers, global query filters, and default normalizer services
            options.UseDefaults();

            // Use Mapster for DTO mapping (default)
            options.UseMapsterMapping();

            // Optional: global inline preppers
            // options.AddPrepper<ISomeInterface>(x => x.SomeField ??= Guid.NewGuid());
        })
        // Entities are added here via extension methods or .For<>() calls
        // .AddProducts()
        // .AddCategories()
        ;

        return services;
    }
}
```

**Tip:** Create separate extension methods per entity for cleaner code:

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

public static class ProductServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddProducts<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            // configure here
        });
        return services;
    }
}
```

---

## Entity Implementation Workflow

### Step 1: Create Entity Model

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

public class YourEntity : IEntityWithSerial, IHasTimestamps, IHasTitle, IArchivable, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    // Normalizes Title + Description into NormalizedContent for full-text search
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    // Navigation properties
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

**Interface selection checklist:**

| Interface | Add when… |
|-----------|-----------|
| `IEntityWithSerial` | int primary key (auto-increment). Shortcut for `IEntity<int>` |
| `IEntity<TKey>` | Non-int primary key (e.g. `Guid`) |
| `IHasTimestamps` | Track Created + LastModified |
| `IArchivable` | Soft-delete instead of hard-delete |
| `IHasTitle` | Entity has a short display name |
| `IHasDescription` | Entity has a long text field |
| `IHasCode` | Entity has a short unique code |
| `ISortable` | Used as a sortable child collection |
| `IHasNormalizedContent` | Entity uses normalized text for search |
| `IHasAttachments` | Entity can have file attachments |

### Step 2: Create SearchObject

```csharp
using Regira.Entities.Models;

public class YourEntitySearchObject : SearchObject // SearchObject defaults TKey to int
{
    // Custom filter properties
    public ICollection<int>? CategoryId { get; set; } // prefer ICollection<TKey> for FK filters
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Already inherited from SearchObject<int>:
    // int? Id, ICollection<int>? Ids, ICollection<int>? Exclude
    // string? Q  (general text search)
    // DateTime? MinCreated, MaxCreated, MinLastModified, MaxLastModified
    // bool? IsArchived
}
```

> Use `ICollection<TKey>` (not a single value) for FK filter properties — enables filtering by multiple values.

### Step 3: Create SortBy Enum

```csharp
// NOTE: NOT a [Flags] enum — values are applied in sequence, not combined
public enum YourEntitySortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Price,
    PriceDesc,
    Created,
    CreatedDesc
}
```

### Step 4: Create Includes Enum

```csharp
// IS a [Flags] enum — values are combined with bitwise OR
[Flags]
public enum YourEntityIncludes
{
    Default = 0,
    Category  = 1 << 0,
    Reviews   = 1 << 1,
    All = Category | Reviews
}
```

### Step 5: Create DTOs

**Output DTO (for reading):**
```csharp
public class YourEntityDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }

    // Flattened/enriched navigation data
    public string? CategoryTitle { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
```

**Input DTO (for creating/updating):**
```csharp
using System.ComponentModel.DataAnnotations;

public class YourEntityInputDto
{
    // Include Id to support the Save (upsert) action
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    // Do NOT include: Created, LastModified, NormalizedContent, computed properties
    // Only include child collections if they are configured using e.Related(...)
}
```

### Step 6: Create Query Builders

**Option A: Inline (simple logic):**
```csharp
// Inside .For<YourEntity,...>(e => { ... })
e.Filter((query, so) =>
{
    if (so?.CategoryId?.Any() == true)
        query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));
    if (so?.MinPrice != null)
        query = query.Where(x => x.Price >= so.MinPrice);
    if (so?.MaxPrice != null)
        query = query.Where(x => x.Price <= so.MaxPrice);
    return query;
});
```

**Option B: Separate Class (complex logic / DI needed):**
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public class YourEntityQueryBuilder : FilteredQueryBuilderBase<YourEntity, int, YourEntitySearchObject>
{
    // Inject services if needed (e.g. ICurrentUserService)
    public YourEntityQueryBuilder() { }

    public override IQueryable<YourEntity> Build(IQueryable<YourEntity> query, YourEntitySearchObject? so)
    {
        if (so == null) return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));

        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);

        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);

        return query;
    }
}

// Register with:
e.AddQueryFilter<YourEntityQueryBuilder>();
```

**Sorting (inline):**
```csharp
e.SortBy((query, sortBy) =>
{
    // Support ThenBy when query is already sorted
    if (query is IOrderedQueryable<YourEntity> sorted)
    {
        return sortBy switch
        {
            YourEntitySortBy.Title    => sorted.ThenBy(x => x.Title),
            YourEntitySortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
            YourEntitySortBy.Price    => sorted.ThenBy(x => x.Price),
            YourEntitySortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
            _                         => sorted.ThenByDescending(x => x.Created)
        };
    }
    return sortBy switch
    {
        YourEntitySortBy.Title    => query.OrderBy(x => x.Title),
        YourEntitySortBy.TitleDesc => query.OrderByDescending(x => x.Title),
        YourEntitySortBy.Price    => query.OrderBy(x => x.Price),
        YourEntitySortBy.PriceDesc => query.OrderByDescending(x => x.Price),
        YourEntitySortBy.Created  => query.OrderBy(x => x.Created),
        YourEntitySortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
        _                         => query.OrderByDescending(x => x.Created)
    };
});
```

**Includes (inline):**
```csharp
e.Includes((query, includes) =>
{
    if (includes?.HasFlag(YourEntityIncludes.Category) == true)
        query = query.Include(x => x.Category);
    if (includes?.HasFlag(YourEntityIncludes.Reviews) == true)
        query = query.Include(x => x.Reviews);
    return query;
});
```

### Step 7: Processors (Optional)

Use processors to fill `[NotMapped]` properties or enrich entities **after** fetching from the database.

**Inline:**
```csharp
e.Process((items, includes) =>
{
    foreach (var item in items)
    {
        item.DisplayPrice = $"€{item.Price:F2}";
        item.IsOnSale = item.SalePrice.HasValue && item.SalePrice < item.Price;
    }
    return Task.CompletedTask;
});
```

**Separate class (with DI):**
```csharp
using Regira.Entities.Services.Abstractions;

public class YourEntityProcessor : IEntityProcessor<YourEntity, YourEntityIncludes>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public YourEntityProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task Process(IList<YourEntity> items, YourEntityIncludes? includes)
    {
        var baseUrl = _httpContextAccessor.HttpContext?.Request.Host.ToString();
        foreach (var item in items)
        {
            item.ImageUrl = $"https://{baseUrl}/images/{item.Id}";
        }
        return Task.CompletedTask;
    }
}

// Register with:
e.Process<YourEntityProcessor>();
```

### Step 8: Preppers (Optional)

Use preppers to prepare entities **before saving** — e.g. generate codes, set FKs, recalculate totals.

**Inline (simple):**
```csharp
e.Prepare(item =>
{
    item.Slug ??= item.Title.ToLowerInvariant().Replace(' ', '-');
});
```

**Inline with original (for comparison):**
```csharp
// original is null when creating, non-null when updating
e.Prepare((modified, original) =>
{
    if (original == null)
        modified.CreatedBy = "system";
});
```

**Inline with DbContext:**
```csharp
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});
```

**Separate class:**
```csharp
using Regira.Entities.Services.Abstractions;

public class YourEntityPrepper : EntityPrepperBase<YourEntity>
{
    public override Task Prepare(YourEntity modified, YourEntity? original)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}

// Register with:
e.Prepare<YourEntityPrepper>();
```

**Child collections (RelatedCollectionPrepper):**
```csharp
// Shortcut — creates RelatedCollectionPrepper internally
e.Related(x => x.OrderItems, (order, _) =>
{
    // Optional: additional preparation for child items
    order.OrderItems?.Prepare();
    foreach (var item in order.OrderItems ?? [])
        item.OrderId = order.Id;
});

// Minimal (no extra preparation):
e.Related(x => x.OrderItems);
```

### Step 9: Primers (Optional)

Primers are EF Core `SaveChangesInterceptors` — they run **when DbContext executes SaveChanges**. They must be registered via `AddPrimerInterceptors(sp)` in `AddDbContext`.

**Separate class:**
```csharp
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

public class YourEntityPrimer : EntityPrimerBase<YourEntity>
{
    public override Task PrepareAsync(YourEntity entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();

        return Task.CompletedTask;
    }
}

// Register on the entity:
e.AddPrimer<YourEntityPrimer>();

// OR register globally (applies to all entities implementing an interface):
options.AddPrimer<GlobalPrimerClass>();
```

### Step 10: Mapping & AfterMappers

**Configure mapping on the entity:**
```csharp
e.UseMapping<YourEntityDto, YourEntityInputDto>()
    // AfterMapper: enrich DTO after Entity→DTO mapping
    .After((entity, dto) =>
    {
        dto.CategoryTitle = entity.Category?.Title;
        dto.AttachmentCount = entity.Attachments?.Count ?? 0;
    })
    // AfterInputMapper: modify entity after InputDto→Entity mapping
    .AfterInput((dto, entity) =>
    {
        entity.UpdatedAt = DateTime.UtcNow;
    });
```

**Additional DTO mappings (for child types):**
```csharp
e.AddMapping<OrderItem, OrderItemDto>();
e.AddMapping<OrderItemInputDto, OrderItem>();
```

**Separate AfterMapper class:**
```csharp
using Regira.Entities.Mapping.Abstractions;

public class YourEntityAfterMapper : EntityAfterMapperBase<YourEntity, YourEntityDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public YourEntityAfterMapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void AfterMap(YourEntity source, YourEntityDto target)
    {
        target.CategoryTitle = source.Category?.Title;
    }
}

// Global AfterMapper (applies to all entities implementing IMyInterface):
options.AfterMap<IMyInterface, MyDto, MyGlobalAfterMapper>();
```

### Step 11: Configure Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class YourEntitiesController : EntityControllerBase<
    YourEntity,
    YourEntitySearchObject,
    YourEntitySortBy,
    YourEntityIncludes,
    YourEntityDto,
    YourEntityInputDto>
{
    // Base controller provides:
    // GET    /api/yourentities         → List
    // GET    /api/yourentities/search  → Search (List + Count)
    // GET    /api/yourentities/{id}    → Details
    // POST   /api/yourentities         → Create
    // PUT    /api/yourentities/{id}    → Modify
    // POST   /api/yourentities/save    → Save (upsert)
    // DELETE /api/yourentities/{id}    → Delete
    // POST   /api/yourentities/list    → Complex list (multiple SearchObjects in body)
    // POST   /api/yourentities/search  → Complex search (multiple SearchObjects in body)

    // Only add custom actions when the above are insufficient
}
```

### Step 12: Update DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

    public DbSet<YourEntity> YourEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Step 13: Create and Apply Migration

```bash
dotnet ef migrations add Add_YourEntity
dotnet ef database update
```

### Step 14: Wire Up DI

```csharp
// In ServiceCollectionExtensions.cs
services.UseEntities<YourDbContext>(options => { /* ... */ })
    .For<YourEntity, YourEntitySearchObject, YourEntitySortBy, YourEntityIncludes>(e =>
    {
        e.AddQueryFilter<YourEntityQueryBuilder>();

        e.SortBy((query, sortBy) => { /* ... */ });

        e.Includes((query, includes) => { /* ... */ });

        e.UseMapping<YourEntityDto, YourEntityInputDto>()
            .After((entity, dto) => { /* ... */ });

        e.Process<YourEntityProcessor>();
        e.Prepare<YourEntityPrepper>();
        e.AddPrimer<YourEntityPrimer>();
        e.Related(x => x.ChildItems);
    });
```

---

## Custom Entity Services

### Using EntityWrappingServiceBase

Use `EntityWrappingServiceBase` to wrap the default `EntityRepository` and add cross-cutting concerns like **caching**, **authorization**, **auditing**, or **complex validation** at the service level.

The wrapper delegates all calls to an inner `IEntityService` and you override only what you need.

```csharp
using Regira.Entities.Services;
using Regira.Entities.Services.Abstractions;

// Define a custom interface (optional but enables typed injection)
public interface IProductService : IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>
{
}

// Implement the wrapper
public class ProductService : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>,
    IProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> inner,
        ILogger<ProductService> logger)
        : base(inner)
    {
        _logger = logger;
    }

    // Override only what needs custom behaviour

    public override async Task<Product?> Details(int id)
    {
        _logger.LogDebug("Fetching product {Id}", id);
        return await base.Details(id);
    }

    public override async Task Save(Product item)
    {
        // Custom validation before save
        if (item.Price < 0)
            throw new EntityInputException<Product>("Price cannot be negative")
            {
                Item = item,
                InputErrors = new Dictionary<string, string>
                {
                    [nameof(Product.Price)] = "Must be >= 0"
                }
            };

        await base.Save(item);
    }

    public override async Task<IList<Product>> List(
        IList<ProductSearchObject?> so,
        IList<ProductSortBy> sortBy,
        ProductIncludes? includes = null,
        PagingInfo? pagingInfo = null)
    {
        // Example: add caching
        _logger.LogDebug("Listing products");
        return await base.List(so, sortBy, includes, pagingInfo);
    }
}
```

**Register the wrapping service:**
```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

services.UseEntities<YourDbContext>(/* ... */)
    .For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
    {
        // Register the custom service interface (optional — enables IProductService injection)
        e.AddTransient<IProductService, ProductService>();

        // Replace the default EntityRepository with the wrapping service
        e.UseEntityService<ProductService>();

        // Continue with other configuration
        e.AddQueryFilter<ProductQueryBuilder>();
        e.UseMapping<ProductDto, ProductInputDto>();
    });
```

**Inject by interface in other services:**
```csharp
public class SomeOtherService
{
    private readonly IProductService _productService;

    public SomeOtherService(IProductService productService)
    {
        _productService = productService;
    }
}
```

### EntityWrappingServiceBase Available Overrides

```csharp
// Read
Task<TEntity?> Details(TKey id)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
Task<long> Count(TSearchObject? so)
Task<long> Count(IList<TSearchObject?> so)

// Write (do NOT auto-persist — caller must call SaveChanges())
Task Save(TEntity item)      // calls Add() or Modify()
Task Add(TEntity item)
Task<TEntity?> Modify(TEntity item)
Task Remove(TEntity item)
Task<int> SaveChanges(CancellationToken token = default)
```

---

## Global Services

Global services apply to **all entities implementing a given interface**. They are registered on the `EntityServiceCollectionOptions` (inside `UseEntities`) and run **before** entity-specific services.

### Global Filter Query Builders

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

// Separate class
public class FilterByTenantQueryBuilder : GlobalFilteredQueryBuilderBase<ITenantEntity, int>
{
    private readonly ITenantContext _tenantContext;

    public FilterByTenantQueryBuilder(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override IQueryable<ITenantEntity> Build(IQueryable<ITenantEntity> query, ISearchObject<int>? so)
    {
        return query.Where(x => x.TenantId == _tenantContext.CurrentTenantId);
    }
}

// Register globally:
options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();
```

### Global Preppers (Inline)

```csharp
// Applies to all entities implementing the interface
options.AddPrepper<IHasSlug>(entity =>
{
    entity.Slug ??= entity.Title?.ToLowerInvariant().Replace(' ', '-');
});
```

### Global Primers

```csharp
// Registers a primer that applies to all entities implementing IHasTimestamps
options.AddPrimer<HasCreatedDbPrimer>();
options.AddPrimer<HasLastModifiedDbPrimer>();
options.AddPrimer<ArchivablePrimer>();
```

### UseDefaults() — What It Registers

`options.UseDefaults()` is a convenience method that registers:

**Primers:**
- `ArchivablePrimer` — soft-delete (sets `IsArchived = true` instead of deleting)
- `HasCreatedDbPrimer` — sets `Created` on new entities
- `HasLastModifiedDbPrimer` — sets `LastModified` on update

**Global Query Filters:**
- `FilterIdsQueryBuilder` — filter by `Id`, `Ids`, `Exclude`
- `FilterArchivablesQueryBuilder` — excludes archived items by default (null = active only)
- `FilterHasCreatedQueryBuilder` — filter by `MinCreated`/`MaxCreated`
- `FilterHasLastModifiedQueryBuilder` — filter by `MinLastModified`/`MaxLastModified`

**Normalizer services:**
- `DefaultNormalizer` (`INormalizer`) — removes diacritics, lowercases, normalizes whitespace
- `DefaultObjectNormalizer` (`IObjectNormalizer`) — processes `[Normalized]` attributes
- `DefaultEntityNormalizer<IEntity>` (`IEntityNormalizer`) — orchestrates attribute-based normalization
- `QKeywordHelper` (`IQKeywordHelper`) — parses Q search strings with wildcard support

```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

services.UseEntities<YourDbContext>(options => options.UseDefaults());
```

---

## Normalizing

Normalization facilitates text search by removing diacritics, special characters, and standardizing whitespace.

### Attribute-Based (Recommended)

```csharp
using Regira.Normalizing;

public class Product : IEntityWithSerial
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // Combines Title + Description into a single normalized search field
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    // Single source
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
}
```

**Normalized attribute options:**

| Property | Purpose |
|----------|---------|
| `SourceProperty` | Single source property name |
| `SourceProperties` | Array of source property names (concatenated with space) |
| `Recursive` | Process nested objects (class-level, default: `true`) |
| `Normalizer` | Custom `INormalizer` or `IObjectNormalizer` type |

### Custom Normalizer

```csharp
using Regira.Normalizing.Abstractions;
using Regira.Entities.EFcore.Normalizing.Abstractions;

public class ProductNormalizer : EntityNormalizerBase<Product>
{
    private readonly INormalizer _normalizer;

    public ProductNormalizer(INormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    // When IsExclusive = true, no other normalizer runs for this entity
    // public override bool IsExclusive => true;

    public override async Task HandleNormalize(Product item)
    {
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);

        // Custom: strip non-digits for phone search
        item.NormalizedPhone = new string((item.Phone ?? "").Where(char.IsDigit).ToArray());
    }
}

// Register per entity:
e.AddNormalizer<ProductNormalizer>();

// Register globally (applies to all entities implementing interface):
options.AddNormalizer<IHasPhone, PhoneNormalizer>();
```

### Filtering with Normalized Content and IQKeywordHelper

```csharp
using Regira.Normalizing.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    private readonly IQKeywordHelper _qHelper;

    public ProductQueryBuilder(IQKeywordHelper qHelper)
    {
        _qHelper = qHelper;
    }

    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            // Parses Q into keywords with wildcard support (e.g. "blue*" → "blue%")
            var keywords = _qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
            {
                // keyword.QW is the wildcard version for LIKE queries
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
            }
        }
        return query;
    }
}
```

**Or use the built-in global filter:**
```csharp
// In UseEntities global options — applies to all entities implementing IHasNormalizedContent
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

### Enable Normalizer Interceptors

Normalizers run automatically when saving via `AddNormalizerInterceptors`:

```csharp
builder.Services.AddDbContext<YourDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddNormalizerInterceptors(sp));  // sp = IServiceProvider (required)
```

---

## Attachments

### 1. EntityAttachment Model

```csharp
using Regira.Entities.Attachments.Models;

public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}
```

### 2. Update Owning Entity

```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Models;

public class Product : IEntityWithSerial, IHasAttachments, IHasAttachments<ProductAttachment>
{
    public int Id { get; set; }
    // ... other properties ...

    // Required by IHasAttachments
    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }

    // Explicit interface implementation required
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
}
```

### 3. Create Attachment Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;

[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int>
{
    // Uses default DTOs (EntityAttachmentDto & EntityAttachmentInputDto)
}

// OR with custom DTOs:
public class ProductAttachmentsController
    : EntityAttachmentControllerBase<ProductAttachment, ProductAttachmentDto, ProductAttachmentInputDto>
{
}
```

### 4. Update DbContext

```csharp
using Regira.Entities.Attachments.Models;

public class YourDbContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductAttachment>()
            .HasOne(x => x.Attachment)
            .WithMany()
            .HasForeignKey(x => x.AttachmentId);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasMany(e => e.Attachments)
                  .WithOne()
                  .HasForeignKey(e => e.ObjectId)
                  .HasPrincipalKey(e => e.Id);
        });
    }
}
```

### 5. Configure DI

**Local file system (default):**
```csharp
using Regira.IO.Storage.FileSystem;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions
        {
            RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        }
    ));
```

**Azure Blob Storage:**
```csharp
using Regira.IO.Storage.Azure;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryBlobService(
        new AzureCommunicator(new AzureOptions
        {
            ConnectionString = configuration["Azure:StorageConnectionString"],
            ContainerName = "attachments"
        })
    ));
```

**SFTP:**
```csharp
using Regira.IO.Storage.Sftp;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new SftpService(new SftpOptions
    {
        Host = "sftp.example.com",
        Username = "user",
        Password = "pass",
        RootFolder = "/uploads"
    }));
```

---

## Error Handling

### EntityInputException (returns HTTP 400)

Controllers automatically catch `EntityInputException` and return `BadRequest (400)`.

```csharp
using Regira.Entities.Exceptions;

// Throw in a prepper, service, or wrapping service
throw new EntityInputException<Product>("Validation failed")
{
    Item = item,
    InputErrors = new Dictionary<string, string>
    {
        [nameof(Product.Price)] = "Price must be greater than 0",
        [nameof(Product.Title)] = "Title is required"
    }
};
```

---

## Built-in Query Extensions

These LINQ extension methods are available for use inside query builders:

```csharp
using Regira.Entities.EFcore.QueryBuilders; // QueryExtensions

query.FilterId(so.Id)
query.FilterIds(so.Ids)
query.FilterExclude(so.Exclude)
query.FilterCode(so.Code)
query.FilterTitle(keywords)          // uses IHasTitle
query.FilterNormalizedTitle(keywords) // uses normalized title
query.FilterCreated(so.MinCreated, so.MaxCreated)
query.FilterLastModified(so.MinLastModified, so.MaxLastModified)
query.FilterTimestamps(minCreated, maxCreated, minModified, maxModified)
query.FilterQ(keywords)              // general Q search
query.FilterArchivable(so.IsArchived)
query.FilterHasAttachment(so.HasAttachment)
query.SortQuery<TEntity, TKey>()     // default sort
```

**Pagination:**
```csharp
using Regira.DAL.Paging;

query.PageQuery(pagingInfo)
query.PageQuery(pageSize: 20, page: 1)
```

---

## Common Patterns

### Master-Detail (Order + OrderItems)

```csharp
// Entity
public class Order : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class OrderItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}

// DI configuration
.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
{
    // Child collection management
    e.Related(x => x.OrderItems, (order, _) =>
    {
        order.OrderItems?.Prepare(); // updates SortOrder, removes deleted
        foreach (var item in order.OrderItems ?? [])
            item.OrderId = order.Id;
    });

    // Recalculate total
    e.Prepare(item =>
    {
        item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    });
});
```

### Soft Delete

```csharp
// Entity
public class Product : IEntityWithSerial, IArchivable
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}

// UseDefaults() already registers ArchivablePrimer + FilterArchivablesQueryBuilder
// SearchObject.IsArchived controls filtering:
//   null  = only active (not archived) — default
//   false = only active
//   true  = only archived
```

### Audit Trail with Custom Primer

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

// Application-specific interface (not from Regira)
public interface IAuditable
{
    int CreatedBy { get; set; }
    int? ModifiedBy { get; set; }
}

// Primer
public class UserTrackingPrimer : EntityPrimerBase<IAuditable>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTrackingPrimer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task PrepareAsync(IAuditable entity, EntityEntry entry)
    {
        var userId = int.Parse(_httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value ?? "0");

        if (entry.State == EntityState.Added)
            entity.CreatedBy = userId;
        else if (entry.State == EntityState.Modified)
            entity.ModifiedBy = userId;

        return Task.CompletedTask;
    }
}

// Register globally
options.AddPrimer<UserTrackingPrimer>();
```

### Caching with EntityWrappingServiceBase

```csharp
using Microsoft.Extensions.Caching.Memory;
using Regira.Entities.Services;
using Regira.Entities.Services.Abstractions;
using Regira.DAL.Paging;

public class CachedProductService
    : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public CachedProductService(
        IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> inner,
        IMemoryCache cache)
        : base(inner)
    {
        _cache = cache;
    }

    public override async Task<Product?> Details(int id)
    {
        var key = $"product:{id}";
        if (_cache.TryGetValue(key, out Product? cached))
            return cached;

        var result = await base.Details(id);
        if (result != null)
            _cache.Set(key, result, CacheDuration);

        return result;
    }

    public override async Task Save(Product item)
    {
        await base.Save(item);
        _cache.Remove($"product:{item.Id}");
    }
}

// Register
e.UseEntityService<CachedProductService>();
```

### Hierarchical Data (Self-referencing)

```csharp
public class Category : IEntityWithSerial, IHasTitle
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? Children { get; set; }
}

// In QueryBuilder:
if (so?.ParentId != null)
    query = query.Where(x => x.ParentId == so.ParentId);
if (so?.RootOnly == true)
    query = query.Where(x => x.ParentId == null);
```

---

## Response Types

All base controller endpoints return standardized wrappers:

```csharp
// GET /api/entities/{id}
DetailsResult<TDto>  { TDto Item; long? Duration; }

// GET /api/entities
ListResult<TDto>     { IList<TDto> Items; long? Duration; }

// GET /api/entities/search  (Items + Count for pagination)
SearchResult<TDto>   { IList<TDto> Items; long Count; long? Duration; }

// POST / PUT / POST save
SaveResult<TDto>     { TDto Item; bool IsNew; int Affected; long? Duration; }

// DELETE /api/entities/{id}
DeleteResult<TDto>   { TDto Item; long? Duration; }
```

---

## Best Practices

### Entity Design
- Keep entities as POCOs — data only, no business logic
- Use data annotations (`[Required]`, `[MaxLength]`, `[Range]`) directly on entity properties
- Use `SetDecimalPrecisionConvention` in DbContext instead of setting precision per property
- Prefer `ICollection<TKey>` over a single `TKey` for FK filter properties in SearchObjects

### Service Configuration
- Global services execute before entity-specific services — order matters
- Use extension methods per entity for clean, composable DI registration
- Use inline config for simple logic; separate classes for complex logic or when DI is needed

### Controller Design
- Never expose entity classes directly in API responses — always use DTOs
- Add custom controller actions only when base methods are insufficient
- Extend `SearchObject` to add filtering rather than creating extra endpoints

### DTO Strategy
- Include `Id` in `InputDto` to support the Save (upsert) action
- Exclude auto-generated fields (`Created`, `LastModified`, `NormalizedContent`) from `InputDto`
- Only include child collections in `InputDto` when they are configured with `e.Related(...)`

### Database
- Always use migrations for schema changes
- Add indexes on foreign keys and frequently filtered columns
- Use `AddAutoTruncateInterceptors()` to prevent string truncation exceptions

---

## Quick Reference: Built-in Entity Interfaces

| Interface | Properties | Related Services |
|-----------|-----------|-----------------|
| `IEntity<TKey>` | `Id (TKey)` | `FilterIdsQueryBuilder` |
| `IEntityWithSerial` | `Id (int)` | *(same as `IEntity<int>`)* |
| `IHasCode` | `Code (string)` | Normalizers |
| `IHasTitle` | `Title (string)` | Normalizers, `FilterTitle` |
| `IHasDescription` | `Description (string)` | Normalizers |
| `IHasNormalizedContent` | `NormalizedContent (string)` | `FilterHasNormalizedContentQueryBuilder` |
| `IHasCreated` | `Created (DateTime)` | `HasCreatedDbPrimer`, `FilterHasCreatedQueryBuilder` |
| `IHasLastModified` | `LastModified (DateTime?)` | `HasLastModifiedDbPrimer`, `FilterHasLastModifiedQueryBuilder` |
| `IHasTimestamps` | `Created, LastModified` | Both timestamp services |
| `IArchivable` | `IsArchived (bool)` | `ArchivablePrimer`, `FilterArchivablesQueryBuilder` |
| `ISortable` | `SortOrder (int)` | `RelatedCollectionPrepper`, `EntityExtensions.SetSortOrder` |
| `IHasObjectId<TKey>` | `ObjectId (TKey)` | Attachments |
| `IHasAttachments` | `HasAttachment, Attachments` | Attachments module |

## Quick Reference: Built-in Services

### Global Filter Query Builders (registered via `UseDefaults()` or manually)

| Class | Applies to | Filters on |
|-------|-----------|-----------|
| `FilterIdsQueryBuilder` | All entities | `Id`, `Ids`, `Exclude` |
| `FilterArchivablesQueryBuilder` | `IArchivable` | `IsArchived` |
| `FilterHasCreatedQueryBuilder` | `IHasCreated` | `MinCreated`, `MaxCreated` |
| `FilterHasLastModifiedQueryBuilder` | `IHasLastModified` | `MinLastModified`, `MaxLastModified` |
| `FilterHasNormalizedContentQueryBuilder` | `IHasNormalizedContent` | `Q` keyword search |

### Primers (registered via `UseDefaults()` or manually)

| Class | Applies to | Behaviour |
|-------|-----------|-----------|
| `HasCreatedDbPrimer` | `IHasCreated` | Sets `Created` on insert |
| `HasLastModifiedDbPrimer` | `IHasLastModified` | Sets `LastModified` on update |
| `ArchivablePrimer` | `IArchivable` | Soft-delete: sets `IsArchived = true` |
| `AutoTruncatePrimer` | All entities | Truncates strings to `[MaxLength]` |

### Normalizer Services (registered via `UseDefaults()` or `AddDefaultEntityNormalizer()`)

| Interface | Implementation | Role |
|-----------|---------------|------|
| `INormalizer` | `DefaultNormalizer` | Normalizes a string value |
| `IObjectNormalizer` | `DefaultObjectNormalizer` | Processes `[Normalized]` attributes |
| `IEntityNormalizer` | `DefaultEntityNormalizer<IEntity>` | Orchestrates entity normalization |
| `IQKeywordHelper` | `QKeywordHelper` | Parses Q search strings with wildcard support |

---

## Complete File Example (Product + Category)

### Project Structure (Per-Entity, Recommended)
```
YourProject/
├── YourProject.csproj
├── NuGet.Config
├── appsettings.json
├── Program.cs
├── Data/
│   └── AppDbContext.cs
├── Entities/
│   ├── Categories/
│   │   ├── Category.cs
│   │   ├── CategorySearchObject.cs
│   │   ├── CategoryDto.cs
│   │   ├── CategoryInputDto.cs
│   │   └── CategoryServiceCollectionExtensions.cs
│   └── Products/
│       ├── Product.cs
│       ├── ProductSearchObject.cs
│       ├── ProductSortBy.cs
│       ├── ProductIncludes.cs
│       ├── ProductDto.cs
│       ├── ProductInputDto.cs
│       ├── ProductQueryBuilder.cs
│       └── ProductServiceCollectionExtensions.cs
├── Controllers/
│   ├── CategoriesController.cs
│   └── ProductsController.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### Entities/Categories/Category.cs
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

public class Category : IEntityWithSerial, IHasTitle
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;

    public ICollection<Product>? Products { get; set; }
}
```

### Entities/Categories/CategorySearchObject.cs
```csharp
using Regira.Entities.Models;

public class CategorySearchObject : SearchObject
{
    // Inherits: Id, Ids, Exclude, Q, MinCreated, MaxCreated, IsArchived
}
```

### Entities/Categories/CategoryDto.cs
```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ProductCount { get; set; }
}
```

### Entities/Categories/CategoryInputDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

public class CategoryInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
}
```

### Entities/Categories/CategoryServiceCollectionExtensions.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

public static class CategoryServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddCategories<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<Category, CategorySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Q))
                    query = query.Where(x => EF.Functions.Like(x.Title, $"%{so.Q}%"));
                return query;
            });

            e.UseMapping<CategoryDto, CategoryInputDto>()
                .After((category, dto) =>
                {
                    dto.ProductCount = category.Products?.Count ?? 0;
                });
        });

        return services;
    }
}
```

### Entities/Products/Product.cs
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IArchivable, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public Category? Category { get; set; }
}
```

### Entities/Products/ProductSearchObject.cs
```csharp
using Regira.Entities.Models;

public class ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### Entities/Products/ProductSortBy.cs
```csharp
public enum ProductSortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Price,
    PriceDesc,
    Created,
    CreatedDesc
}
```

### Entities/Products/ProductIncludes.cs
```csharp
[Flags]
public enum ProductIncludes
{
    Default = 0,
    Category = 1 << 0,
    All = Category
}
```

### Entities/Products/ProductDto.cs
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
```

### Entities/Products/ProductInputDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

public class ProductInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
}
```

### Entities/Products/ProductQueryBuilder.cs
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId));

        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);

        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);

        return query;
    }
}
```

### Entities/Products/ProductServiceCollectionExtensions.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

public static class ProductServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddProducts<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            e.AddQueryFilter<ProductQueryBuilder>();

            e.SortBy((query, sortBy) =>
            {
                if (query is IOrderedQueryable<Product> sorted)
                    return sortBy switch
                    {
                        ProductSortBy.Title    => sorted.ThenBy(x => x.Title),
                        ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                        ProductSortBy.Price    => sorted.ThenBy(x => x.Price),
                        ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
                        _                      => sorted.ThenByDescending(x => x.Created)
                    };

                return sortBy switch
                {
                    ProductSortBy.Title     => query.OrderBy(x => x.Title),
                    ProductSortBy.TitleDesc  => query.OrderByDescending(x => x.Title),
                    ProductSortBy.Price     => query.OrderBy(x => x.Price),
                    ProductSortBy.PriceDesc  => query.OrderByDescending(x => x.Price),
                    ProductSortBy.Created   => query.OrderBy(x => x.Created),
                    ProductSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
                    _                       => query.OrderByDescending(x => x.Created)
                };
            });

            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(ProductIncludes.Category) == true)
                    query = query.Include(x => x.Category);
                return query;
            });

            e.UseMapping<ProductDto, ProductInputDto>()
                .After((product, dto) =>
                {
                    dto.CategoryTitle = product.Category?.Title;
                });
        });

        return services;
    }
}
```

### Controllers/CategoriesController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : EntityControllerBase<Category, CategorySearchObject, CategoryDto, CategoryInputDto>
{
}
```

### Controllers/ProductsController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<
    Product,
    ProductSearchObject,
    ProductSortBy,
    ProductIncludes,
    ProductDto,
    ProductInputDto>
{
}
```

### Data/AppDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Extensions/ServiceCollectionExtensions.cs
```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddProducts();

        return services;
    }
}
```

---

## Troubleshooting

| Problem | Likely Cause | Fix |
|---------|-------------|-----|
| Navigation properties not loaded | Missing `Includes` config or wrong flag | Check `e.Includes(...)` and that the client sends the correct `includes` flag |
| Filter not applied | Query builder not registered or wrong `SearchObject` property name | Verify `e.AddQueryFilter<>()` or `e.Filter(...)` and check property names |
| Mapping errors | Mapster/AutoMapper not configured or property name mismatch | Ensure `options.UseMapsterMapping()` is called; check DTO property names |
| Normalizer not running | `AddNormalizerInterceptors(sp)` missing or wrong overload | Use `(sp, options) =>` factory overload in `AddDbContext` |
| Primers not running | `AddPrimerInterceptors(sp)` missing | Same as above |
| Save not persisting | `SaveChanges()` not called | The base controller calls it; custom code must call it explicitly |
| Soft delete not working | `IArchivable` not implemented or `ArchivablePrimer` not registered | Check entity implements `IArchivable`; use `UseDefaults()` |
| `AddPrimerInterceptors` has no overload taking 0 args | Missing `IServiceProvider` | Use `AddDbContext<T>((sp, options) => ...)` and pass `sp` |
| `EntityWrappingServiceBase` — infinite loop | Inner service is the wrapper itself | Ensure `UseEntityService<T>()` registers the wrapper; `AddTransient` registers the interface |