# Regira Entities AI Agent Instructions

You are an expert .NET developer specializing in the Regira Entities framework. Your role is to help create new API projects and add/modify entities in existing projects using the Regira Entities framework.

## NuGet Packages

**Package Source:**
```xml
<add key="Regira" value="https://packages.regira.com/v3/index.json" />
```

**Required Packages:**
- `Regira.Entities.DependencyInjection` - Core entities framework with DI extensions
- `Regira.Entities.Mapping.Mapster` - Mapster integration (recommended)
  - OR `Regira.Entities.Mapping.AutoMapper` - AutoMapper integration
- `Microsoft.EntityFrameworkCore.SqlServer` - EF Core SQL Server provider
- `Microsoft.EntityFrameworkCore.Design` - EF Core design-time tools

**Optional Packages:**
- `Regira.IO.Storage` - File storage for local file system or SFTP
- `Regira.IO.Storage.Azure` - Azure Blob Storage support for attachments

**Add NuGet.Config to project:**
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

## Namespace Reference

**Always include these using statements when implementing Regira Entities components:**

### Core Entity Namespaces
```csharp
using Regira.Entities.Models;                             // SearchObject, EntitySortBy
using Regira.Entities.Models.Abstractions;                // IEntity<TKey>, IEntityWithSerial, IHasTimestamps, etc.
using Regira.Entities.Services.Abstractions;              // IEntityService, query builders, processors
using Regira.Entities.Web.Controllers.Abstractions;       // EntityControllerBase
```

### Dependency Injection
```csharp
using Regira.Entities.DependencyInjection;       // UseEntities(), For<T>() extensions
```

### Mapping
```csharp
using Regira.Entities.Mapping.Mapster;           // UseMapsterMapping()
// OR
using Regira.Entities.Mapping.AutoMapper;        // UseAutoMapper()
```

### Normalization
```csharp
using Regira.Normalizing;                                 // NormalizedAttribute
using Regira.Normalizing.Abstractions;                    // INormalizer
using Regira.Entities.EFcore.Normalizing;                 // DefaultEntityNormalizer
using Regira.Entities.EFcore.Normalizing.Abstractions;    // IEntityNormalizer
```

### Attachments (when used)
```csharp
using Regira.Entities.Attachments.Models;                 // EntityAttachment<TKey, TObjectKey>
using Regira.Entities.Web.Attachments.Abstractions;       // EntityAttachmentControllerBase
using Regira.IO.Storage.FileSystem;                       // BinaryFileService, FileSystemOptions
using Regira.IO.Storage.Abstractions;                     // IFileService
// For Azure Blob Storage:
using Regira.IO.Storage.Azure;                            // BinaryBlobService, AzureOptions
```

### EF Core Extensions
```csharp
using Regira.DAL.EFcore.Extensions;                       // AddPrimerInterceptors, AddNormalizerInterceptors, SetDecimalPrecisionConvention, AddAutoTruncateInterceptors
using Regira.DAL.EFcore.Services;                         // AutoTruncateDbContextInterceptor
using Regira.Entities.EFcore.Primers;                     // AutoTruncatePrimer, HasCreatedDbPrimer, HasLastModifiedDbPrimer, ArchivablePrimer
using Regira.Entities.EFcore.Primers.Abstractions;        // EntityPrimerBase, IEntityPrimer
using Regira.DAL.Paging;                                  // PagingInfo
```

### Built-in Query Builders
```csharp
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;  // FilterIdsQueryBuilder, FilterArchivablesQueryBuilder, etc.
```

### Common System Namespaces
```csharp
using System.ComponentModel.DataAnnotations;     // [Required], [MaxLength]
using Microsoft.EntityFrameworkCore;             // DbContext, Include(), OrderBy()
using Microsoft.AspNetCore.Mvc;                  // [ApiController], [Route]
```

## Core Understanding

### Framework Architecture

**Regira Entities** is a generic, extensible framework for managing data entities in .NET applications with standardized CRUD operations.

**Key Components:**
- **Entity Models**: POCO classes implementing `IEntity<TKey>`
- **Services**: `IEntityService` (default: `EntityRepository` with DbContext)
- **Controllers**: API endpoints inheriting from `EntityControllerBase`
- **DTOs**: Separate read (TDto) and write (TInputDto) models
- **Pipeline Services**: QueryBuilders, Processors, Preppers, Primers, AfterMappers

### Generic Type System

| Type | Required | Purpose | Example |
|------|----------|---------|---------|
| TEntity | ✓ | The entity class | `Product` |
| TKey | ✓* | Primary key type (*default: int) | `Guid`, `int` |
| TSearchObject | ○ | Advanced filtering | `ProductSearchObject` |
| TSortBy | ○ | Sorting enum | `ProductSortBy` |
| TInclude | ○ | Navigation properties enum | `ProductIncludes` |
| TDto | ○ | Read/display model | `ProductDto` |
| TInputDto | ○ | Create/update model | `ProductInputDto` |

### Processing Pipelines

**Read Pipeline:**
1. EntitySet → QueryBuilders (Filters, Sorting, Paging, Includes) → Processors → Mapping → AfterMapping*

**Write Pipeline:**
1. Input → Mapping → AfterMapping* → Preppers → SaveChanges (Primers) → Submit

*Only executed in API controllers

## Decision-Making Guidelines

### When to Use Inline vs Separate Classes

**Use INLINE configuration when:**
- Simple logic (< 10 lines)
- Entity-specific, not reusable
- Rapid prototyping
- Single filter condition

**Use SEPARATE classes when:**
- Complex logic (> 10 lines)
- Reusable across entities
- Multiple conditions/operations
- Team needs explicit, testable classes
- Production-quality code

### Choosing Base Controller

**Simple entities (no advanced features):**
```csharp
EntityControllerBase<TEntity, TDto, TInputDto>
```

**With filtering:**
```csharp
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>
```

**Full featured (recommended for complex scenarios):**
```csharp
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

### Interface Selection for Entities

**Always implement:**
- `IEntity<TKey>` (or `IEntityWithSerial` for int ID with auto-increment)

**Implement when needed:**
- `IHasTimestamps` - Track creation and modification times
- `IArchivable` - Soft delete functionality
- `IHasTitle` - Entities with display name
- `IHasDescription` - Entities with description text
- `IHasCode` - Entities with short identifier code
- `ISortable` - For sortable child collections
- `IHasAttachments` - When entity needs file attachments

### Service Layer Decisions

**Use default EntityRepository when:**
- Standard CRUD is sufficient
- Using EF Core DbContext
- No complex business logic

**Create custom service when:**
- Complex business rules
- Multiple data sources
- External API integration
- Advanced validation logic
- Need to wrap/extend repository

## Project Creation Workflow

### Step 1: Initial Setup

Create project structure with these templates:

**Project File:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
	</PropertyGroup>
	<ItemGroup>
		<!-- Core packages -->
		<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.0.0" />
		<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
		<PackageReference Include="Scalar.AspNetCore" Version="2.11.1" />
		
		<!-- Logging -->
		<PackageReference Include="Serilog.Extensions.Hosting" Version="10.0.0" />
		<PackageReference Include="Serilog.Settings.Configuration" Version="10.0.0" />
		<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
		<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
		
		<!-- Regira Entities (from https://packages.regira.com/v3/index.json) -->
		<PackageReference Include="Regira.Entities.DependencyInjection" Version="*" />
		<PackageReference Include="Regira.Entities.Mapping.Mapster" Version="*" />
		<!-- Optional: For attachments -->
		<!-- <PackageReference Include="Regira.IO.Storage" Version="*" /> -->
		
		<!-- Entity Framework -->
		<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
	</ItemGroup>
</Project>
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourDbName;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "AllowedHosts": "*",
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
          "path": "logs/YourApp-.log",
          "restrictedToMinimumLevel": "Information",
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

**Program.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Regira.DAL.EFcore.Extensions;
using YourNamespace.Data;
using YourNamespace.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Controllers
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });
    
    // Logging
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
    
    // OpenApi
    builder.Services.AddOpenApi();
    
    // DbContext
    builder.Services.AddDbContext<YourDbContext>((sp, options) =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors();
    });
    
    // Regira Entities
    builder.Services.AddEntityServices();
    
    var app = builder.Build();
    
    // OpenApi
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

### Step 2: Create DbContext

**Basic DbContext:**
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace YourNamespace.Data;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    
    // DbSets will be added here
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set decimal precision globally
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        // Entity configurations will be added here
    }
}
```

### Step 3: Create Extension Method for DI

**Create `ServiceCollectionExtensions.cs`:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Services;
using YourNamespace.Data;

namespace YourNamespace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.UseEntities<YourDbContext>(options =>
        {
            // Global configuration
            options.UseMapsterMapping();
            options.AddDefaultEntityNormalizer();
            
            // Global filters
            options.AddGlobalFilterQueryBuilder<FilterIdsQueryBuilder<int>>();
            options.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>();
            
            // Global primers
            options.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());
        });
        
        return services;
    }
}
```

## Entity Implementation Workflow

### Step 1: Create Entity Model

**Template:**
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace YourNamespace.Entities;

public class YourEntity : IEntity<int>, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // Normalized content for searching
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    
    // Timestamps
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Add your properties here
    
    // Navigation properties
}
```

**Decision checklist:**
- Use `int` for ID unless specific reason for `Guid` (distributed systems, security)
- Add `IHasTimestamps` for audit trail (recommended for most entities)
- Add `IArchivable` for soft delete capability
- Add `IHasTitle` and/or `IHasDescription` for searchable entities
- Use `[Normalized]` attribute when normalization is needed for search
- Use appropriate validation attributes (`[Required]`, `[MaxLength]`, etc.)

### Step 2: Create SearchObject (Optional but Recommended)

**Template:**
```csharp
using Regira.Entities.Models;

namespace YourNamespace.Models;

public class YourEntitySearchObject : SearchObject
{
    // Add custom filter properties
    public int? CategoryId { get; set; }
    public ICollection<int>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Q property inherited for text search
    // Id, Ids, Exclude inherited for ID filtering
    // Created/LastModified range filters inherited
}
```

**When to create:**
- Entity has filterable properties beyond ID/timestamps
- Users need to search/filter results
- Complex query scenarios

**Skip when:**
- Simple entity with no filtering needs
- Only ID-based lookups required

### Step 3: Create Enums (Optional)

**SortBy Enum:**
```csharp
namespace YourNamespace.Models;

public enum YourEntitySortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Created,
    CreatedDesc,
    Price,
    PriceDesc
}
```

**Includes Enum:**
```csharp
using System;

namespace YourNamespace.Models;

[Flags]
public enum YourEntityIncludes
{
    Default = 0,
    Category = 1 << 0,
    RelatedItems = 1 << 1,
    All = Category | RelatedItems
}
```

**When to create:**
- Custom sorting beyond default (ID, Created, LastModified)
- Navigation properties that should be optionally loaded
- API consumers need control over data loading

### Step 4: Create DTOs

**Output DTO (for reading):**
```csharp
namespace YourNamespace.Models;

public class YourEntityDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Add computed or flattened properties
    public string? CategoryName { get; set; }
    public int RelatedItemsCount { get; set; }
}
```

**Input DTO (for creating/updating):**
```csharp
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models;

public class YourEntityInputDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public int CategoryId { get; set; }
    
    // Do NOT include: Id, Created, LastModified, computed properties
}
```

**DTO Guidelines:**
- Output DTO: Include all displayable data, flatten navigation properties
- Input DTO: Only editable fields, exclude auto-generated fields
- Use validation attributes on Input DTO
- Keep DTOs in Models namespace separate from Entities

### Step 5: Create Query Builders

**Option A: Inline (for simple logic):**
```csharp
// In DI configuration
e.Filter((query, so) =>
{
    if (so?.CategoryId != null)
        query = query.Where(x => x.CategoryId == so.CategoryId);
    if (so?.MinPrice != null)
        query = query.Where(x => x.Price >= so.MinPrice);
    return query;
});
```

**Option B: Separate Class (for complex logic):**
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using YourNamespace.Entities;
using YourNamespace.Models;

namespace YourNamespace.Services;

public class YourEntityQueryBuilder : FilteredQueryBuilderBase<YourEntity, int, YourEntitySearchObject>
{
    public override IQueryable<YourEntity> Build(IQueryable<YourEntity> query, YourEntitySearchObject? so)
    {
        if (so == null) return query;
        
        // Category filtering
        if (so.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == so.CategoryId.Value);
        
        if (so.CategoryIds?.Any() == true)
            query = query.Where(x => so.CategoryIds.Contains(x.CategoryId));
        
        // Price range
        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);
        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);
        
        return query;
    }
}
```

**Sorting:**
```csharp
// Inline
e.SortBy((query, sortBy) =>
{
    return sortBy switch
    {
        YourEntitySortBy.Title => query.OrderBy(x => x.Title),
        YourEntitySortBy.TitleDesc => query.OrderByDescending(x => x.Title),
        YourEntitySortBy.Price => query.OrderBy(x => x.Price),
        YourEntitySortBy.PriceDesc => query.OrderByDescending(x => x.Price),
        _ => query.OrderByDescending(x => x.Created)
    };
});
```

**Includes:**
```csharp
// Inline
e.Includes((query, includes) =>
{
    if (includes?.HasFlag(YourEntityIncludes.Category) == true)
        query = query.Include(x => x.Category);
    if (includes?.HasFlag(YourEntityIncludes.RelatedItems) == true)
        query = query.Include(x => x.RelatedItems);
    return query;
});
```

### Step 6: Create Processors (Optional)

**When to use:**
- Calculate `[NotMapped]` properties
- Set display values after fetching
- Enrich data from external sources

```csharp
// Inline
e.Process((items, includes) =>
{
    foreach (var item in items)
    {
        item.DisplayPrice = $"${item.Price:F2}";
        item.IsOnSale = item.SalePrice < item.Price;
    }
    return Task.CompletedTask;
});

// Separate class
using Regira.Entities.Services.Abstractions;
using YourNamespace.Entities;
using YourNamespace.Models;

namespace YourNamespace.Services;

public class YourEntityProcessor : IEntityProcessor<YourEntity, YourEntityIncludes>
{
    public async Task Process(IList<YourEntity> items, YourEntityIncludes? includes)
    {
        foreach (var item in items)
        {
            // Process logic
        }
        await Task.CompletedTask;
    }
}
```

### Step 7: Create Preppers (Optional)

**When to use:**
- Prepare entities before saving
- Handle child collections
- Set calculated fields
- Generate codes/identifiers

```csharp
// Inline
e.Prepare(item =>
{
    item.Sku ??= GenerateSku(item);
    item.UpdatedBy = currentUserId;
});

// With DbContext access
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.Items?.Sum(x => x.Amount) ?? 0;
    await Task.CompletedTask;
});

// For child collections
e.Related(x => x.OrderItems, (item, _) => item.OrderItems?.Prepare());
```

### Step 8: Configure Controller

**Template:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using YourNamespace.Entities;
using YourNamespace.Models;

namespace YourNamespace.Controllers;

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
    // Add custom actions only when necessary
    // Base controller provides: Details, List, Search, Create, Modify, Save, Delete
}
```

**Choosing controller base:**
- Minimal: `EntityControllerBase<TEntity, TDto, TInputDto>`
- Standard: `EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>`
- Full: `EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>`

### Step 9: Configure Dependency Injection

**Add to `ServiceCollectionExtensions.cs`:**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection;
using Regira.Entities.Mapping.Mapster;
using YourNamespace.Data;
using YourNamespace.Entities;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.UseEntities<YourDbContext>(options =>
        {
            // Global configuration...
        })
        .For<YourEntity, YourEntitySearchObject, YourEntitySortBy, YourEntityIncludes>(e =>
        {
            // Query builders
            e.AddQueryFilter<YourEntityQueryBuilder>(); // Separate class
            // OR
            e.Filter((query, so) => { /* inline */ }); // Inline
            
            // Sorting
            e.SortBy((query, sortBy) =>
            {
                return sortBy switch
                {
                    YourEntitySortBy.Title => query.OrderBy(x => x.Title),
                    _ => query.OrderByDescending(x => x.Created)
                };
            });
            
            // Includes
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(YourEntityIncludes.Category) == true)
                    query = query.Include(x => x.Category);
                return query;
            });
            
            // Mapping
            e.UseMapping<YourEntityDto, YourEntityInputDto>()
                .After((entity, dto) =>
                {
                    // AfterMapper: enrich DTO after mapping
                    dto.CategoryName = entity.Category?.Title;
                })
                .AfterInput((dto, entity) =>
                {
                    // Modify entity after mapping from input
                });
            
            // Processors
            e.Process((items, includes) =>
            {
                // Set [NotMapped] properties
                return Task.CompletedTask;
            });
            
            // Preppers
            e.Prepare(item =>
            {
                // Prepare before saving
            });
            
            // Child collections
            e.Related(x => x.ChildItems);
        });
        
        return services;
    }
}
```

### Step 10: Update DbContext

```csharp
public class YourDbContext : DbContext
{
    // Add DbSet
    public DbSet<YourEntity> YourEntities { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        // Configure relationships
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.HasOne(e => e.Category)
                .WithMany(c => c.YourEntities)
                .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Step 11: Create Migration

```bash
# Add migration
dotnet ef migrations add Add_YourEntity

# Update database
dotnet ef database update
```

## Advanced Features

### Attachments Implementation

**Prerequisites:**
Add to project file:
```xml
<PackageReference Include="Regira.IO.Storage" Version="*" />
<!-- OR for Azure Blob Storage: -->
<!-- <PackageReference Include="Regira.IO.Storage.Azure" Version="*" /> -->
```

**1. Create EntityAttachment model:**
```csharp
using Regira.Entities.Attachments.Models;

namespace YourNamespace.Entities;

public class YourEntityAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(YourEntity);
}
```

**2. Update entity to support attachments:**
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Normalization;

namespace YourNamespace.Entities;

public class YourEntity : IEntity<int>, IHasAttachments, IHasAttachments<YourEntityAttachment>
{
    // ... existing properties ...
    
    public bool? HasAttachment { get; set; }
    public ICollection<YourEntityAttachment>? Attachments { get; set; }
    
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<YourEntityAttachment>().ToArray();
    }
}
```

**3. Create controller:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;
using YourNamespace.Entities;

namespace YourNamespace.Controllers;

[ApiController]
[Route("api/your-entities/{objectId}/attachments")]
public class YourEntityAttachmentsController : EntityAttachmentControllerBase<YourEntityAttachment, int, int>
{
}
```

**4. Configure DI:**
```csharp
using Regira.Entities.DependencyInjection;
using Regira.IO.Storage.FileSystem;
using Regira.IO.Storage.Abstractions;

// In ServiceCollectionExtensions.cs
services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions { 
            RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        }
    ));

// For Azure Blob Storage:
using Regira.IO.Storage.Azure;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryBlobService(
        new AzureCommunicator(
            new AzureOptions
            {
                ConnectionString = configuration["Azure:StorageConnectionString"],
                ContainerName = "attachments"
            }
        )
    ));
```

**5. Update DbContext:**
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using YourNamespace.Entities;

namespace YourNamespace.Data;

public class YourDbContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<YourEntityAttachment> YourEntityAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configuration ...
        
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(e => e.ObjectId)
                .HasPrincipalKey(e => e.Id);
        });
    }
}
```

### Custom Normalizers

**When built-in normalization isn't enough:**

```csharp
using Regira.Normalizing.Abstractions;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using YourNamespace.Entities;

namespace YourNamespace.Services;

public class CustomEntityNormalizer : EntityNormalizerBase<YourEntity>
{
    private readonly INormalizer _normalizer;
    
    public CustomEntityNormalizer(INormalizer normalizer)
    {
        _normalizer = normalizer;
    }
    
    public override async Task HandleNormalize(YourEntity item)
    {
        // Custom normalization logic
        var content = $"{item.Title} {item.Description} {item.Tags}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);
        
        // Normalize phone number
        item.NormalizedPhone = NormalizePhone(item.Phone);
    }
    
    private string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return new string(phone.Where(char.IsDigit).ToArray());
    }
}

// Register in ServiceCollectionExtensions.cs
e.AddNormalizer<CustomEntityNormalizer>();
```

## Best Practices

### Entity Design
1. **Keep entities as POCOs** - data only, minimal behavior
2. **Use appropriate interfaces** - only implement what you need
3. **Use data annotations** - `[Required]`, `[MaxLength]`, etc.
4. **Prefer composition over inheritance** - use interfaces for shared behavior
5. **Navigation properties** - use virtual for lazy loading, non-virtual for explicit loading

### Service Configuration
1. **Global before specific** - global services execute first
2. **Extension methods** - create extension methods for cleaner DI registration
3. **Inline for simple** - use inline configuration for simple logic (<10 lines)
4. **Classes for complex** - create separate classes for reusable/complex logic
5. **Order matters** - services execute in registration order

### Controller Design
1. **Minimal controllers** - rely on base controller methods
2. **Custom actions sparingly** - only when base methods insufficient
3. **DTOs always** - never expose entities directly in API
4. **Consistent naming** - use plural for collection endpoints

### DTO Strategy
1. **Separate read/write** - different DTOs for input and output
2. **Flatten navigation** - include related data in output DTO
3. **Validation on input** - use data annotations on InputDto
4. **No IDs on input** - let system generate/manage IDs

### Performance
1. **Include judiciously** - only load navigation properties when needed
2. **Use projection** - select only required fields for lists
3. **Async all the way** - use async methods throughout
4. **Pagination** - always paginate large result sets

### Database
1. **Migrations** - always use migrations for schema changes
2. **Indexes** - add indexes on foreign keys and frequently filtered properties
3. **Decimal precision** - use `SetDecimalPrecisionConvention` globally
4. **Interceptors** - use Primers and Normalizers as interceptors

## Common Patterns

### Master-Detail Relationship
```csharp
// Master entity
.For<Order>(e =>
{
    e.Related(x => x.OrderItems, (order, _) => 
    {
        // Prepare child items
        order.OrderItems?.Prepare();
        // Set parent reference
        foreach (var item in order.OrderItems ?? [])
            item.OrderId = order.Id;
    });
    
    e.Prepare(item =>
    {
        // Recalculate totals
        item.TotalAmount = item.OrderItems?.Sum(x => x.Amount) ?? 0;
    });
});
```

### Hierarchical Data
```csharp
public class Category : IEntity<int>
{
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? Children { get; set; }
}

// In query builder
if (so?.ParentId != null)
    query = query.Where(x => x.ParentId == so.ParentId);
if (so?.RootOnly == true)
    query = query.Where(x => x.ParentId == null);
```

### Soft Delete
```csharp
// Entity implements IArchivable
public class Product : IEntity<int>, IArchivable
{
    public bool IsArchived { get; set; }
}

// Global configuration
options.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>();
options.AddPrimer<ArchivablePrimer>();

// SearchObject
public class ProductSearchObject : SearchObject
{
    public bool? IsArchived { get; set; } // null = exclude archived, true = only archived, false = only active
}
```

### Audit Trail
```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.DAL.EFcore.Services;

namespace YourNamespace.Entities;

// Entity
public class AuditableEntity : IEntity<int>, IHasTimestamps
{
    public int Id { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

namespace YourNamespace.Services;

// Custom primer for user tracking
public class UserTrackingPrimer : EntityPrimerBase<IAuditable>
{
    private readonly ICurrentUserService _currentUser;
    
    public UserTrackingPrimer(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }
    
    public override Task PrepareAsync(IAuditable entity, EntityEntry entry)
    {
        var userId = _currentUser.GetUserId();
        
        if (entry.State == EntityState.Added)
            entity.CreatedBy = userId;
        else if (entry.State == EntityState.Modified)
            entity.ModifiedBy = userId;
            
        return Task.CompletedTask;
    }
}

// In ServiceCollectionExtensions.cs
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection;

services.UseEntities<YourDbContext>(options =>
{
    // Global primers
    options.AddPrimer<HasCreatedDbPrimer>();
    options.AddPrimer<HasLastModifiedDbPrimer>();
    options.AddPrimer<UserTrackingPrimer>();
});
```

## Error Handling

### Input Validation
```csharp
using Regira.Entities.Exceptions;
using YourNamespace.Entities;

namespace YourNamespace.Services;

// In service or prepper
public class ProductValidator
{
    public void Validate(Product item)
    {
        if (item.Price < 0)
            throw new EntityInputException<Product>("Price cannot be negative")
            {
                Item = item,
                InputErrors = new Dictionary<string, string>
                {
                    [nameof(Product.Price)] = "Must be greater than or equal to 0"
                }
            };
    }
}
```

**Controller automatically catches EntityInputException and returns BadRequest(400)**

## Testing Considerations

### Unit Testing Services
- Mock `IEntityService`
- Test query builders independently
- Test processors with sample data
- Test preppers with entity state

### Integration Testing
- Use In-Memory database
- Test full pipelines
- Test endpoint responses
- Verify database state

## Migration Strategy

### Adding to Existing Project
1. Install Regira.Entities packages
2. Update DbContext with interceptors
3. Create entity configuration extension method
4. Migrate entities one at a time
5. Start with simplest entities
6. Gradually add complexity

### Updating Existing Entities
1. Implement required interfaces
2. Add SearchObject if needed
3. Create DTOs
4. Configure in DI
5. Update/create controller
6. Create migration
7. Test thoroughly

## Troubleshooting

### Common Issues

**Missing navigation properties:**
- Add includes configuration
- Check DbContext relationships
- Verify Include flags usage

**Filter not working:**
- Check SearchObject properties
- Verify query builder logic
- Check global filters aren't overriding

**Mapping errors:**
- Ensure Mapster/AutoMapper configured
- Check property name matching
- Verify custom mapping configuration

**Save not persisting:**
- Call `SaveChanges()` on repository
- Check Preppers aren't throwing
- Verify DbContext configuration

**Performance issues:**
- Review includes - loading too much?
- Add pagination
- Check for N+1 queries
- Add database indexes

## Complete File Examples

Here's a complete example showing all files with proper namespaces for a Product entity:

### Project Structure
```
YourProject/
├── YourProject.csproj
├── NuGet.Config
├── appsettings.json
├── Program.cs
├── Data/
│   └── YourDbContext.cs
├── Entities/
│   ├── Product.cs
│   └── Category.cs
├── Models/
│   ├── ProductSearchObject.cs
│   ├── ProductSortBy.cs
│   ├── ProductIncludes.cs
│   ├── ProductDto.cs
│   └── ProductInputDto.cs
├── Services/
│   └── ProductQueryBuilder.cs
├── Controllers/
│   └── ProductsController.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### NuGet.Config
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

### Entities/Product.cs
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace YourNamespace.Entities;

public class Product : IEntity<int>, IHasTimestamps, IHasTitle, IArchivable
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
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

### Models/ProductSearchObject.cs
```csharp
using Regira.Entities.Models;

namespace YourNamespace.Models;

public class ProductSearchObject : SearchObject
{
    public int? CategoryId { get; set; }
    public ICollection<int>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### Models/ProductSortBy.cs
```csharp
namespace YourNamespace.Models;

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

### Models/ProductIncludes.cs
```csharp
namespace YourNamespace.Models;

[Flags]
public enum ProductIncludes
{
    Default = 0,
    Category = 1 << 0,
    All = Category
}
```

### Models/ProductDto.cs
```csharp
namespace YourNamespace.Models;

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

### Models/ProductInputDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models;

public class ProductInputDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Range(0, 999999)]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
}
```

### Services/ProductQueryBuilder.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using YourNamespace.Entities;
using YourNamespace.Models;

namespace YourNamespace.Services;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;
        
        if (so.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == so.CategoryId.Value);
        
        if (so.CategoryIds?.Any() == true)
            query = query.Where(x => so.CategoryIds.Contains(x.CategoryId));
        
        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);
            
        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);
        
        return query;
    }
}
```

### Controllers/ProductsController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using YourNamespace.Entities;
using YourNamespace.Models;

namespace YourNamespace.Controllers;

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

### Data/YourDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using YourNamespace.Entities;

namespace YourNamespace.Data;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Extensions/ServiceCollectionExtensions.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Services;
using YourNamespace.Data;
using YourNamespace.Entities;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.UseEntities<YourDbContext>(options =>
        {
            options.UseMapsterMapping();
            options.AddDefaultEntityNormalizer();
            
            options.AddGlobalFilterQueryBuilder<FilterIdsQueryBuilder<int>>();
            options.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>();
        })
        .For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            e.AddQueryFilter<ProductQueryBuilder>();
            
            e.SortBy((query, sortBy) =>
            {
                return sortBy switch
                {
                    ProductSortBy.Title => query.OrderBy(x => x.Title),
                    ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                    ProductSortBy.Price => query.OrderBy(x => x.Price),
                    ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                    ProductSortBy.Created => query.OrderBy(x => x.Created),
                    ProductSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
                    _ => query.OrderByDescending(x => x.Created)
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

## Quick Reference: Built-in Interfaces

| Interface | Properties | Purpose |
|-----------|-----------|---------|
| `IEntity<TKey>` | Id | Primary key definition |
| `IEntityWithSerial` | Id (int) | Auto-incrementing int ID |
| `IHasCode` | Code | Short identifier code |
| `IHasTitle` | Title | Display name |
| `IHasDescription` | Description | Long text field |
| `IHasCreated` | Created | Creation timestamp |
| `IHasLastModified` | LastModified | Modification timestamp |
| `IHasTimestamps` | Created, LastModified | Both timestamps |
| `IArchivable` | IsArchived | Soft delete capability |
| `ISortable` | SortOrder | Collection ordering |
| `IHasAttachments` | HasAttachment, Attachments | File attachment support |

## Quick Reference: Built-in Services

### Global Filters
- `FilterIdsQueryBuilder` - Filter by ID collection
- `FilterArchivablesQueryBuilder` - Exclude archived items
- `FilterHasCreatedQueryBuilder` - Filter by creation date
- `FilterHasLastModifiedQueryBuilder` - Filter by modification date
- `FilterHasNormalizedContentQueryBuilder` - Text search on normalized content

### Primers
- `ArchivablePrimer` - Soft delete (sets IsArchived instead of deleting)
- `HasCreatedDbPrimer` - Sets Created timestamp
- `HasLastModifiedDbPrimer` - Sets LastModified timestamp
- `AutoTruncatePrimer` - Truncates strings to MaxLength (entity-specific)
- `AutoTruncateDbContextInterceptor` - Truncates strings to MaxLength (DbContext interceptor)

### Normalizers
- `DefaultNormalizer` - Removes diacritics,