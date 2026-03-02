# Regira Entities — Code Examples Reference

> **AI Agent Rule**: Use this file to copy correct, working code patterns.
> Always combine with `entities.namespaces.md` for exact `using` directives.
> Do NOT invent variations — adapt the examples below to the entity at hand.
> For project setup templates (`.csproj`, `Program.cs`, config files), see [`entities.setup.md`](entities.setup.md).

---

## 1. Project Setup

> **→ See:** [`entities.setup.md`](./entities.setup.md)

---

## 2. DbContext

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

        // Configure relationships:
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

```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;

// Global registration:
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

// Per-entity extension:
public static class ProductServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddProducts<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            // configure filters, sorting, mapping, etc.
        });
        return services;
    }
}
```

---

## 4. Entity Models

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

// Standard entity with all common interfaces:
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

// Entity with Guid key: use IEntity<Guid>
// Child entity: implement ISortable (adds SortOrder property)
```

---

## 5. SearchObject, SortBy, Includes

```csharp
using Regira.Entities.Models;

// SearchObject: Use ICollection<TKey> for FK filters (enables multi-value)
public class ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    // Inherited: Id, Ids, Exclude, Q, MinCreated, MaxCreated,
    //            MinLastModified, MaxLastModified, IsArchived
}

// SortBy: NOT [Flags] — values applied one at a time
public enum ProductSortBy
{
    Default = 0,
    Title, TitleDesc,
    Price, PriceDesc,
    Created, CreatedDesc
}

// Includes: IS [Flags] — values combined with bitwise OR
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

## 6. DTOs

```csharp
using System.ComponentModel.DataAnnotations;

// Output DTO: includes computed properties
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }   // navigation (not flattened)
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? DisplayName { get; set; }      // computed in AfterMapper
}

// Input DTO: only editable properties
public class ProductInputDto
{
    public int Id { get; set; }   // required for upsert

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

## 7. Query Builders (Filter, Sort, Includes)

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

// Inline filter:
e.Filter((query, so) =>
{
    if (so?.CategoryId?.Any() == true)
        query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));
    if (so?.MinPrice != null)
        query = query.Where(x => x.Price >= so.MinPrice);
    return query;
});

// Separate class (with DI for IQKeywordHelper):
public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    private readonly IQKeywordHelper _qHelper;

    public ProductQueryBuilder(IQKeywordHelper qHelper) => _qHelper = qHelper;

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
// Registration: e.AddQueryFilter<ProductQueryBuilder>();

// Inline sorting:
e.SortBy((query, sortBy) =>
{
    if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type)
        && query is IOrderedQueryable<Product> sorted)
        return sortBy switch
        {
            ProductSortBy.Title => sorted.ThenBy(x => x.Title),
            ProductSortBy.Price => sorted.ThenBy(x => x.Price),
            _ => sorted.ThenByDescending(x => x.Created)
        };

    return sortBy switch
    {
        ProductSortBy.Title => query.OrderBy(x => x.Title),
        ProductSortBy.Price => query.OrderBy(x => x.Price),
        _ => query.OrderByDescending(x => x.Created)
    };
});

// Inline includes:
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

## 8. Processors

```csharp
using Regira.Entities.EFcore.Processing.Abstractions;

// Inline processor:
e.Process((items, includes) =>
{
    foreach (var item in items)
    {
        item.DisplayPrice = $"€{item.Price:F2}";
        item.IsOnSale = item.SalePrice.HasValue && item.SalePrice < item.Price;
    }
    return Task.CompletedTask;
});

// Separate class with DI (for complex logic):
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
// Registration: e.Process<CategoryProcessor>();
```

---

## 9. Preppers

```csharp
using Regira.Entities.EFcore.Preppers.Abstractions;

// Inline (simple):
e.Prepare(item =>
{
    item.Slug ??= item.Title.ToLowerInvariant().Replace(' ', '-');
});

// With original (create vs update — original is null when creating):
e.Prepare((modified, original) =>
{
    if (original == null)
        modified.CreatedBy = "system";
});

// With DbContext:
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});

// Separate class:
public class ProductPrepper : EntityPrepperBase<Product>
{
    public override Task Prepare(Product modified, Product? original)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}
// Registration: e.Prepare<ProductPrepper>();

// Child collection management:
e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());
// Or minimal: e.Related(x => x.OrderItems);
```

---

## 10. Primers

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

// Entity-specific primer:
public class ProductPrimer : EntityPrimerBase<Product>
{
    public override Task PrepareAsync(Product entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();
        return Task.CompletedTask;
    }
}
// Registration: e.AddPrimer<ProductPrimer>();

// Global primer (interface-based, applies to all entities implementing interface):
public class UserTrackingPrimer : EntityPrimerBase<IAuditable>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTrackingPrimer(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

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
// Registration: options.AddPrimer<UserTrackingPrimer>();

// Built-in primers (registered by UseDefaults()):
// - HasCreatedDbPrimer, HasLastModifiedDbPrimer, ArchivablePrimer
```

---

## 11. Mapping & AfterMappers

```csharp
using Regira.Entities.Mapping.Abstractions;

// Inline mapping with AfterMapper:
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

// Additional DTO mappings (for child collections):
e.AddMapping<OrderItem, OrderItemDto>();
e.AddMapping<OrderItemInputDto, OrderItem>();

// Separate AfterMapper class (with DI):
public class ProductAfterMapper : EntityAfterMapperBase<Product, ProductDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductAfterMapper(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public override void AfterMap(Product source, ProductDto target)
    {
        target.ImageUrl = $"https://{_httpContextAccessor.HttpContext?.Request.Host}/images/{source.Id}";
        target.AttachmentCount = source.Attachments?.Count ?? 0;
    }
}
// Registration: e.UseMapping<ProductDto, ProductInputDto>().After<ProductAfterMapper>();
// Global: options.AddAfterMapper<MyGlobalAfterMapper>();
```

---

## 12. Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

// Full-featured controller (recommended):
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

// Variants (use minimal sufficient base):
// EntityControllerBase<TEntity, TDto, TInputDto>  // minimal
// EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>  // with search
// EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>  // + custom key
// EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>  // full
```

---

## 13. Custom Entity Service (EntityWrappingServiceBase)

```csharp
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Models;
using Microsoft.Extensions.Caching.Memory;

// Interface + wrapper (for custom business logic):
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
        : base(inner) => _logger = logger;

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

// Registration:
e.AddTransient<IProductService, ProductService>();  // enables typed injection
e.UseEntityService<ProductService>();               // replaces default EntityRepository

// Caching example:
public class CachedProductService
    : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public CachedProductService(
        IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> inner,
        IMemoryCache cache)
        : base(inner) => _cache = cache;

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
```

---

## 14. Global Services

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

// Global filter (applies to all entities implementing interface):
public class FilterByTenantQueryBuilder : GlobalFilteredQueryBuilderBase<ITenantEntity, int>
{
    private readonly ITenantContext _tenantContext;

    public FilterByTenantQueryBuilder(ITenantContext tenantContext)
        => _tenantContext = tenantContext;

    public override IQueryable<ITenantEntity> Build(IQueryable<ITenantEntity> query, ISearchObject<int>? so)
    {
        return query.Where(x => x.TenantId == _tenantContext.CurrentTenantId);
    }
}
// Registration: options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();

// Global prepper (inline):
options.AddPrepper<IHasSlug>(entity =>
{
    entity.Slug ??= entity.Title?.ToLowerInvariant().Replace(' ', '-');
});

// Built-in global filter (normalized content Q search):
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

---

## 15. Normalizing

```csharp
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Regira.Entities.EFcore.Normalizing.Abstractions;

// Attribute-based (recommended):
public class Product : IEntityWithSerial
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // Multiple sources (concatenated):
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    // Single source:
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
}

// Custom normalizer:
public class ProductNormalizer : EntityNormalizerBase<Product>
{
    private readonly INormalizer _normalizer;

    public ProductNormalizer(INormalizer normalizer) => _normalizer = normalizer;

    public override async Task HandleNormalize(Product item)
    {
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);
    }
}
// Per-entity: e.AddNormalizer<ProductNormalizer>();
// Global: options.AddNormalizer<IHasPhone, PhoneNormalizer>();

// Enable normalizer interceptors (required in AddDbContext):
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddNormalizerInterceptors(sp));
```

---

## 16. Attachments

```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Abstractions;
using Regira.IO.Storage.FileSystem;
using Microsoft.AspNetCore.Mvc;

// EntityAttachment model:
public class ProductAttachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof(Product);
}

// Entity with IHasAttachments:
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

// Controller:
[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController : EntityAttachmentControllerBase<ProductAttachment, int, int> { }

// DbContext:
public DbSet<Attachment> Attachments { get; set; } = null!;
public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;

// In OnModelCreating:
modelBuilder.Entity<ProductAttachment>()
    .HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId);
modelBuilder.Entity<Product>(entity =>
{
    entity.HasMany(e => e.Attachments).WithOne().HasForeignKey(e => e.ObjectId).HasPrincipalKey(e => e.Id);
});

// DI — File system:
services.UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions { RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads") }));

// DI — Azure Blob (see entities.namespaces.md for full Azure/SFTP examples)
```

---

## 17. Error Handling & Query Extensions

```csharp
using Regira.Entities.Models;
using Regira.Entities.EFcore.QueryBuilders;  // QueryExtensions
using Regira.DAL.Paging;                     // PagingInfo, PageQuery

// EntityInputException (returns HTTP 400):
throw new EntityInputException<Product>("Validation failed")
{
    Item = item,
    InputErrors = new Dictionary<string, string>
    {
        [nameof(Product.Price)] = "Price must be greater than 0",
        [nameof(Product.Title)] = "Title is required"
    }
};

// Built-in query extensions:
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

## 18. Common Patterns

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
    e.Prepare(item => item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0);
});
```

### Many-to-Many (always use explicit join entity)
```csharp
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

// DTOs mirror entity structure:
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
public class Product : IEntityWithSerial, IArchivable
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}
// UseDefaults() registers ArchivablePrimer + FilterArchivablesQueryBuilder automatically
// SearchObject.IsArchived: null = active only (default), true = archived only
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