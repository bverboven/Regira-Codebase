# Web Endpoints

Expose entity CRUD operations as HTTP endpoints using controllers:

| Package | Description |
|---------|-------------|
| `Regira.Entities.Web` | MVC attribute model via `EntityControllerBase` |

---

## Controllers

Controllers provide a more traditional, attribute-based approach using `EntityControllerBase`. Use this when you need full customisation, a per-entity pipeline with DTO mapping, or advanced sorting and includes.

### Controller Selection

```csharp
// basic (not recommended)
EntityControllerBase<TEntity>
EntityControllerBase<TEntity, TKey>
// basic (using DTOs, recommended)
EntityControllerBase<TEntity, TDto, TInputDto>
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>
// complex (advanced operations)
EntityControllerBase<TEntity, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

### Standard Endpoints

All base controllers provide these endpoints:

#### Fetch Endpoints

**Details:**
```csharp
// GET /{entities}/{id} - Single entity
Details(id) -> DetailsResult
```

**List:**
```csharp
// GET /{entities} - Basic List
List() -> ListResult

// GET /{entities}?q={search}&page=1&pageSize=10 - List
List(searchObject, pagingInfo) -> ListResult

// GET /{entities}?categoryId=1&includes=Category&sortBy=CreatedDesc&sortBy=Title
List(searchObject, pagingInfo, includes[], sortBy[]) -> ListResult
```

**Search:**
```csharp
// GET /{entities}/search?q={keyword}&page=1 - List + Count combined
Search(searchObject, pagingInfo) -> SearchResult
```

**Complex (POST):**
```csharp
// POST /{entities}/list (collection of SearchObjects in body)
List([FromBody] searchObject[], pagingInfo, includes[], sortBy[]) -> ListResult

// POST /{entities}/search (collection of SearchObjects in body)
Search([FromBody] searchObject[], pagingInfo, includes[], sortBy[]) -> SearchResult
```

*The SearchObject items return queries that are inclusive (using Union).*

#### Save (Add/Modify)

```csharp
// POST /{entities} - Create
Create(inputDto) -> SaveResult

// PUT /{entities}/{id} - Update
Modify(id, inputDto) -> SaveResult

// POST /{entities}/save - Upsert
Save(inputDto) -> SaveResult
```

#### DELETE Endpoint

```csharp
// DELETE /{entities}/{id} - Delete
Delete(id) -> DeleteResult
```

### Notes

- A controller reads/writes entities using an `IEntityService`
- The controller's generic types must match the service's generic types (DTOs excluded)
- It's **not necessary to inject** the service in the constructor — the base controller resolves it via `HttpContext.RequestServices`
- Responsible for mapping to/from DTO models using `IEntityMapper`

---

## Response Types

Both approaches return the same standardised result wrappers:

```csharp
public class DetailsResult<TDto>
{
    public TDto Item { get; set; }
    public long? Duration { get; set; } // Execution time in ms
}

public class ListResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long? Duration { get; set; }
}

public class SearchResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long Count { get; set; } // Total count for pagination
    public long? Duration { get; set; }
}

public class SaveResult<TDto>
{
    public long? Duration { get; set; }
    public bool IsNew { get; set; }
    public int Affected { get; set; }
    public TDto Item { get; set; }
}

public class DeleteResult<TDto>
{
    public TDto Item { get; set; } // The deleted item
    public long? Duration { get; set; }
}
```

---

## Overview

1. [Index](../README.md) — Overview of Regira Entities
1. [Entity Models](models.md) — Creating and structuring entity models
1. [Services](services.md) — Implementing entity services and repositories
1. [Mapping](mapping.md) — Mapping Entities to and from DTOs
1. **[Web Endpoints](web-endpoints.md)** — Exposing entity operations as HTTP endpoints
1. [Normalizing](normalizing.md) — Data normalization techniques
1. [Attachments](attachments.md) — Managing file attachments
1. [Built-in Features](built-in-features.md) — Ready to use components
1. [Checklist](checklist.md) — Step-by-step guide for common tasks
1. [Practical Examples](examples.md) — Complete implementation examples
