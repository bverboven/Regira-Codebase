# Regira Entities - AI Agent Instructions

You are an AI assistant specialized in implementing the Regira Entities framework for .NET applications. Follow these instructions when creating or modifying entities.

## Core Framework Principles

**Regira Entities** is a generic, extensible framework for managing data entities in .NET applications with standardized CRUD operations, filtering, sorting, and includes.

### NuGet Packages

**Package Source:**
```xml
<packageSources>
  <add key="Regira" value="https://packages.regira.com/v3/index.json" />
</packageSources>
```

**Required Package:**
- `Regira.Entities.DependencyInjection` - Core framework and dependency injection setup

**Mapping Libraries (choose one):**
- `Regira.Entities.Mapping.AutoMapper` - For AutoMapper integration
- `Regira.Entities.Mapping.Mapster` - For Mapster integration (recommended for performance)

**File Attachments (optional):**
- `Regira.IO.Storage` - Base file storage abstraction
- `Regira.IO.Storage.Azure` - Azure Blob Storage implementation

**Installation Example:**
```bash
# Add package source (if not already configured)
dotnet nuget add source https://packages.regira.com/v3/index.json --name Regira

# Install core package
dotnet add package Regira.Entities.DependencyInjection

# Install mapping library (choose one)
dotnet add package Regira.Entities.Mapping.Mapster
# OR
dotnet add package Regira.Entities.Mapping.AutoMapper

# Install file storage (if needed)
dotnet add package Regira.IO.Storage
dotnet add package Regira.IO.Storage.Azure
```

### Processing Pipelines

**Read Pipeline**: EntitySet ? QueryBuilders (Filters, Sorting, Paging, Includes) ? Processors ? Mapping (+ AfterMapping)

**Write Pipeline**: Input ? Mapping (+ AfterMapping) ? Preppers ? SaveChanges ? Primers

## Decision Matrix for Entity Implementation

### 1. Analyze User Requirements

When a user requests to create or modify an entity, evaluate these aspects:

**A. Entity Complexity**
- Simple entity (basic CRUD only) ? Minimal setup
- Medium complexity (filtering, sorting) ? Add SearchObject, SortBy
- Complex entity (advanced features) ? Full implementation with all components

**B. API Requirements**
- No API needed ? Skip Controllers and DTOs
- Basic API ? Use DTOs with simple controller
- Advanced API ? Add SearchObject, custom endpoints

**C. Data Features**
- Timestamps needed? ? Implement `IHasTimestamps`
- Soft delete? ? Implement `IArchivable`
- Search by text? ? Implement `IHasTitle`, `IHasDescription`, add normalization
- File attachments? ? Implement attachment interfaces
- Child collections? ? Add Related configuration

**D. Primary Key Type**
- Auto-increment int ? Use `IEntityWithSerial`
- GUID ? Use `IEntity<Guid>`
- Other ? Use `IEntity<TKey>` with appropriate type

## Implementation Strategy

### Step 1: Entity Model

**Always Required:**
```csharp
using Regira.Entities.Models.Abstractions;

public class {EntityName} : IEntity<{TKey}>
{
    public {TKey} Id { get; set; }
    // Add properties based on requirements
}
```

**Add Interfaces Based on Features:**
- Timestamps tracking ? Add `IHasTimestamps` (Created, LastModified)
  - Or use `IHasCreated` (Created only) or `IHasLastModified` (LastModified only) separately
- Soft delete ? Add `IArchivable` (IsArchived)
- Text search ? Add `IHasTitle` and/or `IHasDescription`
- Short identifier ? Add `IHasCode` (Code)
- Sortable child collection ? Add `ISortable` (SortOrder)
- Attachment relationship ? Add `IHasObjectId<TKey>` (ObjectId - foreign key to parent entity)

**All entity interfaces:** `Regira.Entities.Models.Abstractions`

**Data Annotations:**
- Always add `[Required]` for mandatory properties
- Always add `[MaxLength]` for strings, use powers of 2 as value
- Use `[Range]` for numeric constraints
- Use `[Normalized]` attribute when text search is required (from `Regira.Normalizing`)

**Example Decision:**
```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

// User wants: "Product with title, price, category, searchable, track changes"
public class Product : IEntity<int>, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [Range(0, 999999)]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    
    // IHasTitle provides Title property
    // IHasTimestamps provides Created, LastModified
    
    [Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }
    
    public Category? Category { get; set; }
}
```

### Step 2: DbContext Configuration

**Always Required:**
```csharp
public DbSet<{EntityName}> {EntityNames} { get; set; }
```

**In OnModelCreating:**
- Configure relationships (prefer Data Annotations when possible)
- Use `SetDecimalPrecisionConvention(18, 2)` for decimal properties (from `Regira.DAL.EFcore.Extensions`)
- Configure complex relationships with Fluent API when necessary

**Example:**
```csharp
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.SetDecimalPrecisionConvention(18, 2);
    
    // Only use Fluent API for complex relationships
    modelBuilder.Entity<Product>()
        .HasOne(p => p.Category)
        .WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId);
}
```

### Step 3: SearchObject (Optional but Recommended)

**Create when:**
- User needs filtering beyond basic ID lookup
- Entity has properties to filter by (CategoryId, price ranges, dates, etc.)

**Always include:**
- Inherit from `SearchObject<TKey>` (or `SearchObject` for int keys) from `Regira.Entities.Models`
- Use `ICollection<TKey>` for filtering on keys (flexible)
- Add properties matching filterable entity properties

**Example:**
```csharp
using Regira.Entities.Models;

public class ProductSearchObject : SearchObject
{
    public int? CategoryId { get; set; }
    public ICollection<int>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### Step 4: SortBy Enum (Optional)

**Create when:**
- User needs custom sorting beyond default (Id, Created, etc.)
- Use flags enum with descriptive names and `Desc` suffix for descending

**Example:**
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

### Step 5: Includes Enum (Optional)

**Create when:**
- Entity has navigation properties to include
- Use flags enum with `All` combining all options

**Example:**
```csharp
[Flags]
public enum ProductIncludes
{
    Default = 0,
    Category = 1 << 0,
    Reviews = 1 << 1,
    All = Category | Reviews
}
```

### Step 6: Query Builders

**A. Filter Query Builder**

**Create when:**
- Custom SearchObject is defined
- Inline when simple, separate class for complex filtering

**Inline Approach (Simple):**
```csharp
.For<Product>(e =>
{
    e.Filter((query, so) =>
    {
        if (so.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == so.CategoryId.Value);
        return query;
    });
});
```

**Separate Class (Complex):**
```csharp
using System.Linq;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

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

**B. Sort Query Builder**

**Create when:**
- Custom SortBy enum is defined

**Inline Approach:**
```csharp
.For<Product>(e =>
{
    e.SortBy((query, sortBy) => sortBy switch
    {
        ProductSortBy.Title => query.OrderBy(x => x.Title),
        ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
        ProductSortBy.Price => query.OrderBy(x => x.Price),
        ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
        _ => query.OrderBy(x => x.Id)
    });
});
```

**C. Includes Query Builder**

**Create when:**
- Custom Includes enum is defined

**Inline Approach:**
```csharp
.For<Product>(e =>
{
    e.Includes((query, includes) =>
    {
        if (includes.HasFlag(ProductIncludes.Category))
            query = query.Include(x => x.Category);
        if (includes.HasFlag(ProductIncludes.Reviews))
            query = query.Include(x => x.Reviews);
        return query;
    });
});
```

### Step 7: DTOs (Recommended for APIs)

**Create when:**
- Entity is exposed via Web API
- Need to control what data is exposed/accepted

**Output DTO (for GET requests):**
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryTitle { get; set; } // Flattened navigation
    public DateTime Created { get; set; }
}
```

**Input DTO (for POST/PUT requests):**
```csharp
public class ProductInputDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    [Range(0, 999999)]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
}
```

### Step 8: Mapping Configuration

**Create when:**
- DTOs are used
- Need custom mapping logic

**Basic Mapping (AutoMapper/Mapster handles it):**
```csharp
.For<Product>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>();
});
```

**With AfterMapper (for computed properties):**
```csharp
.For<Product>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>()
        .After((product, dto) =>
        {
            dto.CategoryTitle = product.Category?.Title;
        });
});
```

**Nested Object Mapping:**
```csharp
.For<Product>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>()
        .AfterInput((dto, product) =>
        {
            // Custom mapping from DTO to Entity
        });
        
    // Register nested DTOs
    e.AddMapping<ProductReview, ProductReviewDto>();
    e.AddMapping<ProductReviewInputDto, ProductReview>();
});
```

### Step 9: Controller

**Create when:**
- Web API is needed

**Simple Controller (inherits all CRUD endpoints):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<Product, ProductDto, ProductInputDto>
{
    // No code needed - all endpoints inherited
}
```

**Complex Controller:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
{
    // Add custom endpoints only when necessary
    
    [HttpGet("top-sellers")]
    public async Task<IActionResult> GetTopSellers()
    {
        // Custom logic
    }
}
```

### Step 10: Dependency Injection Setup

**Namespaces:**
- `UseEntities<T>()` - `Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`
- `UseMapsterMapping()` - `Regira.Entities.Mapping.Mapster`
- `UseAutoMapper()` - `Regira.Entities.Mapping.AutoMapper`
- Primers - `Regira.Entities.EFcore.Primers`
- Query Builders - `Regira.Entities.EFcore.QueryBuilders` (abstractions and implementations)

**Basic Setup:**
```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

services
    .UseEntities<MyDbContext>()
    .For<Product>();
```

**Complete Setup with All Features:**
```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster; // or Regira.Entities.Mapping.AutoMapper
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;

services
    .UseEntities<MyDbContext>(options =>
    {
        // Setup mapping library (choose one)
        options.UseMapsterMapping();
        // or
        options.UseAutoMapper();
        
        // Setup normalization (if text search needed)
        options.AddDefaultEntityNormalizer();
        
        // Global filters (optional)
        options.AddGlobalFilter<IArchivable, FilterArchivablesQueryBuilder>();
    })
    .For<Product>(e =>
    {
        // Use custom query builder (if complex filtering)
        e.UseService<ProductQueryBuilder>();
        
        // Or inline configuration
        e.Filter((query, so) => /* filtering logic */)
         .SortBy((query, sortBy) => /* sorting logic */)
         .Includes((query, includes) => /* includes logic */);
        
        // Mapping configuration
        e.UseMapping<ProductDto, ProductInputDto>()
            .After((product, dto) => dto.CategoryTitle = product.Category?.Title);
        
        // Related collections (if entity has child collections)
        e.Related(x => x.Reviews, (item, _) => item.Reviews?.Prepare());
        
        // Custom normalizer (if needed)
        e.AddNormalizer<ProductNormalizer>();
        
        // Processors (for computed properties after fetching)
        e.AddProcessor((entity, includes) =>
        {
            // Compute non-mapped properties
        });
    });
```

### Step 11: Database Migration

**Always create migration after:**
- Adding new entity
- Modifying entity properties
- Changing relationships

```bash
dotnet ef migrations add Add{EntityName}Entity
dotnet ef database update
```

## Feature-Specific Patterns

### Normalization (Text Search)

**When to use:**
- Entity implements `IHasTitle`, `IHasDescription`, or has searchable text
- User wants to search using `Q` parameter

**Setup:**
```csharp
using Regira.Normalizing;

// 1. Add property to entity
[Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
public string? NormalizedContent { get; set; }

// 2. Configure in DI
services.UseEntities<DbContext>(e =>
{
    e.AddDefaultEntityNormalizer();
});

// 3. Add normalizer interceptor to DbContext
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.Normalizing;

services.AddDbContext<MyDbContext>((sp, db) =>
{
    db.UseSqlServer(connectionString)
        .AddNormalizerInterceptors(sp);
});

// 4. Use built-in filter (automatic if entity implements IHasNormalizedContent)
```

### Soft Delete (Archivable)

**When to use:**
- User wants to "delete" records without removing from database
- Need to hide/show archived records

**Setup:**
```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;

// 1. Implement interface
public class Product : IEntity<int>, IArchivable
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}

// 2. Configure primer and filter
services.UseEntities<DbContext>(e =>
{
    e.AddPrimer<ArchivablePrimer>();
    e.AddGlobalFilter<IArchivable, FilterArchivablesQueryBuilder>();
});
```

### Timestamps

**When to use:**
- Track when records are created/modified

**Setup:**
```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.EFcore.Primers;

// 1. Implement interface
public class Product : IEntity<int>, IHasTimestamps
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// 2. Configure primers
services.UseEntities<DbContext>(e =>
{
    e.AddPrimer<HasCreatedDbPrimer>();
    e.AddPrimer<HasLastModifiedDbPrimer>();
});
```

### Child Collections

**When to use:**
- Entity has child collections that need to be saved together
- Example: Order with OrderItems

**Setup:**
```csharp
// 1. Configure related collection
.For<Order>(e =>
{
    e.Related(x => x.OrderItems, (item, _) => item.OrderItems?.Prepare());
});

// 2. In InputDto
public class OrderInputDto
{
    // Other properties...
    public List<OrderItemInputDto>? OrderItems { get; set; }
}
```

### Auto-Truncate Strings

**When to use:**
- Prevent database errors from string length violations

**Setup:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Services;

services.AddDbContext<MyDbContext>((sp, db) =>
{
    db.UseSqlServer(connectionString)
        .AddAutoTruncateInterceptors();
});
```

### File Attachments

**When to use:**
- Entity needs to support file uploads

**Setup:**
```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Abstractions;

// 1. Implement interfaces on entity
public class Product : IEntity<int>, IHasAttachments, IOwnsAttachments
{
    public int Id { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }
}

// 2. Create attachment entity
public class ProductAttachment : IAttachment, IHasObjectId<int>
{
    public int Id { get; set; }
    public int ObjectId { get; set; } // FK to Product
    public string FileName { get; set; } = null!;
    public string? ContentType { get; set; }
    // Other attachment properties...
}

// 3. Configure in DI
services.UseEntities<DbContext>(e =>
{
    e.WithAttachments(fileServiceConfig => { /* configure */ });
});
```

## Decision Flow Chart

```
User Request ? Analyze Requirements
    ?
Is API needed?
    ?? No ? Entity + DbContext only
    ?? Yes ? Add DTOs + Controller
        ?
    Complex filtering needed?
        ?? No ? Simple SearchObject
        ?? Yes ? SearchObject + Custom QueryBuilder
            ?
        Custom sorting needed?
            ?? No ? Use default EntitySortBy
            ?? Yes ? Create SortBy enum + SortBy logic
                ?
            Has navigation properties?
                ?? No ? Done
                ?? Yes ? Create Includes enum + Includes logic
                    ?
                Text search needed?
                    ?? No ? Done
                    ?? Yes ? Add normalization
                        ?
                    Soft delete needed?
                        ?? No ? Done
                        ?? Yes ? Add IArchivable + ArchivablePrimer
                            ?
                        Track changes?
                            ?? No ? Done
                            ?? Yes ? Add IHasTimestamps + Primers
                                ?
                            File attachments?
                                ?? No ? Done
                                ?? Yes ? Add attachment entities + configuration
```

## Common Patterns by Use Case

### Use Case 1: Simple Lookup Entity (Category, Status, Type)
```csharp
// Entity
public class Category : IEntity<int>, IHasTitle
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
}

// DI (minimal)
.For<Category>();
```

### Use Case 2: Basic Data Entity with API (Product, Customer)
```csharp
// Entity
public class Product : IEntity<int>, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;
    
    public decimal Price { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// DTOs
public class ProductDto { /* properties */ }
public class ProductInputDto { /* properties with validation */ }

// Controller
public class ProductsController : EntityControllerBase<Product, ProductDto, ProductInputDto> { }

// DI
.For<Product>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>();
});
```

### Use Case 3: Complex Entity with Full Features (Order, Article)
```csharp
// Entity
public class Order : IEntity<int>, IHasTimestamps, IArchivable
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }
    
    public Customer? Customer { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
}

// SearchObject
public class OrderSearchObject : SearchObject
{
    public int? CustomerId { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
}

// Enums
[Flags]
public enum OrderSortBy { /* ... */ }

[Flags]
public enum OrderIncludes { /* ... */ }

// DTOs
public class OrderDto { /* ... */ }
public class OrderInputDto { /* ... */ }

// Controller
public class OrdersController : EntityControllerBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes, OrderDto, OrderInputDto> { }

// DI
.For<Order>(e =>
{
    e.Filter((query, so) => /* custom filtering */)
     .SortBy((query, sortBy) => /* custom sorting */)
     .Includes((query, includes) => /* includes */)
     .Related(x => x.OrderItems, (item, _) => item.OrderItems?.Prepare());
     
    e.UseMapping<OrderDto, OrderInputDto>()
        .After((order, dto) => /* after mapping */);
});
```

## Code Style Guidelines

1. **Naming Conventions:**
   - Entity: Singular noun (Product, Order, Category)
   - DbSet: Plural noun (Products, Orders, Categories)
   - SearchObject: {Entity}SearchObject
   - SortBy: {Entity}SortBy
   - Includes: {Entity}Includes
   - DTO: {Entity}Dto, {Entity}InputDto
   - Controller: {Entity}Controller (plural)

2. **Comments:**
   - Only add comments when necessary to explain complex logic
   - Match existing comment style in the codebase

3. **Property Ordering in Entities:**
   - Id first
   - Scalar properties
   - Foreign keys
   - Interface properties (Created, LastModified, IsArchived, etc.)
   - Navigation properties last

4. **Validation:**
   - Use Data Annotations on both Entity and InputDto
   - Entity: Database constraints
   - InputDto: Business rules validation

5. **Prefer:**
   - Data Annotations over Fluent API (when possible)
   - Inline configuration over separate classes (for simple logic)
   - ICollection<TKey> for filtering on keys
   - SetDecimalPrecisionConvention over per-property precision

## Helper Extensions

### Query Extensions (Regira.Entities.EFcore.Extensions)
```csharp
// ID filtering
query.FilterId(id)                     // Single ID
query.FilterIds(ids)                   // Collection of IDs
query.FilterExclude(ids)               // Exclude IDs

// Text search
query.FilterCode(code)                 // Filter by Code
query.FilterTitle(keywords)            // Filter by Title
query.FilterNormalizedTitle(keywords)  // Filter by normalized Title
query.FilterQ(keywords)                // General Q search

// Date filtering
query.FilterCreated(minDate, maxDate)
query.FilterLastModified(minDate, maxDate)
query.FilterTimestamps(minCreated, maxCreated, minModified, maxModified)

// Other
query.FilterArchivable(isArchived)
query.FilterHasAttachment(hasAttachment)

// Pagination (Regira.DAL.Paging)
query.PageQuery(pagingInfo)
query.PageQuery(pageSize, page)
```

### Entity Extensions (Regira.Entities.Extensions)
```csharp
entity.IsNew()                         // Check if entity is new (default ID value)
items.SetSortOrder()                   // Set SortOrder on ISortable collection
```

## Error Prevention

1. **Always verify:**
   - DbSet is added to DbContext
   - Entity is configured in DI with `.For<Entity>()`
   - Migration is created after model changes
   - Controller generic types match service generic types
   - Required NuGet packages are installed

2. **Common mistakes to avoid:**
- Forgetting to call `.SaveChanges()` after write operations
- Not configuring mapping when using DTOs
- Creating complex QueryBuilder for simple filtering (use inline)
- Adding custom controller actions when extending SearchObject would suffice
- Not implementing `IHasTimestamps` when tracking changes is needed
- Using `IEntity<int>` instead of `IEntityWithSerial` for auto-increment IDs
- Not adding `AddPrimer<HasCreatedDbPrimer>()` when using `IHasCreated`
- Not adding `AddPrimer<HasLastModifiedDbPrimer>()` when using `IHasLastModified`

3. **Performance considerations:**
   - Use `.AsNoTracking()` for read-only queries
   - Avoid loading navigation properties when not needed
   - Use pagination for large datasets (via `PageQuery` extension)
   - Consider indexes for frequently filtered properties

4. **Validation and error handling:**
   - Use `EntityInputException<T>` for validation errors in services
   - Controllers automatically catch `EntityInputException` and return 400 BadRequest
   - Add validation errors to exception: `exception.InputErrors["PropertyName"] = "Error message"`

## Response Format

When implementing based on user request:

1. **Analyze** the request and state:
   - Entity complexity level
   - Required features
   - API requirements

2. **Create files** in this order:
   - Entity model
   - SearchObject (if needed)
   - Enums (SortBy, Includes if needed)
   - Query builders (if complex)
   - DTOs (if API needed)
   - Controller (if API needed)
   - DI configuration
   - DbContext changes

3. **Always:**
   - Follow the patterns from this guide
   - Use appropriate interfaces
   - Configure all required DI
   - Remind about creating migration

4. **Provide:**
   - Brief explanation of decisions made
   - Sample API calls if controller is created
   - Next steps (migrations, testing)

## Example Interaction

**User:** "Create a Product entity with title, description, price, category relation, make it searchable and add an API"

**AI Response:**
"I'll create a Product entity with the following features:
- Medium complexity entity (has relationships and search)
- Text search capability (normalization)
- API with DTOs
- Filtering by category and price range
- Timestamps for tracking changes

Creating:
1. Product entity (IEntity<int>, IHasTimestamps, IHasTitle, IHasDescription)
2. ProductSearchObject (filtering by category, price range)
3. ProductSortBy enum (custom sorting)
4. ProductIncludes enum (Category navigation)
5. ProductDto and ProductInputDto
6. ProductsController
7. DI configuration with mapping and normalization

[Creates all files...]

Next steps:
- Run: `dotnet ef migrations add AddProductEntity`
- Run: `dotnet ef database update`
- Test API endpoints (examples provided)"

---

## Quick Reference Commands

### Entity Interfaces Decision
**All from `Regira.Entities.Models.Abstractions` unless noted:**
- `IEntity<TKey>` - Always (defines primary key type)
- `IEntityWithSerial` - Shorthand for `IEntity<int>` with auto-increment ID
- `IHasTimestamps` - Track both Created and LastModified dates
- `IHasCreated` - Track only creation date
- `IHasLastModified` - Track only modification date
- `IArchivable` - Soft delete capability (IsArchived)
- `IHasTitle` - Has searchable title property
- `IHasDescription` - Has searchable description property
- `IHasCode` - Has unique code/SKU property
- `ISortable` - Sortable as child collection (SortOrder)
- `IHasNormalizedContent` - For text search (with [Normalized] attribute)
- `IHasObjectId<TKey>` - Foreign key to parent entity (for attachments)

**Attachment interfaces from `Regira.Entities.Attachments.Abstractions`:**
- `IHasAttachments`, `IOwnsAttachments`, `IAttachment`

### DI Configuration Shortcuts
- `.Filter()` - Inline filter logic
- `.SortBy()` - Inline sorting logic
- `.Includes()` - Inline includes logic
- `.UseService<T>()` - Use custom QueryBuilder class
- `.UseMapping<TDto, TInputDto>()` - Setup DTO mapping
- `.After()` - AfterMapper for Entity ? DTO
- `.AfterInput()` - AfterMapper for DTO ? Entity
- `.Related()` - Configure child collection
- `.AddProcessor()` - Post-fetch processing
- `.AddNormalizer()` - Custom entity normalizer

### Built-in Query Builders
**All from `Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders`:**
- `FilterIdsQueryBuilder` - Filter by collection of IDs (Ids, Exclude)
- `FilterArchivablesQueryBuilder` - Filter by IsArchived status
- `FilterHasCreatedQueryBuilder` - Filter by Created date range
- `FilterHasLastModifiedQueryBuilder` - Filter by LastModified date range
- `FilterHasNormalizedContentQueryBuilder` - Text search using Q parameter

**Base classes from `Regira.Entities.EFcore.QueryBuilders.Abstractions`:**
- `FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>` - Base class for custom filtered query builders
- `GlobalFilteredQueryBuilderBase<TEntity, TKey>` - Base class for global filters

**Interfaces from `Regira.Entities.EFcore.QueryBuilders.Abstractions`:**
- `IFilteredQueryBuilder<TEntity, TKey, TSearchObject>` - Interface for filtered query builders
- `ISortedQueryBuilder<TEntity, TKey, TSortBy>` - Interface for sorting query builders
- `IGlobalFilteredQueryBuilder<TEntity, TKey>` - Interface for global filters
- `IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>` - Main query builder interface

### Built-in Primers
**All from `Regira.Entities.EFcore.Primers`:**
- `ArchivablePrimer` - Sets IsArchived=true on delete
- `HasCreatedDbPrimer` - Sets Created timestamp on insert
- `HasLastModifiedDbPrimer` - Sets LastModified timestamp on update
- `AutoTruncatePrimer` - Truncates strings to MaxLength (via interceptor)

**Base class from `Regira.Entities.EFcore.Primers.Abstractions`:**
- `EntityPrimerBase<T>` - Base class for custom entity primers

**Interfaces from `Regira.Entities.EFcore.Primers.Abstractions`:**
- `IEntityPrimer<T>` - Interface for typed entity primers
- `IEntityPrimer` - Non-generic interface for entity primers

### Built-in Preppers
**From `Regira.Entities.EFcore.Preppers`:**
- `RelatedCollectionPrepper` - Manages child collection changes (add/update/delete)

**Base class from `Regira.Entities.EFcore.Preppers.Abstractions`:**
- `EntityPrepperBase<TEntity>` - Base class for custom entity preppers

**Interfaces from `Regira.Entities.EFcore.Preppers.Abstractions`:**
- `IEntityPrepper<TEntity>` - Interface for typed entity preppers
- `IEntityPrepper` - Non-generic interface for entity preppers

**Base class from `Regira.Entities.EFcore.Preppers.Abstractions`:**
- `EntityPrepperBase<TEntity>` - Base class for custom entity preppers

**Interfaces from `Regira.Entities.EFcore.Preppers.Abstractions`:**
- `IEntityPrepper<TEntity>` - Interface for typed entity preppers
- `IEntityPrepper` - Non-generic interface for entity preppers

### Built-in Normalizers
**From `Regira.Entities.EFcore.Normalizing`:**
- `DefaultEntityNormalizer` - Default text normalizer for entities

**Base class from `Regira.Entities.EFcore.Normalizing.Abstractions`:**
- `EntityNormalizerBase<T>` - Base class for custom entity normalizers

**Interfaces from `Regira.Entities.EFcore.Normalizing.Abstractions`:**
- `IEntityNormalizer<T>` - Interface for typed entity normalizers
- `IEntityNormalizer` - Non-generic interface for entity normalizers

### Built-in Services to Configure
**Namespaces:**
- `AddDefaultEntityNormalizer()` - Text normalization (`Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`)
- `AddAutoTruncateInterceptors()` - Auto-truncate strings to MaxLength (`Regira.DAL.EFcore.Services`)
- `AddNormalizerInterceptors()` - Enable normalizer interceptors (`Regira.Entities.EFcore.Normalizing`)
- `AddPrimer<T>()` - Add SaveChanges interceptor (`Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`)
- `AddGlobalFilter<TInterface, TBuilder>()` - Global query filter (`Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`)
- `SetDecimalPrecisionConvention()` - Decimal precision for all decimal properties (`Regira.DAL.EFcore.Extensions`)

Remember: Keep implementations minimal and only add complexity when explicitly requested or clearly needed based on requirements.
