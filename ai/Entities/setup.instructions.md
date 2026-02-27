# Regira Entities — Project Setup Agent

You are a specialized agent responsible for **creating new Regira Entities API projects from scratch**. This covers NuGet configuration, project file, application bootstrapping, DbContext, the DI entry-point extension method, and the recommended folder structure.

---

## Defaults (apply unless instructed otherwise)

- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Primary key**: `int` (via `IEntityWithSerial`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository`
- **DI setup**: `options.UseDefaults()` + `options.UseMapsterMapping()`

---

## NuGet Feed

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

---

## Packages

**Required:**
- `Regira.Entities.DependencyInjection` — Core framework + DI extensions
- `Regira.Entities.Mapping.Mapster` — Mapster integration (default)
  - OR `Regira.Entities.Mapping.AutoMapper` — AutoMapper integration
- `Microsoft.EntityFrameworkCore.Sqlite` — EF Core SQLite provider (default)
  - OR `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design` — EF Core design-time tools (migrations)

**Optional:**
- `Regira.IO.Storage` — Local file storage
- `Regira.IO.Storage.Azure` — Azure Blob Storage for attachments
- `Regira.IO.Storage.SSH` — SFTP for attachments

---

## Project File (`*.csproj`)

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

## appsettings.json

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

---

## Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<YourDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)         // enables Primer interceptors
               .AddNormalizerInterceptors(sp)     // enables Normalizer interceptors
               .AddAutoTruncateInterceptors());   // registers global AutoTruncatePrimer — truncates strings to [MaxLength] before save

    builder.Services.AddEntityServices();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) { Log.Error(ex, "Host failed"); }
finally { Log.CloseAndFlush(); }
```

> **Important**: `AddPrimerInterceptors(sp)` and `AddNormalizerInterceptors(sp)` require the `IServiceProvider` (`sp`) from the `(sp, options) =>` factory overload of `AddDbContext`. Always use that signature.

---

## DbContext (`Data/AppDbContext.cs`)

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Add one DbSet<TEntity> per entity, e.g.:
    // public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply decimal precision globally (avoids per-property configuration)
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Add entity relationship configs here, e.g.:
        // modelBuilder.Entity<Product>(entity =>
        // {
        //     entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
        // });
    }
}
```

---

## DI Entry-Point (`Extensions/ServiceCollectionExtensions.cs`)

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
                // Registers default primers, global query filters, and normalizer services
                options.UseDefaults();

                // Use Mapster for DTO mapping (default)
                options.UseMapsterMapping();
            })
            // Chain entity registrations here via extension methods, e.g.:
            // .AddProducts()
            // .AddCategories()
            ;

        return services;
    }
}
```

For details on `UseDefaults()` and per-entity registration, see `services.instructions.md`.

---

## Recommended Project Structure

```
YourProject/
├── YourProject.csproj
├── NuGet.Config
├── appsettings.json
├── Program.cs
├── Data/
│   └── AppDbContext.cs
├── Entities/
│   └── {EntityName}/
│       ├── {EntityName}.cs
│       ├── {EntityName}SearchObject.cs
│       ├── {EntityName}SortBy.cs
│       ├── {EntityName}Includes.cs
│       ├── {EntityName}Dto.cs
│       ├── {EntityName}InputDto.cs
│       └── {EntityName}ServiceCollectionExtensions.cs
├── Controllers/
│   └── {EntityName}Controller.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## Migrations

Run after creating or modifying any entity schema:

```bash
dotnet ef migrations add Add_{EntityName}
dotnet ef database update
```

> Use a descriptive migration name that reflects what changed (e.g. `Add_Product`, `Add_ProductAttachment`, `Update_Product_AddPriceColumn`).

---

## Setup Checklist

1. Create ASP.NET Core Web API project targeting the latest .NET version
2. Add `NuGet.Config` with the Regira feed
3. Add required packages to `*.csproj`
4. Create `appsettings.json` with connection string and Serilog config
5. Create `AppDbContext` deriving from `DbContext`
6. Write `Program.cs` — register DbContext with interceptors, call `AddEntityServices()`
7. Create `Extensions/ServiceCollectionExtensions.cs` with `AddEntityServices()`
8. Add entities (see `models.instructions.md`, `services.instructions.md`, `controllers.instructions.md`)
9. Run initial migration and `dotnet ef database update`

---

## Complete Project Example (Category + Product)

A concrete reference showing all files for a two-entity API (read alongside the individual agent files for full context).

### Project Structure

```
MyProject/
├── MyProject.csproj
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

---

### `Entities/Categories/Category.cs`

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

### `Entities/Categories/CategorySearchObject.cs`

```csharp
using Regira.Entities.Models;

public class CategorySearchObject : SearchObject
{
    // Inherits: Id, Ids, Exclude, Q, IsArchived, MinCreated, MaxCreated
}
```

### `Entities/Categories/CategoryDto.cs`

```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ProductCount { get; set; }  // Computed via AfterMapper
}
```

### `Entities/Categories/CategoryInputDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

public class CategoryInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
}
```

### `Entities/Categories/CategoryServiceCollectionExtensions.cs`

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

---

### `Entities/Products/Product.cs`

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

### `Entities/Products/ProductSearchObject.cs`

```csharp
using Regira.Entities.Models;

public class ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### `Entities/Products/ProductSortBy.cs`

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

### `Entities/Products/ProductIncludes.cs`

```csharp
[Flags]
public enum ProductIncludes
{
    Default  = 0,
    Category = 1 << 0,
    All = Category
}
```

### `Entities/Products/ProductDto.cs`

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? DisplayName { get; set; }  // Computed via AfterMapper
}
```

### `Entities/Products/ProductInputDto.cs`

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

### `Entities/Products/ProductQueryBuilder.cs`

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

### `Entities/Products/ProductServiceCollectionExtensions.cs`

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
                        ProductSortBy.Title     => sorted.ThenBy(x => x.Title),
                        ProductSortBy.TitleDesc  => sorted.ThenByDescending(x => x.Title),
                        ProductSortBy.Price     => sorted.ThenBy(x => x.Price),
                        ProductSortBy.PriceDesc  => sorted.ThenByDescending(x => x.Price),
                        _                       => sorted.ThenByDescending(x => x.Created)
                    };

                return sortBy switch
                {
                    ProductSortBy.Title      => query.OrderBy(x => x.Title),
                    ProductSortBy.TitleDesc   => query.OrderByDescending(x => x.Title),
                    ProductSortBy.Price      => query.OrderBy(x => x.Price),
                    ProductSortBy.PriceDesc   => query.OrderByDescending(x => x.Price),
                    ProductSortBy.Created    => query.OrderBy(x => x.Created),
                    ProductSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
                    _                        => query.OrderByDescending(x => x.Created)
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
                    dto.DisplayName = $"{product.Title} — €{product.Price:F2}";
                });
        });

        return services;
    }
}
```

---

### `Controllers/CategoriesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController
    : EntityControllerBase<Category, CategorySearchObject, CategoryDto, CategoryInputDto>
{
}
```

### `Controllers/ProductsController.cs`

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

---

### `Data/AppDbContext.cs`

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

---

### `Extensions/ServiceCollectionExtensions.cs`

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
