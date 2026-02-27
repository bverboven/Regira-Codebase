# Regira Entities — Controllers Agent

You are a specialized agent responsible for implementing **Web API controllers** in the Regira Entities framework. This includes choosing the right base controller, understanding the generated endpoints, and adding custom actions.

---

## Overview

- Controllers provide standard CRUD endpoints via `EntityControllerBase`
- They use an `IEntityService` (resolved automatically via `HttpContext.RequestServices` — **do not inject manually**)
- Generic types on the controller must match the service (DTOs are extra)
- Responsible for mapping to/from DTOs using `IEntityMapper` (also resolved automatically)

---

## Controller Selection

Choose the most appropriate base controller variant:

```csharp
// Minimal — no search or custom sorting
EntityControllerBase<TEntity, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TDto, TInputDto>

// With SearchObject
EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>

// Full-featured (recommended for most entities)
EntityControllerBase<TEntity, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>
```

**Rule**: always prefer the richer variant — use the full `TKey, TSearchObject, TSortBy, TIncludes` form unless explicitly keeping it simple.

---

## Standard Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

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
    // All standard endpoints are already provided by the base class
    // Only add actions here when the base endpoints are insufficient
}
```

---

## Standard Endpoints (Provided by Base)

### Fetch

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products/{id}` | Single entity by ID (Details) |
| `GET` | `/api/products` | List (with optional SearchObject from QueryString) |
| `GET` | `/api/products/search` | List + Count combined (for pagination) |
| `POST` | `/api/products/list` | Complex list (multiple SearchObjects in body) |
| `POST` | `/api/products/search` | Complex search (multiple SearchObjects in body) |

**QueryString parameters for List/Search:**
- `q` — general text search
- `page`, `pageSize` — pagination
- `includes` — flag values for navigation properties
- `sortBy` — enum values applied in sequence (can repeat)
- Any SearchObject property (e.g. `categoryId=1&minPrice=10`)

### Write

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/products` | Create (add) |
| `PUT` | `/api/products/{id}` | Modify (update) |
| `POST` | `/api/products/save` | Save (upsert — uses `Id` to determine add/modify) |
| `DELETE` | `/api/products/{id}` | Delete |

---

## Response Types

All endpoints return standardized wrappers:

```csharp
// GET /api/products/{id}
public class DetailsResult<TDto>
{
    public TDto Item { get; set; }
    public long? Duration { get; set; }  // Execution time in ms
}

// GET /api/products
public class ListResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long? Duration { get; set; }
}

// GET /api/products/search  (Items + Count for pagination)
public class SearchResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long Count { get; set; }
    public long? Duration { get; set; }
}

// POST / PUT / POST save
public class SaveResult<TDto>
{
    public TDto Item { get; set; }
    public bool IsNew { get; set; }
    public int Affected { get; set; }
    public long? Duration { get; set; }
}

// DELETE
public class DeleteResult<TDto>
{
    public TDto Item { get; set; }  // The deleted item
    public long? Duration { get; set; }
}
```

---

## Custom Controller Actions

Only add custom actions when the standard base endpoints are insufficient:

```csharp
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
    // Custom action — bulk price update
    [HttpPost("bulk-price-update")]
    public async Task<IActionResult> BulkPriceUpdate([FromBody] BulkPriceUpdateDto dto)
    {
        var service = HttpContext.RequestServices
            .GetRequiredService<IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>>();

        var products = await service.List(new ProductSearchObject { Ids = dto.ProductIds });
        foreach (var product in products)
            product.Price += dto.PriceAdjustment;

        foreach (var product in products)
            await service.Save(product);

        await service.SaveChanges();
        return Ok(new { Updated = products.Count });
    }
}
```

---

## Service Resolution

The base controller resolves `IEntityService` via `HttpContext.RequestServices` — **do NOT inject it via constructor**. If you need the service in a custom action, resolve it from the service provider:

```csharp
var service = HttpContext.RequestServices
    .GetRequiredService<IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>>();
```

---

## Complex Search (Multiple SearchObjects)

The POST `/list` and POST `/search` endpoints accept an array of `SearchObject` instances. Results from each SearchObject are **OR-combined** (inclusive):

```http
POST /api/products/search
Content-Type: application/json

[
  { "categoryId": [1, 2], "isArchived": false },
  { "ids": [42, 43] }
]
```

---

## Error Handling

`EntityInputException<TEntity>` thrown in services, preppers, or custom actions is automatically caught and returns HTTP 400 with the validation errors:

```json
{
  "message": "Validation failed",
  "errors": {
    "Price": "Must be >= 0"
  }
}
```

---

## Controller with Non-Int Key

```csharp
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : EntityControllerBase<
    Category,
    Guid,                    // TKey
    CategorySearchObject,
    CategoryDto,
    CategoryInputDto>
{
}
```

---

## Guidelines

| Rule | Reason |
|------|--------|
| Never expose raw entity classes in responses | Use DTOs to control the API contract |
| Do not override standard endpoints unless necessary | Base class already handles them correctly |
| Do not inject `IEntityService` in controller constructor | Base class handles resolution |
| Match DTOs to registered mapping configuration | `e.UseMapping<TDto, TInputDto>()` must match controller types |
| Extend `SearchObject` for new filters, not new endpoints | Keeps the API surface clean |

---

## Key Namespaces

```csharp
using Regira.Entities.Web.Controllers.Abstractions; // EntityControllerBase<...>
using Regira.Entities.Web.Models;                   // DetailsResult<T>, ListResult<T>, SearchResult<T>, SaveResult<T>, DeleteResult<T>
using Regira.Entities.Services.Abstractions;        // IEntityService<...> (for custom action service resolution)
using Microsoft.AspNetCore.Mvc;                     // [ApiController], [Route], [HttpGet], etc.
```
