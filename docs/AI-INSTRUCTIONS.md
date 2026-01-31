# Regira Entity Framework - AI Agent Instructions

## Overview

This guide provides instructions for AI agents implementing applications using the Regira Entity libraries. The Regira Entity Framework is a comprehensive abstraction layer over Entity Framework Core that provides standardized patterns for entity management, CRUD operations, and data access.

## Instruction Files

This documentation is split into multiple focused instruction files:

1. **[Entity Models & Interfaces](AI-INSTRUCTIONS-MODELS.md)** - Creating and structuring entity models
2. **[Services](AI-INSTRUCTIONS-SERVICES.md)** - Implementing entity services and repositories
3. **[Controllers](AI-INSTRUCTIONS-CONTROLLERS.md)** - Creating Web API controllers
4. **[Dependency Injection](AI-INSTRUCTIONS-DI.md)** - Configuring services and dependencies
5. **[Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)** - Complete implementation examples

## Quick Start Checklist

When implementing a new entity in an application:

- [ ] Create entity model with appropriate interfaces
- [ ] Create DTOs (output DTO, input DTO)
- [ ] Create SearchObject with ICollection<TKey> for foreign keys
- [ ] Configure DbContext with entity
- [ ] Add entity service registration in DI
- [ ] Create controller (if needed for API)
- [ ] Implement query filters (optional)
- [ ] Add processors/preppers (optional)
- [ ] Configure mapping (AutoMapper or Mapster)
- [ ] Declare related/child properties with Related method (optional)

## Key Principles

### Filtering & Fetching Best Practices

1. **Use SearchObject properties for filtering** - Never create custom filter endpoints
   - ✓ `List(new { CategoryId = [1, 2] })` via querystring: `?categoryId=1&categoryId=2`
   - ✗ `GetByCategory(int categoryId)` custom endpoint

2. **Prefer List over Details for custom logic** - Consistent with filtering pattern
   - ✓ `List(new { Id = id }).FirstOrDefault()`
   - ○ `Details(id)` only for standard CRUD endpoints

3. **Use ICollection<TKey> for foreign keys in SearchObject**
   - ✓ `public ICollection<int>? CategoryId { get; set; }`
   - ✗ `public int? CategoryId { get; set; }`

## Core Concepts

### Generic Type Parameters

Understanding the generic type system is crucial:

| Type | Required | Purpose | Example |
|------|----------|---------|---------|
| TEntity | ✓ | The entity class | `Product` |
| TKey | ✓* | Primary key type (*default: int) | `Guid`, `int` |
| TSearchObject | ○ | Advanced filtering | `ProductSearchObject` |
| TSortBy | ○ | Sorting enum | `ProductSortBy` |
| TInclude | ○ | Navigation properties enum | `ProductIncludes` |
| TDto | ○ | Read/display model (details & lists) | `ProductDto` |
| TInputDto | ○ | Create/update model | `ProductInput` |

### Architecture Layers

```
Controller (Web API)
    ↓ uses
IEntityService (Service Interface)
    ↓ implemented by
EntityRepository or Custom Service
    ↓ uses (optional)
DbContext (typically EF Core)
```

**Important Notes:**
- The framework typically uses Entity Framework Core with DbContext, but this is not required
- `IEntityService` can be implemented directly by a repository (e.g., `EntityRepository`)
- Only one implementation of `IEntityService<TEntity, ...>` should be registered per entity in IoC
- Custom services can wrap or replace the default repository implementation

### Processing Pipeline

**Read Pipeline:**
```
Query → Filters → Sorting → Includes → Execute → Process → Map to DTO → IAfterMapper
```

**Write Pipeline:**
```
Input DTO → Map to Entity → Preppers (WriteRepository) → DbContext.SaveChanges → Primers (Interceptors) → Return
```

**Pipeline Details:**
- **IAfterMapper**: Decorates DTOs after mapping engine completes (optional)
- **Preppers**: Executed by WriteRepository before saving to prepare entities
- **Primers**: EF Core SaveChangesInterceptors executed by DbContext during SubmitChanges

## Key Principles

1. **Use Interfaces**: Entities should implement marker interfaces (IHasTitle, IArchivable, etc.) to enable global processing
2. **Prefer DTOs**: Always use DTOs for API responses to decouple internal models from external contracts. A single DTO type typically serves both detail and list views
3. **Generic Services**: Use the most specific generic service signature needed for your use case
4. **Convention over Configuration**: Follow naming conventions (SearchObject, SortBy enum, Includes enum)
5. **Separation of Concerns**: Keep filtering, sorting, and processing logic separate

## Common Patterns

### Entity Naming Convention
- Entity: `Product`
- SearchObject: `ProductSearchObject`
- SortBy enum: `ProductSortBy`
- Includes enum: `ProductIncludes`
- DTO: `ProductDto` (used for both details and lists)
- InputDTO: `ProductInput` / `CreateProductInput` / `UpdateProductInput`

**Note on DTOs**: Typically, a single DTO type is used for both detail and list views. When returning list items, some properties (especially navigation properties) may be null. Use the `TInclude` parameter to control which related entities are loaded.

### Service Selection Guide

Choose the appropriate service interface based on requirements:

- **IEntityService&lt;TEntity&gt;**: Basic CRUD, int primary key
- **IEntityService&lt;TEntity, TKey&gt;**: Custom primary key type
- **IEntityService&lt;TEntity, TKey, TSearchObject&gt;**: Add filtering
- **IEntityService&lt;TEntity, TKey, TSearchObject, TSortBy, TInclude&gt;**: Full featured

## Getting Started

New to the framework? Start with:
1. Read [Entity Models & Interfaces](AI-INSTRUCTIONS-MODELS.md)
2. Review [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
3. Implement a simple entity following the patterns
4. Gradually add advanced features (filtering, sorting, processing)

## Best Practices for AI Agents

### When Creating Entities
- Always check which interfaces the entity should implement based on its properties
- Use nullable reference types appropriately
- Add data annotations for validation
- Keep entities focused on data, not behavior

### When Creating Services
- Use the minimal generic signature that meets requirements
- Don't create custom services unless necessary
- Leverage global filters and processors
- Use dependency injection for all dependencies

### When Creating Controllers
- Match generic types with the service
- Use appropriate HTTP verbs and routes
- Prefer returning DTOs, try avoiding to return entities
- Include proper status codes and error handling

### Code Organization
- Group related entities in the same namespace
- Keep DTOs near their entities
- Use clear, descriptive names

## Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| Can't find entity by Guid | Use `IEntityService<TEntity, Guid>` instead of default |
| Filtering not working | Ensure SearchObject is registered with service and controller |
| Related entities not loaded | Add Includes enum and use IIncludableQueryBuilder |
| Changes not saving | Check that primers are registered and DbContext is configured |
| Mapping errors | Verify mapping configuration and DTO property types match |

## Next Steps

Proceed to the specific instruction files based on your current task:
- Defining entities? → [Entity Models & Interfaces](AI-INSTRUCTIONS-MODELS.md)
- Creating business logic? → [Services](AI-INSTRUCTIONS-SERVICES.md)
- Building APIs? → [Controllers](AI-INSTRUCTIONS-CONTROLLERS.md)
- Setting up the app? → [Dependency Injection](AI-INSTRUCTIONS-DI.md)
- Need examples? → [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
