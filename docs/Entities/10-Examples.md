# Practical Examples

This guide demonstrates the Regira Entities framework using a simple webshop scenario with Products and Categories.

## Example 1: Product Entity (Full Implementation)

### Entity Model

```csharp
public class Product : IEntity<int>, IHasTimestamps, IArchivable, IHasTitle, IHasDescription
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    
    // Normalization support
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    
    // Built-in interfaces
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }
    
    // Navigation
    public Category? Category { get; set; }
}
```

### SearchObject

```csharp
public class ProductSearchObject : SearchObject
{
    public int? CategoryId { get; set; }
    public ICollection<int>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### SortBy Enum

```csharp
[Flags]
public enum ProductSortBy
{
    Default = 0,
    Title = 1 << 0,
    TitleDesc = 1 << 1,
    Price = 1 << 2,
    PriceDesc = 1 << 3,
    Created = 1 << 4,
    CreatedDesc = 1 << 5
}
```

### Includes Enum

```csharp
[Flags]
public enum ProductIncludes
{
    Default = 0,
    Category = 1 << 0,
    All = Category
}
```

### Query Builder (Separate Class)

```csharp
public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;

        // Filter by CategoryId
        if (so.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == so.CategoryId.Value);

        // Filter by CategoryIds
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

### DTOs

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

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
{
}
```

### Dependency Injection

```csharp
services.UseEntities<ShopDbContext>(options =>
{
    options.AddDefaultEntityNormalizer();
})
.For<Product>(e =>
{
    e.UseService<ProductQueryBuilder>()
        .UseMapping<ProductDto, ProductInputDto>()
        .After((product, dto) =>
        {
            // AfterMapper: Add category title to DTO
            dto.CategoryTitle = product.Category?.Title;
        });
});
```

## Example 2: Category with Inline Configuration

### Entity Model

```csharp
public class Category : IEntity<int>, IHasTitle
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public ICollection<Product>? Products { get; set; }
}
```

### SearchObject

```csharp
public class CategorySearchObject : SearchObject
{
    // Uses default SearchObject properties only
}
```

### DTOs

```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ProductCount { get; set; }
}

public class CategoryInputDto
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
}
```

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : EntityControllerBase<Category, CategoryDto, CategoryInputDto>
{
}
```

### Dependency Injection (Inline Configuration)

```csharp
services.UseEntities<ShopDbContext>(options => { /* ... */ })
.For<Category>(e =>
{
    e.UseService()
        // Inline QueryBuilder
        .Filter((query, so) =>
        {
            // Title search using Q property
            if (!string.IsNullOrWhiteSpace(so?.Q))
                query = query.Where(x => EF.Functions.Like(x.Title, $"%{so.Q}%"));
            return query;
        })
        .UseMapping<CategoryDto, CategoryInputDto>()
        // Inline AfterMapper
        .After((category, dto) =>
        {
            dto.ProductCount = category.Products?.Count ?? 0;
        });
});
```

## Example 3: Product Attachments

### Entity Attachment Model

```csharp
public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}
```

### Update Product Entity

```csharp
public class Product : IEntity<int>, IHasTimestamps, IArchivable, IHasTitle, IHasDescription,
    IHasAttachments, IHasAttachments<ProductAttachment>
{
    // ... existing properties ...
    
    // Attachment support
    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
}
```

### DbContext Configuration

```csharp
public class ShopDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<ProductAttachment> ProductAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        modelBuilder.Entity<ProductAttachment>()
            .HasOne(x => x.Attachment)
            .WithMany()
            .HasForeignKey(x => x.AttachmentId);
    }
}
```

### Attachment Controller

```csharp
[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int>
{
}
```

### Dependency Injection

```csharp
services.AddDbContext<ShopDbContext>((sp, db) =>
{
    db.UseSqlServer(connectionString)
        .AddNormalizerInterceptors(sp)
        .AddAutoTruncateInterceptors();
});

services.UseEntities<ShopDbContext>(options => { /* ... */ })
.For<Product>(e =>
{
    // ... existing configuration ...
    
    // Attachment configuration
    e.Attachments<ProductAttachment>(a =>
    {
        a.UseFileService<BinaryFileService>()
            .RootFolder("uploads/products");
    });
});
```

## Example 4: Custom Normalizer

### Separate Normalizer Class

```csharp
public class ProductNormalizer : EntityNormalizerBase<Product>
{
    private readonly INormalizer _normalizer;

    public ProductNormalizer(INormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public override async Task HandleNormalize(Product item)
    {
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);
    }
}
```

### Registration

```csharp
services.UseEntities<ShopDbContext>(options => { /* ... */ })
.For<Product>(e =>
{
    e.AddNormalizer<ProductNormalizer>();
    // ... rest of configuration ...
});
```



## Overview

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. **[Practical Examples](10-Examples.md)** - Complete implementation examples
