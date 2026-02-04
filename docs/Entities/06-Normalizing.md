# Normalizing Entity Properties

- Optional
- Facilitates searching
    - Removes diacritics (accents)
    - Removes special characters
    - Standardizes whitespace
    - Converts to lowercase
- Customizable
    - Format phone numbers

## Saving normalized properties

A normalized property is usually just a **joined string** (using a space), built by normalizing one or more **source properties**.

### Automated

- Use `[Normalized]` attribute on property to be normalized
- Define `SourceProperties` for the source properties
- Use `DefaultEntityNormalizer` to process `NormalizedAttribute` automatically

### Customized

- Implement interface `IEntityNormalizer`
- Derive from base class `EntityNormalizerBase`
- Derive from `DefaultEntityNormalizer` to extend default behavior

```csharp
// base class
public abstract class EntityNormalizerBase<T>(INormalizer? normalizer = null) : IEntityNormalizer<T>
    where T : class
{
    public virtual bool IsExclusive => false;

    public abstract Task HandleNormalize(T item);
    public virtual async Task HandleNormalizeMany(IEnumerable<T> items) {...;
}
```

*When `IsExclusive` is true, only **this normalizer** is executed for the entity type.
Otherwise, other compatible normalizers are also executed.*

## Filtering using normalized properties

- Use `IQKeywordHelper` to normalize search keywords
- Use same `INormalizer` for saving and filtering (by default)

Sample from `FilterHasNormalizedContentQueryBuilders`
```csharp
    public IQueryable<IHasNormalizedContent> Build(IQueryable<IHasNormalizedContent> query, ISearchObject<TKey>? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var q in keywords)
            {
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, q.QW));
            }
        }

        return query;
    }
```

## Architecture

### Services

1. **INormalizer** - Property-level normalization (string transformation)
2. **IObjectNormalizer** - Object-level normalization (processes properties with `[Normalized]` attribute)
3. **IEntityNormalizer** - Entity-level normalization (custom business logic)

### Normalized attribute

- `SourceProperty` - Single source property name
- `SourceProperties` - Array of source property names (content concatenated with space)
- `Recursive` - Process nested objects (class-level only, default: true)
- `Normalizer` - Custom normalizer type (must implement `INormalizer` or `IObjectNormalizer`)

```csharp
// Normalize from multiple properties (concatenated with space)
[Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
public string? NormalizedContent { get; set; }
```

## Dependency Injection

### Auto retrieve normalizers

Enable an normalizers to be executed as SaveChanges interceptors:
```csharp
services.AddDbContext<MyDbContext>((serviceProvider, db) =>
{
    db.UseSqlServer(connectionString)
        .AddNormalizerInterceptors(serviceProvider);
});
```
*This will register a `EntityNormalizerContainerInterceptor` which will get all matching normalizers when saving entities.*

### Default services

| Interface | Implementation |
|-----------|----------------|
| `INormalizer` | `DefaultNormalizer` |
| `IQKeywordHelper` | `QKeywordHelper` |
| `IObjectNormalizer` | `DefaultObjectNormalizer` |
| `IEntityNormalizer` | `DefaultEntityNormalizer<IEntity>` |

```csharp
services.UseEntities<DbContext>(e =>
{
    // Registers all default services
    e.AddDefaultEntityNormalizer();
});
```

### Globally

```csharp
services.UseEntities<DbContext>(e =>
{
    e.AddNormalizer<IEntityInterface, MyGlobalEntityNormalizer>();
});
```

### Per Entity

```csharp
services
    .UseEntities<DbContext>(/*...*/)
    .For<Entity>(e =>
    {
        e.AddNormalizer<MyEntityNormalizer>();
    });
```

## Overview

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. **[Normalizing](06-Normalizing.md)** - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
