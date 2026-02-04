# Regira Entities

## Index

Regira Entities is a generic, extensible framework for managing **data entities** in .NET applications. 
It provides a **standardized** way to handle CRUD operations, filtering, sorting, and includes, 
while allowing **customization** through generic type parameters, interfaces and specialized helper services.

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
| TInputDto | ○ | Create/update model | `ProductInputDto` |

### Architecture

- An Entity Controller requires an `IEntityService` to perform operations.
- The Entity Controller should implement all generic types of the service, but can add an extra Dto and InputDto type
- An `IEntityService` is implemented by `EntityRepository` by default, but can be replaced by any custom implementation.

Main **functionality** of the service:

| Action  | Purpose |
|---------|---------|
| Details | Get a single item by ID, usually with all Navigation properties included |
| List    | Get a (filtered, sorted & paged) collection of items, usually with limited or no Navigation properties |
| Save    | Create or Update an item, usually Navigation properties are excluded (when updating). However, child collections can be included |
| Remove  | Delete an item |

### Processing Pipeline

Assuming a `DbContext Repository` is being used.

**Read Pipeline:**

1. EntitySet
1. QueryBuilders 
   1. Filters
   1. Sorting
   1. Paging
   1. Includes
1. Processors
1. Mapping (+AfterMapping)*

**Write Pipeline:**

1. Input
1. Mapping (+AfterMapping)*
1. Preppers (Repository)
1. SaveChanges (DbContext)
   1. Primers (Interceptors)
   1. Submit changes

*\*: only executed when using API controller*

**Pipeline Details:**
- **QueryBuilders**: Build IQueryable based on SearchObject, SortBy & Includes
- **Processors**: Modify entities after fetching (e.g. setting non-mapped properties)
- **Preppers**: Executed by WriteRepository before saving to prepare entities
- **Primers**: EF Core SaveChangesInterceptors executed by DbContext during SubmitChanges
- **AfterMapper**: Decorates DTOs or Entities after Mapper completes (e.g. calculating URIs)

## Dependency Injection

```csharp
// basic sample setup
builder.Services
    .UseEntities(/* ... */)
    .For<Category>()
    .For<Product, int, ProductSearchObject>(item=> {
        item.SortBy(query, _) => query.OrderBy(x => x.Title));
        item.Includes((query, _) => query.Include(x => x.Category));
    })
    .For<Order, int, OrderSearchObject, OrderSortBy, OrderIncludes>(item=> {
        item.SortBy(query, _) => query.OrderByDesc(x => x.Created));
        item.Includes((query, _) => query.Include(x => x.OrderItems).OrderBy(x => x.Product.Title));
        item.Related(c => c.OrderItems);
    });
```

## Overview

1. **[Index](01-Index.md)** - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
