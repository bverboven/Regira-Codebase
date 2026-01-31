# Practical Examples - AI Agent Instructions

## Complete Implementation Examples

This guide provides end-to-end examples of implementing entities using the Regira Entity Framework.

## Example 1: Simple Entity (Category)

### 1. Entity Model

```csharp
// Models/Category.cs
public class Category : IEntity, IHasTitle, IArchivable, ISortable
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsArchived { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

### 2. DTOs

```csharp
// Models/DTOs/CategoryDto.cs
public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
}

// Models/DTOs/CategoryInput.cs
public class CategoryInput
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
```

### 3. DbContext Configuration

```csharp
// Data/AppDbContext.cs
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.SortOrder);
        });
    }
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.AutoTruncateStringsToMaxLengthForEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.AutoTruncateStringsToMaxLengthForEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
```

### 4. Controller

```csharp
// Controllers/CategoriesController.cs
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : EntityController<Category, CategoryDto, CategoryInput>
{
    public CategoriesController(
        IEntityService<Category> service,
        IMapper mapper)
        : base(service, mapper)
    {
    }
}
```

### 5. DI Configuration

```csharp
// Program.cs
builder.Services
    .UseEntities<AppDbContext>(config =>
    {
        config.UseDefaults();
        config.UseMapsterMapping();
    })
    .For<Category>(cfg =>
    {
        cfg.SortBy((q, _) => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
        cfg.Includes((q, _) => q.Include(x => x.Products));
        cfg.Related(c => c.Products);
    });
```

### 6. Mapping Configuration

```csharp
// Configuration/MappingConfig.cs
TypeAdapterConfig<Category, CategoryDto>.NewConfig()
    .Map(dest => dest.ProductCount, src => src.Products.Count);
    
TypeAdapterConfig<CategoryInput, Category>.NewConfig()
    .Ignore(dest => dest.Id)
    .Ignore(dest => dest.Products);
```

---

## Example 2: Advanced Entity (Product)

### 1. Entity Model

```csharp
// Models/Product.cs
public class Product : IEntity, IHasTitle, IArchivable, IHasTimestamps, IHasSlug
{
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [MaxLength(500)]
    public string? ImagePath { get; set; }
    
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Foreign Keys
    public int CategoryId { get; set; }
    
    // Navigation
    public Category? Category { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    // Unmapped
    [NotMapped]
    public string? ImageUrl { get; set; }
}
```

### 2. Search Object

```csharp
// Models/ProductSearchObject.cs
public class ProductSearchObject : SearchObject
{
    public string? Title { get; set; }
    public ICollection<int>? CategoryId { get; set; }  // Use ICollection for foreign keys
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
}
```

### 3. Enums

```csharp
// Models/Enums/ProductSortBy.cs
public enum ProductSortBy
{
    Default = 0,
    Title = 1,
    Price = 2,
    PriceDesc = 3,
    Newest = 4,
    Rating = 5
}

// Models/Enums/ProductIncludes.cs
[Flags]
public enum ProductIncludes
{
    None = 0,
    Category = 1,
    Reviews = 2,
    All = Category | Reviews
}
```

### 4. DTOs

```csharp
// Models/DTOs/ProductDto.cs
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime Created { get; set; }
    
    // Navigation properties - may be null in list views
    public CategoryDto? Category { get; set; }
    public ICollection<ReviewDto>? Reviews { get; set; }
    
    // Computed properties
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

// Models/DTOs/ProductInput.cs
public class ProductInput
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public string? ImagePath { get; set; }
}
```

### 5. Query Filter

```csharp
// Services/QueryFilters/ProductQueryFilter.cs
public class ProductQueryFilter : IFilteredQueryBuilder<Product, ProductSearchObject>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? search)
    {
        if (search == null) return query;
        
        // General text search using Q property (supports wildcards)
        if (!string.IsNullOrWhiteSpace(search.Q))
        {
            var keywords = QKeywordHelper.GetKeywords(search.Q);
            query = query.Where(x => 
                keywords.Any(k => x.Title.Contains(k)) ||
                (x.Description != null && keywords.Any(k => x.Description.Contains(k))));
        }
        
        if (!string.IsNullOrWhiteSpace(search.Title))
        {
            query = query.Where(x => x.Title.Contains(search.Title));
        }
        
        if (search.CategoryId?.Any() == true)
        {
            query = query.Where(x => search.CategoryId.Contains(x.CategoryId));
        }
        
        if (search.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= search.MinPrice.Value);
        }
        
        if (search.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= search.MaxPrice.Value);
        }
        
        return query;
    }
}
```

### 6. Sort Builder

```csharp
// Services/QueryBuilders/ProductSortedQueryBuilder.cs
public class ProductSortedQueryBuilder : ISortedQueryBuilder<Product, ProductSortBy>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductSortBy? sortBy)
    {
        return sortBy switch
        {
            ProductSortBy.Title => query.OrderBy(x => x.Title),
            ProductSortBy.Price => query.OrderBy(x => x.Price),
            ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
            ProductSortBy.Newest => query.OrderByDescending(x => x.Created),
            ProductSortBy.Rating => query.OrderByDescending(x => x.Reviews.Average(r => r.Rating)),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
```

### 7. Include Builder

```csharp
// Services/QueryBuilders/ProductIncludableQueryBuilder.cs
public class ProductIncludableQueryBuilder : IIncludableQueryBuilder<Product, ProductIncludes>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductIncludes? include)
    {
        if (include == null || include == ProductIncludes.None)
            return query;
        
        if (include.Value.HasFlag(ProductIncludes.Category))
        {
            query = query.Include(x => x.Category);
        }
        
        if (include.Value.HasFlag(ProductIncludes.Reviews))
        {
            query = query.Include(x => x.Reviews);
        }
        
        return query;
    }
}
```

### 8. Processor

```csharp
// Services/Processors/ProductProcessor.cs
public class ProductProcessor : IEntityProcessor<Product>
{
    private readonly IConfiguration _configuration;
    
    public ProductProcessor(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Task ProcessAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(entity.ImagePath))
        {
            var baseUrl = _configuration["ImageBaseUrl"];
            entity.ImageUrl = $"{baseUrl}/{entity.ImagePath}";
        }
        
        return Task.CompletedTask;
    }
    
    public async Task ProcessAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await ProcessAsync(entity, cancellationToken);
        }
    }
}
```

### 9. Prepper

```csharp
// Services/Preppers/ProductPrepper.cs
public class ProductPrepper : IEntityPrepper<Product>
{
    public Task PrepAsync(Product entity, CancellationToken cancellationToken = default)
    {
        // Normalize title
        entity.Title = entity.Title.Trim();
        
        // Generate slug if empty
        if (string.IsNullOrEmpty(entity.Slug))
        {
            entity.Slug = GenerateSlug(entity.Title);
        }
        
        return Task.CompletedTask;
    }
    
    private static string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("?", "")
            .Replace("!", "");
    }
}
```

### 10. Controller

```csharp
// Controllers/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityController<
    Product,
    int,
    ProductSearchObject,
    ProductSortBy,
    ProductIncludes,
    ProductDto,
    ProductInput>
{
    public ProductsController(
        IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> service,
        IMapper mapper)
        : base(service, mapper)
    {
    }
    
    // GET api/products/slug/my-product-slug
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug)
    {
        // Note: For unique lookups, add Slug property to ProductSearchObject
        var items = await Service.List(new { Slug = slug });
        var item = items.FirstOrDefault();
        
        if (item == null)
            return NotFound();
        
        var dto = Mapper.Map<ProductDto>(item);
        return Ok(dto);
    }
}
```

### 11. DI Configuration

```csharp
// Program.cs
builder.Services
    .UseEntities<AppDbContext>(config =>
    {
        config.UseDefaults();
        config.UseMapsterMapping();
    })
    .For<Product>(cfg =>
    {
        cfg.UseFilter<ProductQueryFilter>();
        cfg.UseSorting<ProductSortedQueryBuilder>();
        cfg.UseIncludes<ProductIncludableQueryBuilder>();
        cfg.UseProcessor<ProductProcessor>();
        cfg.UsePrepper<ProductPrepper>();
        cfg.Related(p => p.Category);
        cfg.Related(p => p.Reviews);
    });
```

### 12. Mapping Configuration

```csharp
// Configuration/MappingConfig.cs
TypeAdapterConfig<Product, ProductDto>.NewConfig()
    .Map(dest => dest.AverageRating, 
         src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0)
    .Map(dest => dest.ReviewCount, 
         src => src.Reviews.Count);

TypeAdapterConfig<ProductInput, Product>.NewConfig()
    .Ignore(dest => dest.Id)
    .Ignore(dest => dest.Slug)
    .Ignore(dest => dest.Created)
    .Ignore(dest => dest.LastModified)
    .Ignore(dest => dest.Category)
    .Ignore(dest => dest.Reviews);
```

---

## Example 3: Multi-Tenant Entity

### 1. Tenant Interface

```csharp
// Models/Abstractions/IHasTenantId.cs
public interface IHasTenantId
{
    int TenantId { get; set; }
}
```

### 2. Entity with Tenant

```csharp
// Models/Order.cs
public class Order : IEntity<Guid>, IHasTimestamps, IHasTenantId, IHasUserId
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
```

### 3. Tenant Provider

```csharp
// Services/TenantProvider.cs
public interface ITenantProvider
{
    int GetCurrentTenantId();
}

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public int GetCurrentTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("TenantId")?.Value;
        
        return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : 0;
    }
}
```

### 4. Tenant Primer

```csharp
// Services/Primers/TenantPrimer.cs
public class TenantPrimer : EntityPrimerBase<IHasTenantId>
{
    private readonly ITenantProvider _tenantProvider;
    
    public TenantPrimer(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }
    
    protected override Task ProcessAsync(
        IHasTenantId entity,
        EntityState state,
        CancellationToken cancellationToken)
    {
        if (state == EntityState.Added)
        {
            entity.TenantId = _tenantProvider.GetCurrentTenantId();
        }
        
        return Task.CompletedTask;
    }
}
```

### 5. Tenant Global Filter

```csharp
// Services/QueryFilters/TenantGlobalFilter.cs
public class TenantGlobalFilter : IGlobalFilteredQueryBuilder
{
    private readonly ITenantProvider _tenantProvider;
    
    public TenantGlobalFilter(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }
    
    public IQueryable<TEntity> Build<TEntity>(
        IQueryable<TEntity> query,
        object? search)
        where TEntity : class
    {
        if (typeof(IHasTenantId).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            query = query.Where(e => ((IHasTenantId)e).TenantId == tenantId);
        }
        
        return query;
    }
}
```

### 6. DI Configuration

```csharp
// Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services
    .UseEntities<AppDbContext>(config =>
    {
        config.UseDefaults();
        config.AddPrimer<TenantPrimer>();
        config.AddGlobalQueryFilter<TenantGlobalFilter>();
        config.UseMapsterMapping();
    })
    .For<Order>(cfg =>
    {
        cfg.Includes((q, _) => q.Include(x => x.Items));
    });
```

---

## Example 4: Hierarchical Entity

### 1. Entity Model

```csharp
// Models/Department.cs
public class Department : IEntity, IHasTitle, IHasParentEntity<Department>, ISortable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    
    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = new List<Department>();
}
```

### 2. DTO

```csharp
// Models/DTOs/DepartmentDto.cs
public class DepartmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentTitle { get; set; }
    public List<DepartmentDto> Children { get; set; } = new();
}
```

### 3. Service

```csharp
// Services/IDepartmentService.cs
public interface IDepartmentService : IEntityService<Department>
{
    Task<IEnumerable<Department>> GetRootDepartmentsAsync();
    Task<IEnumerable<Department>> GetChildrenAsync(int parentId);
}

public class DepartmentService : EntityManager<Department>, IDepartmentService
{
    public DepartmentService(IEntityService<Department> service)
        : base(service)
    {
    }
    
    public async Task<IEnumerable<Department>> GetRootDepartmentsAsync()
    {
        var all = await ListAsync();
        return all.Where(d => d.ParentId == null).OrderBy(d => d.SortOrder);
    }
    
    public async Task<IEnumerable<Department>> GetChildrenAsync(int parentId)
    {
        var all = await ListAsync();
        return all.Where(d => d.ParentId == parentId).OrderBy(d => d.SortOrder);
    }
}
```

### 4. DI Configuration

```csharp
builder.Services
    .UseEntities<AppDbContext>(config => config.UseDefaults())
    .For<Department>(cfg =>
    {
        cfg.SortBy((q, _) => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
        cfg.Includes((q, _) => q.Include(x => x.Parent).Include(x => x.Children));
    });

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
```

---

## Testing Examples

### Unit Test Example

```csharp
public class ProductServiceTests
{
    private readonly Mock<IEntityService<Product, int, ProductSearchObject>> _mockService;
    private readonly ProductService _productService;
    
    public ProductServiceTests()
    {
        _mockService = new Mock<IEntityService<Product, int, ProductSearchObject>>();
        _productService = new ProductService(_mockService.Object);
    }
    
    [Fact]
    public async Task List_FiltersByCategory_ReturnsProducts()
    {
        // Arrange
        var categoryId = 1;
        var searchObject = new ProductSearchObject { CategoryId = [categoryId] };
        var expectedProducts = new List<Product>
        {
            new() { Id = 1, Title = "Product 1", CategoryId = categoryId },
            new() { Id = 2, Title = "Product 2", CategoryId = categoryId }
        };
        
        _mockService
            .Setup(s => s.List(searchObject, null))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _mockService.Object.List(searchObject, null);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(categoryId, p.CategoryId));
    }
}
```

---

## Example 4: Entity with Child Collection (Invoice with Lines)

### 1. Entity Models

```csharp
// Models/Invoice.cs
public class Invoice : IEntity<Guid>, IHasTimestamps
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}

// Models/InvoiceLine.cs
public class InvoiceLine : IEntity
{
    public int Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    
    public Invoice? Invoice { get; set; }
}
```

### 2. DTOs

```csharp
// Models/DTOs/InvoiceDto.cs
public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public CustomerDto? Customer { get; set; }
    public List<InvoiceLineDto>? InvoiceLines { get; set; }
}

public class InvoiceLineDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

// Models/DTOs/InvoiceInput.cs
public class InvoiceInput
{
    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime InvoiceDate { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    public List<InvoiceLineInput> Lines { get; set; } = new();
}

public class InvoiceLineInput
{
    public int Id { get; set; } // 0 for new lines
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, 9999999)]
    public decimal Quantity { get; set; }
    
    [Range(0.01, 9999999)]
    public decimal UnitPrice { get; set; }
}
```

### 3. DI Configuration with Related

```csharp
builder.Services
    .UseEntities<AppDbContext>(config => config.UseDefaults())
    .For<Invoice>(cfg =>
    {
        cfg.Includes((q, _) => q.Include(x => x.Customer).Include(x => x.InvoiceLines));
        
        // Declare related child collection with prepare function
        cfg.Related(x => x.InvoiceLines, prepare: (invoice, inputLines) =>
        {
            // Map input lines to entity lines
            var lines = inputLines.Select(l => new InvoiceLine
            {
                Id = l.Id,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList();
            
            // Add new lines (Id == 0)
            foreach (var line in lines.Where(l => l.Id == 0))
            {
                line.InvoiceId = invoice.Id;
                invoice.InvoiceLines.Add(line);
            }
            
            // Update existing lines
            foreach (var line in lines.Where(l => l.Id > 0))
            {
                var existing = invoice.InvoiceLines.FirstOrDefault(il => il.Id == line.Id);
                if (existing != null)
                {
                    existing.Description = line.Description;
                    existing.Quantity = line.Quantity;
                    existing.UnitPrice = line.UnitPrice;
                }
            }
            
            // Remove deleted lines (not in input)
            var lineIds = lines.Select(l => l.Id).Where(id => id > 0).ToList();
            var toRemove = invoice.InvoiceLines
                .Where(il => !lineIds.Contains(il.Id))
                .ToList();
            
            foreach (var line in toRemove)
            {
                invoice.InvoiceLines.Remove(line);
            }
            
            // Recalculate total
            invoice.TotalAmount = invoice.InvoiceLines.Sum(l => l.LineTotal);
        });
    });
```

### 4. Usage Notes

**Important Points about Related:**
- The prepare function handles add, modify, and delete operations for child entities
- Related properties can only go **1 level deep** (Invoice → InvoiceLines, not Invoice → InvoiceLines → SubLines)
- The prepare function is called automatically by the framework when saving
- Perfect for managing one-to-many relationships with full CRUD support

---

## Example 5: Hierarchical Entity

### 1. Entity Model

```csharp
// Models/Department.cs
public class Department : IEntity, IHasTitle, IHasParentEntity<Department>, ISortable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    
    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = new List<Department>();
}
```

### 2. DTO

```csharp
// Models/DTOs/DepartmentDto.cs
public class DepartmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? ParentTitle { get; set; }
    public List<DepartmentDto> Children { get; set; } = new();
}
```

### 3. Service

```csharp
// Services/IDepartmentService.cs
public interface IDepartmentService : IEntityService<Department>
{
    Task<IEnumerable<Department>> GetRootDepartmentsAsync();
    Task<IEnumerable<Department>> GetChildrenAsync(int parentId);
}

public class DepartmentService : EntityManager<Department>, IDepartmentService
{
    public DepartmentService(IEntityService<Department> service)
        : base(service)
    {
    }
    
    public async Task<IEnumerable<Department>> GetRootDepartmentsAsync()
    {
        var all = await ListAsync();
        return all.Where(d => d.ParentId == null).OrderBy(d => d.SortOrder);
    }
    
    public async Task<IEnumerable<Department>> GetChildrenAsync(int parentId)
    {
        var all = await ListAsync();
        return all.Where(d => d.ParentId == parentId).OrderBy(d => d.SortOrder);
    }
}
```

### 4. DI Configuration

```csharp
builder.Services
    .UseEntities<AppDbContext>(config => config.UseDefaults())
    .For<Department>(cfg =>
    {
        cfg.SortBy((q, _) => q.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
        cfg.Includes((q, _) => q.Include(x => x.Parent).Include(x => x.Children));
    });

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
```

---

## Quick Reference: Step-by-Step Implementation

1. **Create Entity** with appropriate interfaces
2. **Create DTOs** (Output DTO, Input DTO)
3. **Create SearchObject** (if filtering needed)
4. **Create Enums** (SortBy, Includes if needed)
5. **Configure DbContext** with entity
6. **Create Query Builders** (Filter, Sort, Include)
7. **Create Processor** (if post-fetch decoration needed)
8. **Create Prepper** (if pre-save preparation needed)
9. **Create Controller** matching service signature
10. **Configure DI** with `.For<TEntity>()`
11. **Declare Related properties** (if managing child collections)
12. **Configure Mapping** (AutoMapper or Mapster)
13. **Test** endpoints

---

## Next Steps

Return to main instructions: [AI Instructions](AI-INSTRUCTIONS.md)
