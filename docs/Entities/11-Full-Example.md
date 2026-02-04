# Complete Webshop Example

This is a complete, working implementation of a simple webshop using the Regira Entities framework. It demonstrates all major features integrated together.

## Project Structure

```
WebShop/
├── Models/
│   ├── Entities/
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   └── ProductAttachment.cs
│   ├── DTOs/
│   │   ├── ProductDto.cs
│   │   ├── ProductInputDto.cs
│   │   ├── CategoryDto.cs
│   │   └── CategoryInputDto.cs
│   └── SearchObjects/
│       ├── ProductSearchObject.cs
│       └── CategorySearchObject.cs
├── Data/
│   └── ShopDbContext.cs
├── Services/
│   ├── QueryBuilders/
│   │   └── ProductQueryBuilder.cs
│   └── Normalizers/
│       └── ProductNormalizer.cs
├── Controllers/
│   ├── ProductsController.cs
│   ├── CategoriesController.cs
│   └── ProductAttachmentsController.cs
└── Program.cs
```

## 1. Entity Models

### Product.cs

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Abstractions;
using Regira.Entities.Models.Attributes;
using Regira.Entities.Attachments.Abstractions;

namespace WebShop.Models.Entities;

public class Product : IEntity<int>, IHasTimestamps, IArchivable, IHasTitle, IHasDescription,
    IHasAttachments, IHasAttachments<ProductAttachment>
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
    
    public int CategoryId { get; set; }
    
    // Normalization
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    
    // Timestamps (IHasTimestamps)
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Soft delete (IArchivable)
    public bool IsArchived { get; set; }
    
    // Attachments (IHasAttachments)
    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
    
    // Navigation properties
    public Category? Category { get; set; }
}
```

### Category.cs

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Abstractions;
using Regira.Entities.Models.Attributes;

namespace WebShop.Models.Entities;

public class Category : IEntity<int>, IHasTimestamps, IHasTitle, IHasDescription
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Normalization
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    
    // Timestamps
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Navigation
    public ICollection<Product>? Products { get; set; }
}
```

### ProductAttachment.cs

```csharp
using Regira.Entities.Attachments.Models;

namespace WebShop.Models.Entities;

public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}
```

## 2. Search Objects

### ProductSearchObject.cs

```csharp
using Regira.Entities.Models;

namespace WebShop.Models.SearchObjects;

public class ProductSearchObject : SearchObject
{
    // Category filtering
    public int? CategoryId { get; set; }
    public ICollection<int>? CategoryIds { get; set; }
    
    // Price filtering
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Stock filtering
    public int? MinStock { get; set; }
    public bool? InStock { get; set; }
}
```

### CategorySearchObject.cs

```csharp
using Regira.Entities.Models;

namespace WebShop.Models.SearchObjects;

public class CategorySearchObject : SearchObject
{
    // Uses default SearchObject properties (Q, IsArchived, Created, etc.)
}
```

## 3. Enums

### ProductSortBy.cs

```csharp
namespace WebShop.Models.Enums;

public enum ProductSortBy
{
    Default = 0,
    Title = 1 << 0,
    TitleDesc = 1 << 1,
    Price = 1 << 2,
    PriceDesc = 1 << 3,
    Stock = 1 << 4,
    StockDesc = 1 << 5,
    Created = 1 << 6,
    CreatedDesc = 1 << 7,
    Category = 1 << 8
}
```

### ProductIncludes.cs

```csharp
namespace WebShop.Models.Enums;

[Flags]
public enum ProductIncludes
{
    Default = 0,
    Category = 1 << 0,
    Attachments = 1 << 1,
    All = Category | Attachments
}
```

## 4. DTOs

### ProductDto.cs

```csharp
namespace WebShop.Models.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool InStock { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public bool HasAttachments { get; set; }
    public int AttachmentCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }
}
```

### ProductInputDto.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebShop.Models.DTOs;

public class ProductInputDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Range(0, 999999)]
    public int Stock { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
}
```

### CategoryDto.cs

```csharp
namespace WebShop.Models.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
```

### CategoryInputDto.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebShop.Models.DTOs;

public class CategoryInputDto
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
```

## 5. Database Context

### ShopDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Extensions;
using WebShop.Models.Entities;

namespace WebShop.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ProductAttachment> ProductAttachments => Set<ProductAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set decimal precision for all decimal properties
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => e.Created);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Title);
        });

        // ProductAttachment configuration
        modelBuilder.Entity<ProductAttachment>(entity =>
        {
            entity.HasOne(e => e.Attachment)
                .WithMany()
                .HasForeignKey(e => e.AttachmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.ObjectId);
            entity.HasIndex(e => e.AttachmentId);
        });

        // Attachment configuration
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasIndex(e => e.FileName);
        });
    }
}
```

## 6. Query Builders

### ProductQueryBuilder.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.QueryBuilders;
using WebShop.Models.Entities;
using WebShop.Models.SearchObjects;

namespace WebShop.Services.QueryBuilders;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;

        // Category filtering
        if (so.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == so.CategoryId.Value);
        }

        if (so.CategoryIds?.Any() == true)
        {
            query = query.Where(x => so.CategoryIds.Contains(x.CategoryId));
        }

        // Price filtering
        if (so.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= so.MinPrice.Value);
        }

        if (so.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= so.MaxPrice.Value);
        }

        // Stock filtering
        if (so.MinStock.HasValue)
        {
            query = query.Where(x => x.Stock >= so.MinStock.Value);
        }

        if (so.InStock == true)
        {
            query = query.Where(x => x.Stock > 0);
        }
        else if (so.InStock == false)
        {
            query = query.Where(x => x.Stock == 0);
        }

        return query;
    }
}
```

### ProductSortedQueryBuilder.cs

```csharp
using Regira.Entities.Services.QueryBuilders;
using WebShop.Models.Entities;
using WebShop.Models.Enums;

namespace WebShop.Services.QueryBuilders;

public class ProductSortedQueryBuilder : SortedQueryBuilderBase<Product, ProductSortBy>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, IList<ProductSortBy> sortBy)
    {
        foreach (var sort in sortBy)
        {
            query = sort switch
            {
                ProductSortBy.Title => query.OrderBy(x => x.Title),
                ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                ProductSortBy.Price => query.OrderBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                ProductSortBy.Stock => query.OrderBy(x => x.Stock),
                ProductSortBy.StockDesc => query.OrderByDescending(x => x.Stock),
                ProductSortBy.Created => query.OrderBy(x => x.Created),
                ProductSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
                ProductSortBy.Category => query.OrderBy(x => x.Category!.Title),
                _ => query.OrderBy(x => x.Id)
            };
        }
        
        return query;
    }
}
```

### ProductIncludesQueryBuilder.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.QueryBuilders;
using WebShop.Models.Entities;
using WebShop.Models.Enums;

namespace WebShop.Services.QueryBuilders;

public class ProductIncludesQueryBuilder : IncludesQueryBuilderBase<Product, ProductIncludes>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductIncludes? includes)
    {
        if (includes == null) return query;

        if (includes.Value.HasFlag(ProductIncludes.Category))
        {
            query = query.Include(x => x.Category);
        }

        if (includes.Value.HasFlag(ProductIncludes.Attachments))
        {
            query = query.Include(x => x.Attachments);
        }

        return query;
    }
}
```

## 7. Normalizers

### ProductNormalizer.cs

```csharp
using Regira.Normalizing.Abstractions;
using Regira.Entities.Normalizing;
using WebShop.Models.Entities;

namespace WebShop.Services.Normalizers;

public class ProductNormalizer : EntityNormalizerBase<Product>
{
    private readonly INormalizer _normalizer;

    public ProductNormalizer(INormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public override async Task HandleNormalize(Product item)
    {
        // Combine title and description
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);
    }
}
```

## 8. Controllers

### ProductsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Abstractions;
using WebShop.Models.DTOs;
using WebShop.Models.Entities;
using WebShop.Models.Enums;
using WebShop.Models.SearchObjects;

namespace WebShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
{
    // All CRUD endpoints are inherited from base controller:
    // - GET /api/products
    // - GET /api/products/{id}
    // - POST /api/products/search
    // - POST /api/products
    // - PUT /api/products/{id}
    // - DELETE /api/products/{id}
    
    // Custom endpoint example
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var searchObject = new ProductSearchObject
        {
            MinStock = 0,
            MaxPrice = threshold,
            IsArchived = false
        };
        
        var result = await Search(searchObject, null);
        return Ok(result);
    }
}
```

### CategoriesController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Abstractions;
using WebShop.Models.DTOs;
using WebShop.Models.Entities;
using WebShop.Models.SearchObjects;

namespace WebShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : EntityControllerBase<Category, CategorySearchObject, CategoryDto, CategoryInputDto>
{
    // All CRUD endpoints are inherited from base controller
}
```

### ProductAttachmentsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;
using WebShop.Models.Entities;

namespace WebShop.Controllers;

[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int>
{
    // Endpoints:
    // - GET /api/products/{objectId}/attachments
    // - GET /api/products/{objectId}/attachments/{id}
    // - POST /api/products/{objectId}/attachments (multipart/form-data)
    // - PUT /api/products/{objectId}/attachments/{id}
    // - DELETE /api/products/{objectId}/attachments/{id}
}
```

## 9. Dependency Injection

### Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection;
using Regira.Entities.EFcore.Extensions;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage;
using WebShop.Data;
using WebShop.Models.Entities;
using WebShop.Models.DTOs;
using WebShop.Models.SearchObjects;
using WebShop.Models.Enums;
using WebShop.Services.QueryBuilders;
using WebShop.Services.Normalizers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ShopDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString)
        .AddNormalizerInterceptors(serviceProvider)  // Auto-normalize on save
        .AddAutoTruncateInterceptors();               // Auto-truncate strings
});

// Configure File Storage for Attachments
builder.Services.AddSingleton<IFileService>(sp =>
{
    var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    return new BinaryFileService(uploadsPath);
});

// Configure Entities Framework
builder.Services
    .UseEntities<ShopDbContext>(options =>
    {
        // Add default normalizer (handles [Normalized] attributes)
        options.AddDefaultEntityNormalizer();
        
        // Use Mapster for object mapping
        options.UseMapsterMapping();
    })
    // Configure Product Entity
    .For<Product>(e =>
    {
        e.UseService<ProductQueryBuilder, ProductSearchObject, ProductSortBy, ProductIncludes>()
            .Sort<ProductSortedQueryBuilder>()
            .Include<ProductIncludesQueryBuilder>()
            .UseMapping<ProductDto, ProductInputDto>()
            .After((product, dto) =>
            {
                // Custom mapping logic
                dto.CategoryTitle = product.Category?.Title;
                dto.InStock = product.Stock > 0;
                dto.HasAttachments = product.HasAttachment == true;
                dto.AttachmentCount = product.Attachments?.Count ?? 0;
            });
        
        // Add custom normalizer
        e.AddNormalizer<ProductNormalizer>();
        
        // Configure attachments
        e.Attachments<ProductAttachment>(a =>
        {
            a.UseFileService<BinaryFileService>()
                .RootFolder("products");
        });
    })
    // Configure Category Entity
    .For<Category>(e =>
    {
        e.UseService()
            .Filter((query, so) =>
            {
                // Inline filter logic
                if (!string.IsNullOrWhiteSpace(so?.Q))
                {
                    var searchTerm = $"%{so.Q}%";
                    query = query.Where(x => 
                        EF.Functions.Like(x.Title, searchTerm) ||
                        EF.Functions.Like(x.Description ?? string.Empty, searchTerm));
                }
                return query;
            })
            .UseMapping<CategoryDto, CategoryInputDto>()
            .After((category, dto) =>
            {
                // Calculate product counts
                dto.ProductCount = category.Products?.Count ?? 0;
                dto.ActiveProductCount = category.Products?.Count(p => !p.IsArchived) ?? 0;
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 10. Database Migrations

### Create Initial Migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Migration will create:

- **Categories** table (Id, Title, Description, NormalizedContent, Created, LastModified)
- **Products** table (Id, Title, Description, Price, Stock, CategoryId, NormalizedContent, Created, LastModified, IsArchived, HasAttachment)
- **Attachments** table (Id, FileName, Path, ContentType, Length, Created, LastModified)
- **ProductAttachments** table (Id, ObjectId, ObjectType, AttachmentId, SortOrder, NewFileName, NewContentType, NewBytes)

## 11. Usage Examples

### API Endpoints

#### Get all products

```http
GET /api/products
```

#### Search products with filters

```http
GET /api/products?q=laptop&categoryId=1&minPrice=500&maxPrice=2000&inStock=true&page=1&pageSize=20
```

#### Get product details

```http
GET /api/products/123
```

#### Create a new product

```http
POST /api/products
Content-Type: application/json

{
  "title": "Laptop Pro 15",
  "description": "High-performance laptop with 16GB RAM",
  "price": 1299.99,
  "stock": 50,
  "categoryId": 1
}
```

#### Update a product

```http
PUT /api/products/123
Content-Type: application/json

{
  "title": "Laptop Pro 15 (Updated)",
  "description": "High-performance laptop with 32GB RAM",
  "price": 1499.99,
  "stock": 45,
  "categoryId": 1
}
```

#### Delete a product (soft delete)

```http
DELETE /api/products/123
```

#### Upload product image

```http
POST /api/products/123/attachments
Content-Type: multipart/form-data

file: [binary data]
```

#### Get product attachments

```http
GET /api/products/123/attachments
```

#### Get categories

```http
GET /api/categories?q=electronics
```

### C# Client Usage

```csharp
// Using HttpClient
var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Get products
var response = await client.GetAsync("/api/products?categoryId=1&inStock=true");
var result = await response.Content.ReadFromJsonAsync<ListResult<ProductDto>>();

// Create product
var input = new ProductInputDto
{
    Title = "Gaming Mouse",
    Description = "RGB gaming mouse with 16000 DPI",
    Price = 79.99m,
    Stock = 100,
    CategoryId = 2
};
var createResponse = await client.PostAsJsonAsync("/api/products", input);
var saveResult = await createResponse.Content.ReadFromJsonAsync<SaveResult<ProductDto>>();

// Upload attachment
using var content = new MultipartFormDataContent();
var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync("image.jpg"));
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
content.Add(fileContent, "file", "product-image.jpg");

var uploadResponse = await client.PostAsync($"/api/products/{saveResult.Item.Id}/attachments", content);
```

## 12. Testing Data Seeding

### SeedData.cs

```csharp
using WebShop.Data;
using WebShop.Models.Entities;

namespace WebShop.Data;

public static class SeedData
{
    public static async Task Initialize(ShopDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if already seeded
        if (context.Categories.Any())
        {
            return;
        }

        // Seed categories
        var categories = new[]
        {
            new Category { Title = "Electronics", Description = "Electronic devices and accessories" },
            new Category { Title = "Computers", Description = "Laptops, desktops, and accessories" },
            new Category { Title = "Gaming", Description = "Gaming consoles and accessories" }
        };
        
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Seed products
        var products = new[]
        {
            new Product
            {
                Title = "Laptop Pro 15",
                Description = "High-performance laptop with 16GB RAM",
                Price = 1299.99m,
                Stock = 50,
                CategoryId = categories[1].Id
            },
            new Product
            {
                Title = "Gaming Mouse",
                Description = "RGB gaming mouse with 16000 DPI",
                Price = 79.99m,
                Stock = 100,
                CategoryId = categories[2].Id
            },
            new Product
            {
                Title = "Wireless Keyboard",
                Description = "Mechanical keyboard with RGB backlighting",
                Price = 129.99m,
                Stock = 75,
                CategoryId = categories[1].Id
            }
        };
        
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
```

### Update Program.cs to seed data

```csharp
// After app.Build() and before app.Run()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    await SeedData.Initialize(context);
}
```

## Summary

This complete example demonstrates:

✅ **Entity Models** with built-in interfaces (IHasTimestamps, IArchivable, IHasTitle, etc.)  
✅ **Search Objects** with custom filtering properties  
✅ **Custom Enums** for sorting and includes  
✅ **Query Builders** (Filtered, Sorted, Includes)  
✅ **DTOs** for input and output  
✅ **Controllers** with automatic CRUD endpoints  
✅ **Database Context** with proper configuration  
✅ **Normalization** (automatic and custom)  
✅ **Attachments** with file storage  
✅ **Dependency Injection** configuration  
✅ **AfterMappers** for custom DTO mapping  
✅ **Complete API** ready for testing

You can copy this entire structure into a new ASP.NET Core project and have a fully functional webshop API with all Regira Entities features enabled.

## Overview

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
1. **[Full Example](11-Full-Example.md)** - Complete webshop implementation
