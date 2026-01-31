# Entity Models & Interfaces - AI Agent Instructions

## Creating Entity Models

### Basic Entity Structure

Every entity should:
1. Implement `IEntity<TKey>` (or use default `IEntity` for int keys)
2. Have a primary key property named `Id`
3. Implement relevant marker interfaces based on properties
4. Be a POCO (Plain Old CLR Object) - data only, minimal behavior

### Entity Template

```csharp
public class Product : IEntity<int>, IHasTitle, IArchivable, IHasTimestamps
{
    public int Id { get; set; }
    
    [Required, MaxLength(64)]
    public string? Title { get; set; }
    
    [MaxLength(2048)]
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    
    // Navigation properties
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

**Recommended**: Use data annotations (MaxLength, Column) directly on entity properties, similar to InputDTOs. This provides clear schema definition and enables automatic string truncation.

## Entity Interfaces Reference

### Essential Interfaces

| Interface | Properties | When to Use | Auto-Processing |
|-----------|-----------|-------------|-----------------|
| `IEntity` | Id (int) | Every entity with int PK | Required |
| `IEntity<TKey>` | Id (TKey) | Every entity | Required |
| `IHasTitle` | Title (string) | Entities with name/title | Used for display |

### Identity & Keys

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IEntityWithSerial` | Serial (int) | Auto-incrementing int ID |
| `IHasAggregateKey` | AggregateKey (Guid) | Entities needing Guid identifier |
| `IHasCode` | Code (string) | Entities with short code/SKU |
| `IHasSlug` | Slug (string) | Entities with URL-friendly identifier |

### Hierarchy & Relationships

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasObjectId<TKey>` | ObjectId (TKey) | Foreign key without navigation property |
| `IHasParentEntity` | Parent (same type) | Self-referential hierarchy |
| `IHasParentEntity<T>` | Parent (T) | Parent-child relationship |

### Timestamps & Auditing

| Interface | Properties | When to Use | Primer Available |
|-----------|-----------|-------------|------------------|
| `IHasCreated` | Created (DateTime) | Track creation time | ✓ HasCreatedDbPrimer |
| `IHasLastModified` | LastModified (DateTime?) | Track modification time | ✓ HasLastModifiedDbPrimer |
| `IHasTimestamps` | Created, LastModified | Both timestamps | ✓ Both primers |

**Important**: When using timestamp interfaces, register the corresponding primers in DI.

### State & Flags

| Interface | Properties | When to Use | Filter Available |
|-----------|-----------|-------------|------------------|
| `IArchivable` | IsArchived (bool) | Soft delete capability | ✓ FilterArchivablesQueryBuilder |
| `IHasDefault` | IsDefault (bool) | Mark default item | - |
| `IHasDefault<TKey>` | IsDefault (bool), Id (TKey) | Default with typed ID | - |

### Content & Description

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasDescription` | Description (string) | Entities with description field |
| `IHasNormalizedContent` | NormalizedContent (string) | Search/comparison optimization |
| `IHasNormalizedTitle` | Title, NormalizedTitle (string) | Searchable title |

### Date Ranges

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasStartDate` | StartDate (DateTime?) | Items with start time |
| `IHasEndDate` | EndDate (DateTime?) | Items with end time |
| `IHasStartEndDate` | StartDate, EndDate (DateTime?) | Date range (events, promotions) |

### Sorting & Ordering

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `ISortable` | SortOrder (int) | Custom display order |

### Security & Users

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasUserId` | UserId (string) | User-owned entities |
| `IHasPassword` | Password (string, read-only) | Password storage |
| `IHasEncryptedPassword` | EncryptedPassword (string) | Encrypted password storage |

### Other

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasUri` | Uri (string) | Entities with URL/URI |

## Interface Selection Guidelines

### Decision Tree

1. **Does it have an ID?** → `IEntity<TKey>` (or `IEntity` for int)
2. **Does it have a name/title?** → Add `IHasTitle`
3. **Should it support soft delete?** → Add `IArchivable`
4. **Should it track creation/modification?** → Add `IHasTimestamps`
5. **Does it belong to a user?** → Add `IHasUserId`
6. **Does it have a hierarchy?** → Add `IHasParentEntity<T>`
7. **Does it need custom ordering?** → Add `ISortable`

### Common Combinations

**Basic Entity:**
```csharp
public class Category : IEntity, IHasTitle
```

**Full-Featured Entity:**
```csharp
public class Article : IEntity<Guid>, IHasTitle, IArchivable, IHasTimestamps, IHasUserId, IHasSlug
```

**Hierarchical Entity:**
```csharp
public class Department : IEntity, IHasTitle, IHasParentEntity<Department>, ISortable
```

## Creating DTOs

### DTO Types

1. **Output DTO**: Entity representation for API responses (both details and lists)
2. **Input DTO**: For create/update operations

**Note**: Typically, a single DTO type is used for both detail and list views. When returning lists, navigation properties may be null unless explicitly included via the `TInclude` parameter.

### Output DTO

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Related entities - may be null when not included
    public CategoryDto? Category { get; set; }
    public ICollection<ReviewDto>? Reviews { get; set; }
}
```

**Usage Pattern:**
- **Detail view**: `GET /api/products/5?include=Category,Reviews` → All properties populated
- **List view**: `GET /api/products` → Navigation properties typically null unless explicitly included

### Input DTO

```csharp
public class ProductInput
{
    [Required, MaxLength(64)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2048)]
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
}
```

## SearchObject

Use SearchObject for filtering entities in list queries.

```csharp
public class ProductSearchObject : SearchObject
{
    public string? Title { get; set; }
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Inherited from SearchObject:
    // - int? Id
    // - ICollection<int>? Ids
    // - ICollection<int>? Exclude
    // - string? Q
    // - DateTime? MinCreated
    // - DateTime? MaxCreated
    // - DateTime? MinLastModified
    // - DateTime? MaxLastModified
    // - bool? IsArchived
}
```

**Important Notes**:
- `SearchObject` base class provides common filtering properties
- **QueryString Compatible**: SearchObject properties are automatically mapped from querystring in controllers
- **Foreign key properties should use `ICollection<TKey>`** for flexibility (e.g., `ICollection<int>? CategoryId`)
- This allows filtering by single value (`new[] { 1 }`) or multiple values (`new[] { 1, 2, 3 }`)
- Use `.Any()` and `.Contains()` in query filters to handle collection properties

**Q Property (General Text Search)**:
- The `Q` property serves as a general text search field
- Typically used when entity implements `IHasTitle` or `IHasDescription`
- Developers can add custom filtering logic in query filters
- Use `QKeywordHelper` for wildcard support (*) in search queries
- Example: `?q=product*` will search for items starting with "product"

## Enums

### SortBy Enum

```csharp
public enum ProductSortBy
{
    Default = 0,
    Title = 1,
    Price = 2,
    Created = 3,
    Popular = 4
}
```

### Includes Enum

```csharp
[Flags]
public enum ProductIncludes
{
    None = 0,
    Category = 1,
    Reviews = 2,
    Images = 4,
    All = Category | Reviews | Images
}
```

**Important**: Use `[Flags]` attribute for includes enum to support multiple includes.

## Entity Configuration

### DbContext Configuration

**Recommended Approach** (using attributes on entities):

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set decimal precision convention for all decimal properties
        modelBuilder.SetDecimalPrecisionConvention(18, 2);
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            
            // Relationship configuration
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Indexes
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.CategoryId);
        });
    }
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // Automatically truncate strings to MaxLength before saving
        this.AutoTruncateStringsToMaxLengthForEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // Automatically truncate strings to MaxLength before saving
        this.AutoTruncateStringsToMaxLengthForEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
```

**Key Features:**
- **SetDecimalPrecisionConvention**: Extension method on ModelBuilder/ModelConfigurationBuilder to set decimal precision globally
- **AutoTruncateStringsToMaxLengthForEntries**: Extension method on DbContext that automatically truncates strings to their MaxLength attribute value
- Use attributes on entity properties instead of Fluent API when possible
- Reserve Fluent API for relationships, indexes, and configurations not supported by attributes

## Best Practices

### DO:
- ✓ Use interface-based design for common properties
- ✓ Keep entities focused on data structure
- ✓ Use nullable reference types appropriately
- ✓ Add data annotations to both entities and InputDTOs (MaxLength, Column, etc.)
- ✓ Use SetDecimalPrecisionConvention for consistent decimal handling
- ✓ Use AutoTruncateStringsToMaxLengthForEntries in SaveChanges methods
- ✓ Use a single DTO type for both details and lists
- ✓ Use descriptive property names
- ✓ Make navigation properties nullable in DTOs

### DON'T:
- ✗ Add business logic to entities
- ✗ Expose entities directly in APIs
- ✗ Mix concerns (use separate interfaces)
- ✗ Forget to configure relationships in DbContext
- ✗ Use magic strings or numbers

## Validation

Add validation to InputDTOs, not entities:

```csharp
public class ProductInput
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }
    
    [Url]
    public string? Website { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
}
```

## Next Steps

- Configure services: [Services Instructions](AI-INSTRUCTIONS-SERVICES.md)
- Create API endpoints: [Controllers Instructions](AI-INSTRUCTIONS-CONTROLLERS.md)
- See complete examples: [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
