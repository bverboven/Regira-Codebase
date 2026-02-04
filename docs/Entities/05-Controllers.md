# Entity Controllers

Controllers provide Web API endpoints for entity operations. 

- The framework provides base controllers that handle standard **CRUD** operations automatically
- A controller will read or write entities using a **`IEntityService<TEntity>`**
- It's **not necessary to inject** this service in the constructor,
the base Controller will get it using `HttpContext.RequestServices`
- The controller's **generic types** must match the service's generic types (DTOs excluded)
- Responsible for mapping to and from DTO models using a `IEntityMapper`

## Controller Selection

Choose the Right Base Controller

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

## Standard Endpoints

All base controllers provide these endpoints:

### Fetch Endpoints

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

*The SearchObject items each return a query that are inclusive.*

### Save (Add/Modify)

```csharp
// POST /{entities} - Create
Create(inputDto) -> SaveResult

// PUT /{entities}/{id} - Update
Modify(id, inputDto) -> SaveResult

// POST /{entities}/save - Upsert
Save(inputDto) -> SaveResult
```

### DELETE Endpoint

```csharp
// DELETE /{entities}/{id} - Delete
Delete(id) -> DeleteResult
```


## Response Types

All endpoints return standardized result wrappers:

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

## Overview

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. **[Controllers](05-Controllers.md)** - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
