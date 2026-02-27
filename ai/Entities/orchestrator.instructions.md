# Regira Entities — Orchestrator Agent

You are the **Orchestrator** for the Regira Entities AI agent system. Your role is to understand the overall architecture of the Regira Entities framework, decompose user requests into sub-tasks, and invoke the appropriate specialized instruction files.

**Always load the relevant instruction file(s) for the task at hand.** Do not rely solely on the knowledge in this file — the specialized files contain the authoritative patterns, interfaces, code templates, and namespace references for each domain.

---

## Framework Overview

**Regira Entities** is a generic, extensible .NET framework for managing data entities with standardized CRUD operations, filtering, sorting, and navigation property includes. It uses EF Core for data access and supports Web API controllers out of the box.

### Core Components

| Component | Description | Instruction File |
|-----------|-------------|------------------|
| Entity Models | POCO classes, SearchObject, SortBy, Includes, DTOs | `models.instructions.md` |
| Services | `IEntityService`, `EntityRepository`, QueryBuilders, Processors, Preppers, Primers | `services.instructions.md` |
| Mapping | `IEntityMapper`, AfterMappers, Mapster/AutoMapper | `mapping.instructions.md` |
| Controllers | `EntityControllerBase`, HTTP endpoints, response types | `controllers.instructions.md` |
| Normalizing | `IEntityNormalizer`, `[Normalized]` attribute, keyword search | `normalizing.instructions.md` |
| Attachments | `EntityAttachment`, `AttachmentFileService`, `IFileService` | `attachments.instructions.md` |
| Troubleshooting | Common issues, root causes, fixes, quick-reference checklist | `troubleshooting.instructions.md` |

---

## Generic Type System

Understanding the generic type parameters is fundamental to every task in this framework:

| Type | Required | Purpose | Example |
|------|----------|---------|---------|
| `TEntity` | ✓ | The entity class | `Product` |
| `TKey` | ✓* | Primary key type (*default `int`) | `Guid`, `int` |
| `TSearchObject` | ○ | Advanced filtering | `ProductSearchObject` |
| `TSortBy` | ○ | Sorting enum (not flags) | `ProductSortBy` |
| `TIncludes` | ○ | Navigation property flags enum | `ProductIncludes` |
| `TDto` | ○ | Read/display model | `ProductDto` |
| `TInputDto` | ○ | Create/update model | `ProductInputDto` |

---

## Processing Pipelines

### Read Pipeline
```
EntitySet
  → QueryBuilders (Filters → Sorting → Paging → Includes)
  → Processors
  → Mapping + AfterMapping*
```
*AfterMapping only in API controllers*

### Write Pipeline
```
Input
  → Mapping* + AfterInput*
  → Preppers
  → SaveChanges (DbContext)
    → Primers (Interceptors)
    → Submit
```
*Mapping only in API controllers*

---

## Instruction File Map

Load the instruction file that matches the user's request. For tasks that span multiple domains, load all relevant files.

| User intent | Load |
|-------------|------|
| Create a new project (scaffolding, NuGet, Program.cs, DbContext, DI entry-point) | `setup.instructions.md` |
| Entity class, interfaces, SearchObject, SortBy, Includes, DTOs | `models.instructions.md` |
| QueryBuilders, Processors, Preppers, Primers, `IEntityService`, DI registration | `services.instructions.md` |
| Mapping setup, AfterMappers, Mapster/AutoMapper configuration | `mapping.instructions.md` |
| API controllers, endpoints, response types | `controllers.instructions.md` |
| Normalized fields, `[Normalized]`, `IEntityNormalizer`, keyword filtering | `normalizing.instructions.md` |
| File attachments, `EntityAttachment`, `IFileService`, attachment controllers | `attachments.instructions.md` |
| Diagnosing errors, runtime issues, DI problems, migration failures | `troubleshooting.instructions.md` |

---

## Orchestration — Task Routing

### Create a New Regira API Project

Load: `setup.instructions.md` (scaffolding, templates, packages, migrations)

1. Set up project files (`*.csproj`, `NuGet.Config`, `appsettings.json`, `Program.cs`) — see `setup.instructions.md`
2. Create `AppDbContext` — see `setup.instructions.md`
3. Create the DI entry-point (`AddEntityServices`) — see `setup.instructions.md` + `services.instructions.md`
4. Add entities using the workflow below

### Add a New Entity

Load all relevant files and execute in this order:

1. **Entity class + interfaces** → load `models.instructions.md`
2. **`SearchObject`, `SortBy`, `Includes`** → load `models.instructions.md`
3. **DTOs (`TDto`, `TInputDto`)** → load `models.instructions.md`
4. **Query builders, processors, preppers, primers** → load `services.instructions.md`
5. **Mapping config + AfterMappers** → load `mapping.instructions.md`
6. **API controller** → load `controllers.instructions.md`
7. **DbContext `DbSet` + EF relationships** → see `setup.instructions.md`
8. **Migration**: `dotnet ef migrations add Add_{EntityName}` + `dotnet ef database update`
9. **DI registration** (`.For<TEntity,...>(e => { ... })`) → load `services.instructions.md`

If the entity uses full-text search → also load `normalizing.instructions.md`  
If the entity uses file attachments → also load `attachments.instructions.md`

### Modify an Existing Entity

Load the files relevant to what changed:

1. Entity class / `DbSet` / relationships changed → load `models.instructions.md`
2. DTOs or mapping changed → load `mapping.instructions.md`
3. `SearchObject`, enums, query builders changed → load `models.instructions.md` + `services.instructions.md`
4. Processors, preppers, normalizers changed → load `services.instructions.md` + `normalizing.instructions.md`
5. Controller behavior changed → load `controllers.instructions.md`
6. Schema changed → create and apply a new migration

---

## Defaults (apply unless explicitly instructed otherwise)

- **Database**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Mapping**: Mapster (`Regira.Entities.Mapping.Mapster`)
- **Primary key**: `int` (via `IEntityWithSerial`)
- **Project structure**: Per-entity folder structure
- **Service layer**: Default `EntityRepository`
- **DI setup**: `options.UseDefaults()` + `options.UseMapsterMapping()`

> For all project scaffolding templates (`NuGet.Config`, `.csproj`, `appsettings.json`, `Program.cs`, `AppDbContext`, DI entry-point, folder structure, migration commands) see `setup.instructions.md`.

---

## Decision Guidelines

### Inline vs. Separate Classes

**Use inline** when:
- Logic is simple (< 10 lines)
- Not reusable across entities
- Rapid prototyping

**Use separate classes** when:
- Complex logic
- Needs DI (DbContext, services)
- Reusable across entities
- Production code requiring isolated testability

### When to Create a Custom `EntityWrappingService`

Use `EntityWrappingServiceBase` to wrap the default `EntityRepository` when you need:
- Caching
- Authorization at the service level
- Auditing / logging around operations
- Complex cross-entity validation

Otherwise, use `EntityRepository` with QueryBuilders, Processors, Preppers, and Primers.

---

## Error Handling

`EntityInputException<T>` is automatically caught by controllers and returns HTTP 400:

```csharp
throw new EntityInputException<Product>("Validation failed")
{
    Item = item,
    InputErrors = new Dictionary<string, string>
    {
        [nameof(Product.Price)] = "Price must be greater than 0"
    }
};
```

---

## Troubleshooting

For diagnosing errors, runtime issues, DI problems, and migration failures, load `troubleshooting.instructions.md`. It contains root-cause analysis, code fixes, and a quick-reference checklist covering all common problem areas.
