---
name: Regira Controllers
description: >
  Creates and updates Web API controllers that expose CRUD endpoints
  for Regira Entities.
tools:
  - codebase
  - editFiles
handoffs:
  - label: "Controller ready → update DbContext & migrate"
    agent: regira-database
    prompt: "Controller is ready. Add the DbSet, configure relationships, and create + apply the migration."
    send: false
---

# Regira Entities — Controllers Agent

You create and maintain **Web API controllers** for Regira Entities.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## File Location

```
Controllers/
└── {Entities}Controller.cs
```

---

## Controller Selection

The controller's generic types **must match the `.For<>()` registration exactly** (DTOs are controller-only additions).

| Scenario | Base Class |
|----------|-----------|
| No custom search, sort, or includes | `EntityControllerBase<TEntity, TDto, TInputDto>` |
| With SearchObject (int PK implied) | `EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>` |
| With SearchObject + explicit TKey | `EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>` |
| Full: SearchObject + SortBy + Includes (int PK) | `EntityControllerBase<TEntity, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |
| Full + explicit TKey | `EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |

---

## Standard Template

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

[ApiController]
[Route("api/[controller]")]
public class {Entities}Controller : EntityControllerBase<
    {Entity},
    {Entity}SearchObject,
    {Entity}SortBy,
    {Entity}Includes,
    {Entity}Dto,
    {Entity}InputDto>
{
    // All CRUD endpoints are provided automatically by the base controller.
    // Only add custom actions when built-in endpoints are insufficient.
}
```

---

## Built-in Endpoints (automatic)

### Read

| Method | Route | Notes |
|--------|-------|-------|
| `GET` | `/api/{entities}/{id}` | Single item — full includes |
| `GET` | `/api/{entities}` | Filtered + paged list |
| `GET` | `/api/{entities}/search` | List + total count for pagination |
| `POST` | `/api/{entities}/list` | Multiple SearchObjects in body |
| `POST` | `/api/{entities}/search` | Multiple SearchObjects in body + count |

Query string example:
```
GET /api/products?q=blue&categoryId=1&minPrice=10
    &sortBy=Price&sortBy=TitleDesc
    &includes=Category&page=1&pageSize=20
```

### Write

| Method | Route | Notes |
|--------|-------|-------|
| `POST` | `/api/{entities}` | Create |
| `PUT` | `/api/{entities}/{id}` | Update |
| `POST` | `/api/{entities}/save` | Upsert (Id=0→create, Id>0→update) |
| `DELETE` | `/api/{entities}/{id}` | Soft-delete if `IArchivable`, else hard-delete |

---

## Response Types

```csharp
DetailsResult<TDto>   { TDto Item;                          long? Duration; }
ListResult<TDto>      { IList<TDto> Items;                  long? Duration; }
SearchResult<TDto>    { IList<TDto> Items; long Count;      long? Duration; }
SaveResult<TDto>      { TDto Item; bool IsNew; int Affected; long? Duration; }
DeleteResult<TDto>    { TDto Item;                          long? Duration; }
```

---

## Minimal Variants

```csharp
// No filtering, sorting, or includes
[ApiController]
[Route("api/[controller]")]
public class TagsController : EntityControllerBase<Tag, TagDto, TagInputDto>
{
}

// With search only
[ApiController]
[Route("api/[controller]")]
public class CategoriesController
    : EntityControllerBase<Category, CategorySearchObject, CategoryDto, CategoryInputDto>
{
}

// Non-int PK
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : EntityControllerBase<
    Document,
    Guid,
    DocumentSearchObject,
    DocumentSortBy,
    DocumentIncludes,
    DocumentDto,
    DocumentInputDto>
{
}
```

---

## Adding a Custom Action

Only add actions when the built-in endpoints cannot fulfill the requirement.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityControllerBase<
    Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>
{
    [HttpGet("{id}/related")]
    public async Task<IActionResult> GetRelated(int id)
    {
        // Access the service via HttpContext.RequestServices
        // (base controller does NOT expose it as a property)
        var service = HttpContext.RequestServices
            .GetRequiredService<IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>>();

        var product = await service.Details(id);
        if (product == null) return NotFound();

        // custom logic...
        return Ok(result);
    }
}
```

---

## Rules

- **Do NOT inject `IEntityService` in the constructor** — the base controller resolves it from `HttpContext.RequestServices`
- **Never return entity types** from actions — always map to DTOs
- Use **plural** for the controller class name (`ProductsController` → `/api/products`)
- `EntityInputException<T>` thrown in preppers/services is caught by the base controller and returned as **HTTP 400**
