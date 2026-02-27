# Regira Entities — Normalizing Agent

You are a specialized agent responsible for implementing **data normalization** in the Regira Entities framework. This includes configuring normalized search fields, custom normalizers, and normalized keyword filtering.

---

## What Is Normalization?

Normalization transforms text for reliable search by:
- Removing diacritics/accents (e.g. `café` → `cafe`)
- Removing or standardizing special characters
- Normalizing whitespace
- Converting to lowercase
- Optionally formatting phone numbers, codes, etc.

A normalized property is typically a **single combined string** built from one or more source properties, stored on the entity for efficient LIKE queries.

---

## When to Use Normalization

- Entity has a `Q` search field (text search on `Title`, `Description`, etc.)
- Entity implements `IHasNormalizedContent`
- Phone or code fields need standardized formatting for search

---

## Architecture

| Service | Interface | Role |
|---------|-----------|------|
| String normalizer | `INormalizer` | Transforms a single string |
| Object normalizer | `IObjectNormalizer` | Processes `[Normalized]` attributes on a POCO |
| Entity normalizer | `IEntityNormalizer<T>` | Custom business logic for a specific entity type |

---

## Attribute-Based Normalization (Recommended)

Use `[Normalized]` on a property to have it auto-populated by the normalizer interceptor.

```csharp
using Regira.Normalizing;
using Regira.Entities.Models.Abstractions;

public class Product : IEntityWithSerial, IHasNormalizedContent
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // Combines Title + Description, normalized, into a single searchable field
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    // Single source
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }
}
```

### `[Normalized]` Attribute Options

| Property | Purpose |
|----------|---------|
| `SourceProperty` | Single source property name |
| `SourceProperties` | Array of source property names (concatenated with space) |
| `Recursive` | Process nested objects (class-level, default: `true`) |
| `Normalizer` | Custom `INormalizer` or `IObjectNormalizer` type to use |

---

## Custom Entity Normalizer

Use when attribute-based normalization is insufficient — e.g. for phone numbers, codes, or multi-step transformations.

```csharp
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;

public class ProductNormalizer : EntityNormalizerBase<Product>
{
    private readonly INormalizer _normalizer;

    public ProductNormalizer(INormalizer normalizer) => _normalizer = normalizer;

    // When true, no other normalizer runs for this entity type
    // public override bool IsExclusive => true;

    public override async Task HandleNormalize(Product item)
    {
        var content = $"{item.Title} {item.Description}".Trim();
        item.NormalizedContent = await _normalizer.Normalize(content);

        // Custom: strip non-digits for phone-based search
        item.NormalizedPhone = new string((item.Phone ?? "").Where(char.IsDigit).ToArray());
    }
}

// Register per entity
e.AddNormalizer<ProductNormalizer>();

// Register globally (applies to all entities implementing interface)
options.AddNormalizer<IHasPhone, PhoneNormalizer>();
```

### `EntityNormalizerBase<T>` Base Class

```csharp
public abstract class EntityNormalizerBase<T>(INormalizer? normalizer = null) : IEntityNormalizer<T>
{
    public virtual bool IsExclusive => false;

    public abstract Task HandleNormalize(T item);
    public virtual async Task HandleNormalizeMany(IEnumerable<T> items) { /* iterates items */ }
}
```

> When `IsExclusive = true`, **only this normalizer** runs for the entity type. Otherwise, all compatible normalizers are executed.

---

## Enabling Normalizer Interceptors

Normalizers run automatically via EF Core interceptors. Must be registered in `AddDbContext`:

```csharp
builder.Services.AddDbContext<YourDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddNormalizerInterceptors(sp));  // sp = IServiceProvider (required)
```

> **Important**: Always use the `(sp, options) =>` factory overload when calling `AddNormalizerInterceptors`.

---

## Registering Default Normalizer Services

```csharp
services.UseEntities<DbContext>(options =>
{
    // Registers all default normalizer services
    options.AddDefaultEntityNormalizer();
    // Or included in:
    options.UseDefaults();
});
```

### Default Services

| Interface | Implementation | Role |
|-----------|---------------|------|
| `INormalizer` | `DefaultNormalizer` | Normalizes a string value |
| `IObjectNormalizer` | `ObjectNormalizer` | Processes `[Normalized]` attributes |
| `IEntityNormalizer` | `DefaultEntityNormalizer<IEntity>` | Orchestrates attribute-based normalization |
| `IQKeywordHelper` | `QKeywordHelper` | Parses Q search strings with wildcard support |

---

## Filtering with Normalized Content

### Built-in Global Filter

Registers a global filter for all entities implementing `IHasNormalizedContent`:

```csharp
// Registered by UseDefaults() — no need to add manually:
options.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
```

This filter applies `Q` to the `NormalizedContent` field using LIKE queries.

### Custom Filter with `IQKeywordHelper`

Use `IQKeywordHelper` to parse the `Q` string into normalized keywords with wildcard support (`*` → `%`):

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    private readonly IQKeywordHelper _qHelper;

    public ProductQueryBuilder(IQKeywordHelper qHelper) => _qHelper = qHelper;

    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            // Parse Q into keywords; keyword.QW is the LIKE-compatible wildcard version
            var keywords = _qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }
        return query;
    }
}
```

### `FilterHasNormalizedContentQueryBuilder` Example (Built-in)

```csharp
public IQueryable<IHasNormalizedContent> Build(IQueryable<IHasNormalizedContent> query, ISearchObject<TKey>? so)
{
    if (!string.IsNullOrWhiteSpace(so?.Q))
    {
        var keywords = qHelper.Parse(so.Q);
        foreach (var q in keywords)
            query = query.Where(x => EF.Functions.Like(x.NormalizedContent, q.QW));
    }
    return query;
}
```

---

## Extending `DefaultNormalizer`

```csharp
using Regira.Entities.EFcore.Normalizing;
using Regira.Normalizing.Abstractions;

public class CustomProductNormalizer : DefaultEntityNormalizer<Product>
{
    public CustomProductNormalizer(IObjectNormalizer objectNormalizer)
        : base(objectNormalizer) { }

    public override async Task HandleNormalize(Product item)
    {
        // Run base normalization (processes [Normalized] attributes)
        await base.HandleNormalize(item);

        // Custom additional logic
        item.NormalizedCode = item.Code?.ToUpperInvariant().Trim();
    }
}
```

---

## DI Registration Summary

```csharp
// DbContext setup (required to enable normalizer interceptors)
builder.Services.AddDbContext<YourDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddNormalizerInterceptors(sp));

// Global normalization + all default services
services.UseEntities<YourDbContext>(options =>
{
    options.UseDefaults();  // includes AddDefaultEntityNormalizer()
    
    // Global normalizer for interface
    options.AddNormalizer<IHasPhone, PhoneNormalizer>();
});

// Per-entity
services.UseEntities<YourDbContext>(/* ... */)
    .For<Product>(entity =>
    {
        entity.AddNormalizer<ProductNormalizer>();
    });
```

---

## Key Namespaces

```csharp
using Regira.Normalizing;                                  // [Normalized], ObjectNormalizer
using Regira.Normalizing.Abstractions;                     // INormalizer, IObjectNormalizer
using Regira.Entities.EFcore.Normalizing;                  // DefaultEntityNormalizer<T>
using Regira.Entities.EFcore.Normalizing.Abstractions;     // IEntityNormalizer<T>, EntityNormalizerBase<T>
using Regira.Entities.Keywords.Abstractions;               // IQKeywordHelper
using Regira.Entities.Keywords;                            // QKeywordHelper
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders; // FilterHasNormalizedContentQueryBuilder
```
