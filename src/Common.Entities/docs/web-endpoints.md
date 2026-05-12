# Web Endpoints

There are two approaches to exposing entity CRUD operations as HTTP endpoints:

| Approach | Package | Best for |
|----------|---------|---------|
| **FastEndpoints** (preferred) | `Regira.Entities.Web.FastEndpoints` | Zero-boilerplate auto-registration; no per-entity controller classes needed |
| **Controllers** | `Regira.Entities.Web` | MVC attribute model; use when the project already relies on controllers |

---

## FastEndpoints (Preferred)

The FastEndpoints approach auto-discovers all registered `IEntityService<,>` services and wires up a complete set of CRUD routes with a single call. No per-entity boilerplate required.

### Quick Setup

**1. Add the NuGet package:**
```
Regira.Entities.Web.FastEndpoints
```
Also requires `FastEndpoints` (v8+) to be set up in the pipeline:
```csharp
builder.Services.AddFastEndpoints();
// ...
app.UseFastEndpoints();
```

**2. Auto-register all entity endpoints in `Program.cs`:**
```csharp
app.MapEntityEndpoints();
```

That's it. Every entity that has an `IEntityService<,>` registered via `UseEntities()` gets its own set of CRUD routes automatically.

> **Prerequisite:** `UseEntities<TContext>()` must be called during service registration. It registers `IServiceCollection` in the DI container, which `MapEntityEndpoints()` uses for service discovery.

### Registered Routes

The following routes are generated per entity using `api/{entityname}s` as the base (simple pluralization of the entity type name):

```
GET    /api/{entities}/{id}     — Details (404 if not found)
GET    /api/{entities}          — List (?page, ?pageSize)
POST   /api/{entities}          — Create
POST   /api/{entities}/save     — Save (upsert — 404 if updating nonexistent)
PUT    /api/{entities}/{id}      — Modify (404 if not found)
DELETE /api/{entities}/{id}     — Delete (404 if not found)
```

All write endpoints catch `EntityInputException` and return `ValidationProblem` (HTTP 400).

> **Note:** Auto-registration uses raw entity classes directly (no DTOs). For DTO-aware endpoints, use the [manual endpoint base classes](#manual-fastendpoints-base-classes) below.

### Customising Routes

Use the optional `configure` callback to change the route prefix or override individual entity routes:

```csharp
// Change prefix from "api" to "v1"
app.MapEntityEndpoints(routePrefix: "v1");

// Override specific entity routes (useful for irregular plurals)
app.MapEntityEndpoints(configure: options =>
{
    options.For<Category>("api/categories");
    options.For<Person>("api/people");
});

// Both at once
app.MapEntityEndpoints(routePrefix: "v2", configure: options =>
{
    options.For<Category>("v2/categories");
});
```

Default pluralization is a simple English convention (`name + "s"`). Use `For<TEntity>(route)` for irregular plurals (Category → categories, Person → people, Status → statuses).

### Manual FastEndpoints Base Classes

When you need DTO mapping, custom authorization, or per-endpoint logic, inherit from the individual base classes instead of using auto-registration:

**Simple (int key shortcut available for each):**
```csharp
EntityDetailsEndpointBase<TEntity, TKey, TDto>
EntityListEndpointBase<TEntity, TKey, TSearchObject, TDto>
EntityCreateEndpointBase<TEntity, TKey, TDto, TInputDto>
EntitySaveEndpointBase<TEntity, TKey, TDto, TInputDto>
EntityModifyEndpointBase<TEntity, TKey, TDto, TInputDto>
EntityDeleteEndpointBase<TEntity, TKey, TDto>
```

**Complex (with SortBy + Includes support):**
```csharp
ComplexEntityListGetEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntityListPostEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntitySearchGetEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
ComplexEntitySearchPostEndpointBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto>
```

Example:
```csharp
// Products endpoint with DTO mapping
public class ProductDetailsEndpoint
    : EntityDetailsEndpointBase<Product, int, ProductDto>
{
    public override void Configure()
    {
        base.Configure();
        // add auth, tags, etc.
        Roles("Admin");
    }
}
```

The base class sets the route (`GET /api/products/{id}`) and handles the service call and mapping. Override `Configure()` to add authentication, OpenAPI tags, or other FastEndpoints configuration.

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

*The SearchObject items return queries that are inclusive.*

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
