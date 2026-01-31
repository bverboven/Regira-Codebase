# Entity Controllers - AI Agent Instructions

## Controller Overview

Controllers provide Web API endpoints for entity operations. The framework provides base controllers that handle standard CRUD operations automatically.

## Controller Selection

### Choose the Right Base Controller

```csharp
// 1. Basic CRUD with int PK, no DTOs
EntityControllerBase<Product>

// 2. With DTOs (recommended)
EntityControllerBase<Product, ProductDto, ProductInput>

// 3. Custom primary key
EntityControllerBase<Product, Guid, ProductDto, ProductInput>

// 4. With SearchObject
EntityControllerBase<Product, int, ProductSearchObject, ProductDto, ProductInput>

// 5. Full featured with sorting and includes
EntityControllerBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInput>

// 6. Alternative: Skip key type (defaults to int)
EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInput>
```

### Generic Type Alignment

**Critical**: Controller generic types must match entity characteristics (key type, search object, enums).

```csharp
// Entity with SearchObject, SortBy, and Includes
EntityControllerBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInput>

// Simple entity with just DTOs
EntityControllerBase<Product, ProductDto, ProductInput>
```

## Basic Controller Implementation

### Minimal Controller

```csharp
[ApiController]
[Route("products")]
public class ProductsController : EntityControllerBase<Product, ProductDto, ProductInput>;
```

**Note**: Controllers are abstract classes. Use constructor injection for dependencies you need throughout the controller, or use `[FromServices]` attribute for action-specific dependencies.

### Full-Featured Controller

```csharp
[ApiController]
[Route("products")]
public class ProductsController : EntityControllerBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInput>;
```

## Standard Endpoints

The base controller provides these endpoints automatically:

### GET Endpoints

```csharp
// GET api/products/5
// Returns a single entity with metadata
[HttpGet("{id}")]
public virtual Task<ActionResult<DetailsResult<TDto>>> Details(TKey id)

// GET api/products?title=laptop&page=1&pageSize=10
// Returns a list of entities with metadata (no count)
[HttpGet]
public virtual Task<ActionResult<ListResult<TDto>>> List(
    [FromQuery] TSearchObject so,
    [FromQuery] PagingInfo pagingInfo)
// Complex version with sorting and includes:
public virtual Task<ActionResult<ListResult<TDto>>> List(
    [FromQuery] TSo so,
    [FromQuery] PagingInfo pagingInfo,
    [FromQuery] TIncludes[] includes,
    [FromQuery] TSortBy[] sortBy)

// POST api/products/list (for complex search objects in body)
[HttpPost("list")]
public virtual Task<ActionResult<ListResult<TDto>>> List(
    [FromBody] TSo[] so,
    [FromQuery] PagingInfo pagingInfo,
    [FromQuery] TIncludes[] includes,
    [FromQuery] TSortBy[] sortBy)

// GET api/products/search?title=laptop&page=1&pageSize=10
// Returns entities with total count for pagination
[HttpGet("search")]
public virtual Task<ActionResult<SearchResult<TDto>>> Search(
    [FromQuery] TSo so,
    [FromQuery] PagingInfo pagingInfo,
    [FromQuery] TIncludes[] includes,
    [FromQuery] TSortBy[] sortBy)

// POST api/products/search (for complex search objects in body)
[HttpPost("search")]
public virtual Task<ActionResult<SearchResult<TDto>>> Search(
    [FromBody] TSo[] so,
    [FromQuery] PagingInfo pagingInfo,
    [FromQuery] TIncludes[] includes,
    [FromQuery] TSortBy[] sortBy)
```

### POST Endpoints

```csharp
// POST api/products
// Creates a new entity
[HttpPost]
public virtual Task<ActionResult<SaveResult<TDto>>> Create([FromBody] TInputDto model)

// POST api/products/save
// Creates or updates based on ID presence
[HttpPost("save")]
public virtual Task<ActionResult<SaveResult<TDto>>> Save([FromBody] TInputDto model)
```

### PUT Endpoints

```csharp
// PUT api/products/5
// Updates an existing entity
[HttpPut("{id}")]
public virtual Task<ActionResult<SaveResult<TDto>>> Modify(TKey id, [FromBody] TInputDto input)
```

### DELETE Endpoints

```csharp
// DELETE api/products/5
// Deletes an entity
[HttpDelete("{id}")]
public virtual Task<ActionResult<DeleteResult<TDto>>?> Delete(TKey id)
```

### Response Types

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
    public int Total { get; set; } // Total count for pagination
    public long? Duration { get; set; }
}

public class SaveResult<TDto>
{
    public TDto Item { get; set; }
    public long? Duration { get; set; }
}

public class DeleteResult<TDto>
{
    public TDto Item { get; set; } // The deleted item
    public long? Duration { get; set; }
}
```

## Customizing Controllers

### Adding Custom Endpoints

Custom endpoints should be for operations not covered by standard CRUD:

```csharp
[ApiController]
[Route("products")]
public class ProductsController(IEntityService<Product> service) : EntityControllerBase<Product, int, ProductSearchObject, ProductDto, ProductInput>
{
    // GET api/products/5/stock
    [HttpGet("{id}/stock")]
    public async Task<ActionResult<bool>> CheckStock(int id)
    {
        var items = await service.List(new { Id = id });
        var item = items.FirstOrDefault();
        
        if (item == null)
            return NotFound();
        
        // Custom business logic
        var inStock = item.Quantity > 0;
        return Ok(new { inStock });
    }
    
    // POST api/products/batch
    [HttpPost("batch")]
    public async Task<ActionResult<ListResult<ProductDto>>> CreateBatch(
        [FromBody] ProductInput[] models, 
        [FromServices] IEntityMapper mapper)
    {        
        var items = new List<Product>();
        foreach (var model in models)
        {
            var item = mapper.Map<Product>(model);
            await service.Add(item);
            items.Add(item);
        }
        await service.SaveChanges();
        
        var dtos = mapper.Map<List<ProductDto>>(items);
        return this.ListResult(dtos);
    }
}
```

### Overriding Standard Actions

```csharp
public class ProductsController(IEntityService<Product> service, ILogger<ProductsController> logger, IEntityMapper mapper) : EntityControllerBase<Product, ProductDto, ProductInput>
{
    // Override Details to add custom logic
    public override async Task<ActionResult<DetailsResult<ProductDto>>> Details(int id)
    {
        var result = await this.Details<Product, int, ProductDto>(id);
        if (result?.Value?.Item == null)
            return NotFound(new { message = "Product not found" });
        
        // Custom processing (e.g., logging)
        logger.LogInformation("Product {Id} accessed", id);
        
        return result;
    }
    
    // Override Delete for soft delete
    public override async Task<ActionResult<DeleteResult<ProductDto>>?> Delete(int id)
    {
        var items = await service.List(new { Id = id });
        var item = items.FirstOrDefault();
        
        if (item == null)
            return NotFound();
        
        // Soft delete instead of hard delete
        if (item is IArchivable archivable)
        {
            archivable.IsArchived = true;
            await service.Modify(item);
            await service.SaveChanges();
        }
        
        var dto = mapper.Map<ProductDto>(item);
        return this.DeleteResult(dto);
    }
}
```

### Custom Authorization

```csharp
[ApiController]
[Route("products")]
[Authorize] // Require authentication for all endpoints
public class ProductsController : EntityControllerBase<Product, ProductDto, ProductInput>
{
    // Override authorization per action
    [AllowAnonymous] // Allow anonymous access
    public override Task<ActionResult<ListResult<ProductDto>>> List(SearchObject so, PagingInfo pagingInfo)
    {
        return base.List(so, pagingInfo);
    }
    
    [Authorize(Roles = "Admin")] // Require admin role
    public override Task<ActionResult<DeleteResult<ProductDto>>?> Delete(int id)
    {
        return base.Delete(id);
    }
}
```

## Response Types and Status Codes

### Standard Responses

All base controller methods return result wrappers with 200 OK or NotFound:

```csharp
// Details, List, Search, Save, Delete all return:
// - 200 OK with result wrapper when successful
// - 404 NotFound when entity not found (Details, Save with ID, Delete)

// Example responses:
{
  "item": { /* DTO */ },
  "duration": 42
}

{
  "items": [ /* DTOs */ ],
  "duration": 123
}

{
  "items": [ /* DTOs */ ],
  "total": 150,
  "duration": 98
}
```

### Custom Response Status Codes

When adding custom endpoints, use standard HTTP status codes:

```csharp
// 200 OK - Success with data
return Ok(data);

// 201 Created - Resource created
return CreatedAtAction(nameof(Details), new { id = entity.Id }, result);

// 204 No Content - Success without data
return NoContent();

// 400 Bad Request - Validation error
return BadRequest(ModelState);
return BadRequest(new { message = "Invalid data" });

// 404 Not Found - Resource not found
return NotFound();
return NotFound(new { message = "Product not found" });

// 500 Internal Server Error - Unhandled exception
// Handled by middleware
```

## Validation

### Model Validation

```csharp
public class ProductsController(IEntityService<Product, int> service) : EntityControllerBase<Product, ProductDto, ProductInput>
{
    [HttpPost]
    public override async Task<ActionResult<SaveResult<ProductDto>>> Create([FromBody] ProductInput input)
    {
        // Automatic validation via [ApiController] attribute
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // Additional custom validation
        var existing = await service.List(new { Title = input.Title });
        if (existing.Any())
        {
            ModelState.AddModelError("Title", "Product with this title already exists");
            return BadRequest(ModelState);
        }
        
        return await base.Create(input);
    }
}
```

### FluentValidation Integration

```csharp
public class ProductInputValidator : AbstractValidator<ProductInput>
{
    public ProductInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .Must(BeUniqueTitle).WithMessage("Title already exists");
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);
    }
    
    private bool BeUniqueTitle(string title)
    {
        // Check uniqueness
        return true;
    }
}

// Register in DI
builder.Services.AddValidatorsFromAssemblyContaining<ProductInputValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

## Pagination

### Using SearchResult for Pagination

The Search endpoints return `SearchResult<TDto>` with total count for pagination:

```csharp
// GET api/products/search?page=2&pageSize=10&title=laptop
public class ProductsController : EntityControllerBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInput>
{
    // Search endpoint automatically returns:
    // {
    //   "items": [...],
    //   "total": 150,
    //   "duration": 42
    // }
}

// Client-side pagination calculation:
// totalPages = Math.Ceiling(total / pageSize)
// currentPage = page
// hasNextPage = (page * pageSize) < total
```

**PagingInfo Properties**:
- `Page`: Current page number (1-based)
- `PageSize`: Number of items per page

## Next Steps

- Configure dependency injection: [DI Instructions](AI-INSTRUCTIONS-DI.md)
- See complete examples: [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
