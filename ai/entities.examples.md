# Regira Entities — Code Examples Reference

> **AI Agent Rule**: Use this file to copy correct, working code patterns.
> Always combine with `entities.namespaces.md` for exact `using` directives.
> Do NOT invent variations — adapt the examples below to the entity at hand.
> For project setup templates (`.csproj`, `Program.cs`, config files), see [`entities.setup.md`](entities.setup.md).

---

## 1. Project Setup

> **→ See:** [`entities.setup.md`](entities.setup.md)

---

## 2. DbContext

### Data/AppDbContext.cs (minimal)
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
    }
}
```

### Data/AppDbContext.cs (with entity relationship)
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

## 3. DI Extension Methods

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

### Entities/Products/ProductServiceCollectionExtensions.cs (per-entity pattern)
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

## 4. Entity Model

### Basic entity (all common interfaces)
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

    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

### Entity with Guid primary key
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

public class Document : IEntity<Guid>, IHasTimestamps, IHasTitle
{
    public Guid Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
```

### Child entity (ISortable)
```csharp
using Regira.Entities.Models.Abstractions;

public class OrderItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}
```

---

## 5. SearchObject

```csharp
using Regira.Entities.Models;

public class ProductSearchObject : SearchObject
{
    // Use ICollection<TKey> for FK filters (enables multi-value filtering)
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Inherited: Id, Ids, Exclude, Q, MinCreated, MaxCreated,
    //            MinLastModified, MaxLastModified, IsArchived
}
```

---

## 6. SortBy Enum

```csharp
// NOT [Flags] — values are applied one at a time, not combined
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

---

## 7. Includes Enum

```csharp
// IS [Flags] — values are combined with bitwise OR
[Flags]
public enum ProductIncludes
{
    Default  = 0,
    Category = 1 << 0,
    Reviews  = 1 << 1,
    All      = Category | Reviews
}
```

---

## 8. DTOs

### Output DTO
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }   // navigation property, not flattened
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? DisplayName { get; set; }      // computed — set in AfterMapper
}
```

### Input DTO
```csharp
using System.ComponentModel.DataAnnotations;

public class ProductInputDto
{
    public int Id { get; set; }   // required for Save (upsert)

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    // DO NOT include: Created, LastModified, NormalizedContent, computed properties
    // Only include child collections when configured with e.Related(...)
}
```

---

## 9. Query Builders

### Inline filter
```csharp
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

### Separate class
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
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

// Registration:
e.AddQueryFilter<ProductQueryBuilder>();
```

### Separate class with IQKeywordHelper (normalized text search)
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

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
            var keywords = _qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }
        return query;
    }
}
```

### Inline sorting
```csharp
e.SortBy((query, sortBy) =>
{
    if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type)
        && query is IOrderedQueryable<Product> sorted)
    {
        return sortBy switch
        {
            ProductSortBy.Title     => sorted.ThenBy(x => x.Title),
            ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
            ProductSortBy.Price     => sorted.ThenBy(x => x.Price),
            ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
            _                       => sorted.ThenByDescending(x => x.Created)
        };
    }
    return sortBy switch
    {
        ProductSortBy.Title      => query.OrderBy(x => x.Title),
        ProductSortBy.TitleDesc  => query.OrderByDescending(x => x.Title),
        ProductSortBy.Price      => query.OrderBy(x => x.Price),
        ProductSortBy.PriceDesc  => query.OrderByDescending(x => x.Price),
        ProductSortBy.Created    => query.OrderBy(x => x.Created),
        ProductSortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
        _                        => query.OrderByDescending(x => x.Created)
    };
});
```

### Inline includes
```csharp
e.Includes((query, includes) =>
{
    if (includes?.HasFlag(ProductIncludes.Category) == true)
        query = query.Include(x => x.Category);
    if (includes?.HasFlag(ProductIncludes.Reviews) == true)
        query = query.Include(x => x.Reviews!.OrderBy(r => r.SortOrder));
    return query;
});
```

---

## 10. Processors

### Inline
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

### Separate class (with DI)
```csharp
using Regira.Entities.EFcore.Processing.Abstractions;

public class CategoryProcessor(AppDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes)
    {
        var itemIds = items.Select(i => i.Id).ToList();
        var productCountPerCategory = await dbContext.Products
            .Where(p => itemIds.Contains(p.CategoryId ?? 0))
            .GroupBy(p => p.CategoryId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        foreach (var item in items)
            item.ProductCount = productCountPerCategory.TryGetValue(item.Id, out var count) ? count : 0;
    }
}

// Registration:
e.Process<CategoryProcessor>();
```

---

## 11. Preppers

### Inline (simple)
```csharp
e.Prepare(item =>
{
    item.Slug ??= item.Title.ToLowerInvariant().Replace(' ', '-');
});
```

### Inline with original (create vs update)
```csharp
// original is null when creating, non-null when updating
e.Prepare((modified, original) =>
{
    if (original == null)
        modified.CreatedBy = "system";
});
```

### Inline with DbContext
```csharp
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});
```

### Separate class
```csharp
using Regira.Entities.EFcore.Preppers.Abstractions;

public class ProductPrepper : EntityPrepperBase<Product>
{
    public override Task Prepare(Product modified, Product? original)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}

// Registration:
e.Prepare<ProductPrepper>();
```

### Child collection (Related)
```csharp
// Manage child collection + apply sort order
e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());

// Minimal (no extra preparation)
e.Related(x => x.OrderItems);
```

---

## 12. Primers

### Separate class (entity-specific)
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

public class ProductPrimer : EntityPrimerBase<Product>
{
    public override Task PrepareAsync(Product entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();

        return Task.CompletedTask;
    }
}

// Registration on entity:
e.AddPrimer<ProductPrimer>();
```

### Global primer (interface-based)
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

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

// Global registration (runs for all entities implementing IAuditable):
options.AddPrimer<UserTrackingPrimer>();
```

### Built-in primers (registered by UseDefaults)
```csharp
options.AddPrimer<HasCreatedDbPrimer>();
options.AddPrimer<HasLastModifiedDbPrimer>();
options.AddPrimer<ArchivablePrimer>();
```

---

## 13. Mapping & AfterMappers

### Inline mapping with AfterMapper
```csharp
e.UseMapping<ProductDto, ProductInputDto>()
    .After((entity, dto) =>
    {
        dto.DisplayName = $"{entity.Title} - €{entity.Price:F2}";
        dto.AttachmentCount = entity.Attachments?.Count ?? 0;
    })
    .AfterInput((inputDto, entity) =>
    {
        // runs after InputDto → Entity mapping
    });
```

### Additional DTO mappings (for related entities in child collections)
```csharp
e.AddMapping<OrderItem, OrderItemDto>();
e.AddMapping<OrderItemInputDto, OrderItem>();
```

### Separate AfterMapper class (with DI)
```csharp
using Regira.Entities.Mapping.Abstractions;

public class ProductAfterMapper : EntityAfterMapperBase<Product, ProductDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductAfterMapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void AfterMap(Product source, ProductDto target)
    {
        target.ImageUrl = $"https://{_httpContextAccessor.HttpContext?.Request.Host}/images/{source.Id}";
        target.AttachmentCount = source.Attachments?.Count ?? 0;
    }
}

// Registration:
e.UseMapping<ProductDto, ProductInputDto>()
    .After<ProductAfterMapper>();

// Global AfterMapper (applies to all entities implementing IMyInterface):
options.AddAfterMapper<MyGlobalAfterMapper>();
```

---

## 14. Controllers

### Full-featured controller
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
    // Provides: GET /list, GET /search, GET /{id}, POST, PUT /{id},
    //           POST /save, DELETE /{id}, POST /list, POST /search
}
```

### Controller variants (choose the minimal sufficient one)
```csharp
// Minimal — no search, no sorting
EntityControllerBase<TEntity, TDto, TInputDto>

// With search only
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>

// With search + non-default key type
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>

// Full-featured (recommended)
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

---

## 15. Custom Entity Service (EntityWrappingServiceBase)

### Interface + wrapper
```csharp
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Models;
using Regira.DAL.Paging;

public interface IProductService
    : IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> { }

public class ProductService
    : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>,
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

    public override async Task<Product?> Details(int id)
    {
        _logger.LogDebug("Fetching product {Id}", id);
        return await base.Details(id);
    }

    public override async Task Save(Product item)
    {
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
}
```

### Registration
```csharp
e.AddTransient<IProductService, ProductService>();  // enables typed injection
e.UseEntityService<ProductService>();               // replaces default EntityRepository
```

### Caching variant
```csharp
using Microsoft.Extensions.Caching.Memory;
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

// Registration:
e.UseEntityService<CachedProductService>();
```

---

## 16. Global Services

### Global filter query builder (separate class)
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

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

// Registration:
options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();
```

### Global prepper (inline)
```csharp
options.AddPrepper<IHasSlug>(entity =>
{
    entity.Slug ??= entity.Title?.ToLowerInvariant().Replace(' ', '-');
});
```

### Built-in global filter (normalized content Q search)
```csharp
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

---

## 17. Normalizing

### Attribute-based (recommended)
```csharp
using Regira.Normalizing;

public class Product : IEntityWithSerial
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // Multiple sources — concatenated with space
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    // Single source
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
}
```

### Custom normalizer
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

    public override async Task HandleNormalize(Product item)
    {
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);
    }
}

// Per-entity registration:
e.AddNormalizer<ProductNormalizer>();

// Global registration (applies to all entities implementing interface):
options.AddNormalizer<IHasPhone, PhoneNormalizer>();
```

### Enable normalizer interceptors (required in AddDbContext)
```csharp
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddNormalizerInterceptors(sp));
```

---

## 18. Attachments

### EntityAttachment model
```csharp
using Regira.Entities.Attachments.Models;

public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}
```

### Entity with IHasAttachments
```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Models;

public class Product : IEntityWithSerial, IHasAttachments, IHasAttachments<ProductAttachment>
{
    public int Id { get; set; }

    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }

    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
}
```

### Attachment controller
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;

[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int>
{
}
```

### DbContext for attachments
```csharp
using Regira.Entities.Attachments.Models;

public DbSet<Attachment> Attachments { get; set; } = null!;
public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;

// In OnModelCreating:
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
```

### DI — file system storage
```csharp
using Regira.IO.Storage.FileSystem;

services.UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions
        {
            RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        }
    ));
```

### DI — Azure Blob Storage
```csharp
using Regira.IO.Storage.Azure;

services.UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryBlobService(
        new AzureCommunicator(new AzureOptions
        {
            ConnectionString = configuration["Azure:StorageConnectionString"],
            ContainerName = "attachments"
        })
    ));
```

### DI — SFTP storage
```csharp
using Regira.IO.Storage.SSH;

services.UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new SftpService(new SftpCommunicator(new SftpConfig
    {
        Host = "sftp.example.com",
        UserName = "user",
        Password = "pass",
        ContainerName = "/uploads"
    })));
```

---

## 19. Error Handling

### EntityInputException (returns HTTP 400)
```csharp
using Regira.Entities.Models;

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

## 20. Built-in Query Extensions

```csharp
using Regira.Entities.EFcore.QueryBuilders; // QueryExtensions
using Regira.DAL.Paging;                    // PagingInfo, PageQuery

query.FilterId(so.Id)
query.FilterIds(so.Ids)
query.FilterExclude(so.Exclude)
query.FilterCode(so.Code)
query.FilterTitle(keywords)
query.FilterNormalizedTitle(keywords)
query.FilterCreated(so.MinCreated, so.MaxCreated)
query.FilterLastModified(so.MinLastModified, so.MaxLastModified)
query.FilterTimestamps(minCreated, maxCreated, minModified, maxModified)
query.FilterQ(keywords)
query.FilterArchivable(so.IsArchived)
query.FilterHasAttachment(so.HasAttachment)
query.SortQuery<TEntity, TKey>()
query.PageQuery(pagingInfo)
query.PageQuery(pageSize: 20, page: 1)
```

---

## 21. Common Patterns

### Master-Detail (Order + OrderItems)

```csharp
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

// DI:
.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
{
    e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());
    e.Prepare(item =>
    {
        item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    });
});
```

### Many-to-Many (explicit join entity)

Always use an explicit join entity. 
- compatible with `e.Related()` prepper
- easy mapping: keep DTOs structure in sync with entities

```csharp
// Entities
public class Student : IEntityWithSerial
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<StudentCourse>? StudentCourses { get; set; }
}

public class Course : IEntityWithSerial
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public ICollection<StudentCourse>? StudentCourses { get; set; }
}

public class StudentCourse : IEntityWithSerial
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public int? Grade { get; set; }

    public Student? Student { get; set; }
    public Course? Course { get; set; }
}

// DTOs
public class StudentInputDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<StudentCourseInputDto>? StudentCourses { get; set; }
}

public class StudentCourseInputDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public int? Grade { get; set; }
}

// DbContext:
modelBuilder.Entity<StudentCourse>(entity =>
{
    entity.HasOne(sc => sc.Student).WithMany(s => s.StudentCourses).HasForeignKey(sc => sc.StudentId);
    entity.HasOne(sc => sc.Course).WithMany(c => c.StudentCourses).HasForeignKey(sc => sc.CourseId);
});

// DI:
.For<Student, StudentSearchObject, StudentSortBy, StudentIncludes>(e =>
{
    e.Includes((query, includes) =>
    {
        if (includes?.HasFlag(StudentIncludes.Courses) == true)
            query = query.Include(x => x.StudentCourses!).ThenInclude(sc => sc.Course);
        return query;
    });

    e.Related(x => x.StudentCourses);

    e.UseMapping<StudentDto, StudentInputDto>();
    e.AddMapping<StudentCourse, StudentCourseDto>();
    e.AddMapping<StudentCourseInputDto, StudentCourse>();
});
```

### Soft Delete
```csharp
// Entity — implement IArchivable
public class Product : IEntityWithSerial, IArchivable
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}

// UseDefaults() registers ArchivablePrimer + FilterArchivablesQueryBuilder automatically
// SearchObject.IsArchived:  null = active only (default), true = archived only, false = active only
```

### Hierarchical / Self-referencing
```csharp
public class Category : IEntityWithSerial, IHasTitle
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? Children { get; set; }
}

// In query builder:
if (so?.ParentId != null)
    query = query.Where(x => x.ParentId == so.ParentId);
if (so?.RootOnly == true)
    query = query.Where(x => x.ParentId == null);
```

---

## 22. Complete Example (Product + Category)

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

public class CategorySearchObject : SearchObject { }
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
    Title, TitleDesc,
    Price, PriceDesc,
    Created, CreatedDesc
}
```

### Entities/Products/ProductIncludes.cs
```csharp
[Flags]
public enum ProductIncludes
{
    Default  = 0,
    Category = 1 << 0,
    All      = Category
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
    public CategoryDto? Category { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? DisplayName { get; set; }
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
                if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type)
                    && query is IOrderedQueryable<Product> sorted)
                    return sortBy switch
                    {
                        ProductSortBy.Title     => sorted.ThenBy(x => x.Title),
                        ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                        ProductSortBy.Price     => sorted.ThenBy(x => x.Price),
                        ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
                        _                       => sorted.ThenByDescending(x => x.Created)
                    };

                return sortBy switch
                {
                    ProductSortBy.Title      => query.OrderBy(x => x.Title),
                    ProductSortBy.TitleDesc  => query.OrderByDescending(x => x.Title),
                    ProductSortBy.Price      => query.OrderBy(x => x.Price),
                    ProductSortBy.PriceDesc  => query.OrderByDescending(x => x.Price),
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
                    dto.DisplayName = $"{product.Title} - €{product.Price:F2}";
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
public class CategoriesController
    : EntityControllerBase<Category, CategorySearchObject, CategoryDto, CategoryInputDto>
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

### Migration commands
```bash
dotnet ef migrations add Add_Product
dotnet ef database update
```
