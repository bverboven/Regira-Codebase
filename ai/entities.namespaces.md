# Regira Entities — Namespace Reference

> **AI Agent Rule**: You MUST use the exact namespaces listed in this file.
> You are NOT allowed to guess, invent, or assume any namespace.
> If a type is not listed here, look it up in the codebase before using it.

---

## Entity Interfaces & Base Models

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Models` | `SearchObject`, `SearchObject<TKey>`, `EntitySortBy`, `EntityInputException<T>` |
| `Regira.Entities.Models.Abstractions` | `IEntity<TKey>`, `IEntityWithSerial`, `IHasTimestamps`, `IHasTitle`, `IHasDescription`, `IHasCode`, `IArchivable`, `ISortable`, `IHasCreated`, `IHasLastModified`, `IHasAttachments`, `IHasNormalizedContent`, `IHasObjectId<TKey>`, `ISearchObject<TKey>` |

---

## Services

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Services.Abstractions` | `IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>`, `EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>` |

---

## Query Builders

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.QueryBuilders.Abstractions` | `FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>`, `GlobalFilteredQueryBuilderBase<TEntity>`, `GlobalFilteredQueryBuilderBase<TEntity, TKey>` |
| `Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders` | `FilterIdsQueryBuilder`, `FilterArchivablesQueryBuilder`, `FilterHasCreatedQueryBuilder`, `FilterHasLastModifiedQueryBuilder`, `FilterHasNormalizedContentQueryBuilder` |
| `Regira.Entities.EFcore.QueryBuilders` | `QueryExtensions` (LINQ helpers: `FilterId`, `FilterIds`, `FilterExclude`, `FilterCode`, `FilterTitle`, `FilterNormalizedTitle`, `FilterCreated`, `FilterLastModified`, `FilterTimestamps`, `FilterQ`, `FilterArchivable`, `FilterHasAttachment`, `SortQuery`, `PageQuery`) |

---

## Processors

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Processing.Abstractions` | `IEntityProcessor<TEntity, TIncludes>` |

---

## Preppers

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Preppers.Abstractions` | `IEntityPrepper<TEntity>`, `EntityPrepperBase<TEntity>` |

---

## Primers (EF Core SaveChanges Interceptors)

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.EFcore.Primers` | `ArchivablePrimer`, `HasCreatedDbPrimer`, `HasLastModifiedDbPrimer`, `AutoTruncatePrimer`, `AddPrimerInterceptors(IServiceProvider)` *(DbContextOptionsBuilder extension)* |
| `Regira.Entities.EFcore.Primers.Abstractions` | `IEntityPrimer<T>`, `EntityPrimerBase<T>` |

---

## Normalizing

| Namespace | Types |
|-----------|-------|
| `Regira.Normalizing` | `[NormalizedAttribute]`, `ObjectNormalizer` |
| `Regira.Normalizing.Abstractions` | `INormalizer`, `IObjectNormalizer` |
| `Regira.Entities.EFcore.Normalizing` | `DefaultEntityNormalizer`, `AddNormalizerInterceptors(IServiceProvider)` *(DbContextOptionsBuilder extension)* |
| `Regira.Entities.EFcore.Normalizing.Abstractions` | `IEntityNormalizer<T>`, `EntityNormalizerBase<T>` |
| `Regira.Entities.Keywords` | `QKeywordHelper` |
| `Regira.Entities.Keywords.Abstractions` | `IQKeywordHelper` |

---

## Dependency Injection

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.DependencyInjection.ServiceBuilders.Extensions` | `UseEntities<TContext>(IServiceCollection, ...)` *(IServiceCollection extension)*, `UseDefaults()`, `For<TEntity,...>(...)` |
| `Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions` | `IEntityServiceCollection`, `IEntityServiceCollection<TContext>` |

---

## Mapping

| Namespace | Types | Notes |
|-----------|-------|-------|
| `Regira.Entities.Mapping.Mapster` | `UseMapsterMapping()` | **Default** mapping provider |
| `Regira.Entities.Mapping.AutoMapper` | `UseAutoMapper()` | Alternative mapping provider |
| `Regira.Entities.Mapping.Abstractions` | `IEntityMapper`, `IEntityAfterMapper`, `EntityAfterMapperBase<TSource, TTarget>` | |

---

## EF Core Extensions & Interceptors

| Namespace | Types |
|-----------|-------|
| `Regira.DAL.EFcore.Extensions` | `SetDecimalPrecisionConvention(int precision, int scale)` *(ModelBuilder extension)* |
| `Regira.DAL.EFcore.Services` | `AddAutoTruncateInterceptors()` *(DbContextOptionsBuilder extension)* |
| `Regira.DAL.Paging` | `PagingInfo` |

---

## Attachments

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Attachments.Models` | `EntityAttachment<TKey, TObjectKey>`, `Attachment` |
| `Regira.Entities.Web.Attachments.Abstractions` | `EntityAttachmentControllerBase<TAttachment, TKey, TObjectKey>` |
| `Regira.IO.Storage.Abstractions` | `IFileService` |
| `Regira.IO.Storage.FileSystem` | `BinaryFileService`, `FileSystemOptions` |
| `Regira.IO.Storage.Azure` | `BinaryBlobService`, `AzureOptions`, `AzureCommunicator` |
| `Regira.IO.Storage.SSH` | `SftpService`, `SftpCommunicator`, `SftpConfig` |

---

## Controllers

| Namespace | Types |
|-----------|-------|
| `Regira.Entities.Web.Controllers.Abstractions` | `EntityControllerBase<TEntity, TDto, TInputDto>`, `EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto>`, `EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto>`, `EntityControllerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TDto, TInputDto>` |
| `Microsoft.AspNetCore.Mvc` | `[ApiController]`, `[Route]`, `ControllerBase` |

---

## Common .NET / EF Core Namespaces

| Namespace | Types |
|-----------|-------|
| `System.ComponentModel.DataAnnotations` | `[Required]`, `[MaxLength]`, `[Range]` |
| `Microsoft.EntityFrameworkCore` | `DbContext`, `DbSet<T>`, `ModelBuilder`, `EntityState`, `EF.Functions.Like(...)`, `Include(...)`, `ThenInclude(...)`, `OrderBy(...)` |
| `Microsoft.EntityFrameworkCore.ChangeTracking` | `EntityEntry` |
| `Microsoft.Extensions.DependencyInjection` | `IServiceCollection`, `IServiceProvider` |

---

## Grouped by Use Case (Quick Lookup)

### Setting up a new project
```
Regira.Entities.DependencyInjection.ServiceBuilders.Extensions   → UseEntities(), UseDefaults()
Regira.Entities.Mapping.Mapster                                  → UseMapsterMapping()
Regira.Entities.EFcore.Primers                                   → AddPrimerInterceptors(sp)
Regira.Entities.EFcore.Normalizing                               → AddNormalizerInterceptors(sp)
Regira.DAL.EFcore.Services                                       → AddAutoTruncateInterceptors()
Regira.DAL.EFcore.Extensions                                     → SetDecimalPrecisionConvention()
```

### Creating an entity
```
Regira.Entities.Models.Abstractions    → IEntityWithSerial, IHasTimestamps, IHasTitle,
                                         IHasDescription, IHasCode, IArchivable,
                                         ISortable, IHasNormalizedContent, IHasAttachments
Regira.Normalizing                     → [NormalizedAttribute]
System.ComponentModel.DataAnnotations  → [Required], [MaxLength], [Range]
```

### Creating SearchObject / SortBy / Includes
```
Regira.Entities.Models   → SearchObject, SearchObject<TKey>
```
*(SortBy is a plain enum — no using required. Includes is a [Flags] enum — no using required.)*

### Building a query builder
```
Regira.Entities.EFcore.QueryBuilders.Abstractions   → FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
Regira.Entities.EFcore.QueryBuilders                → QueryExtensions (FilterId, FilterQ, etc.)
Regira.Entities.Keywords.Abstractions               → IQKeywordHelper
Microsoft.EntityFrameworkCore                       → EF.Functions.Like(...)
```

### Creating a processor
```
Regira.Entities.EFcore.Processing.Abstractions   → IEntityProcessor<TEntity, TIncludes>
```

### Creating a prepper
```
Regira.Entities.EFcore.Preppers.Abstractions   → EntityPrepperBase<TEntity>, IEntityPrepper<TEntity>
```

### Creating a primer
```
Regira.Entities.EFcore.Primers.Abstractions   → EntityPrimerBase<T>, IEntityPrimer<T>
Microsoft.EntityFrameworkCore.ChangeTracking  → EntityEntry
Microsoft.EntityFrameworkCore                 → EntityState
```

### Creating a normalizer
```
Regira.Entities.EFcore.Normalizing.Abstractions   → EntityNormalizerBase<T>, IEntityNormalizer<T>
Regira.Normalizing.Abstractions                   → INormalizer
```

### Creating a wrapping service
```
Regira.Entities.Services.Abstractions   → EntityWrappingServiceBase<...>, IEntityService<...>
Regira.Entities.Models                  → EntityInputException<T>
Regira.DAL.Paging                       → PagingInfo
```

### Creating a controller
```
Regira.Entities.Web.Controllers.Abstractions   → EntityControllerBase<...>
Microsoft.AspNetCore.Mvc                       → [ApiController], [Route]
```

### Registering an entity in DI
```
Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions   → IEntityServiceCollection<TContext>
Microsoft.EntityFrameworkCore                                       → DbContext (constraint)
```

### Setting up attachments
```
Regira.Entities.Attachments.Models                  → EntityAttachment<TKey, TObjectKey>, Attachment
Regira.Entities.Models.Abstractions                 → IHasAttachments
Regira.Entities.Web.Attachments.Abstractions        → EntityAttachmentControllerBase<...>
Regira.IO.Storage.FileSystem                        → BinaryFileService, FileSystemOptions
Regira.IO.Storage.Azure                             → BinaryBlobService, AzureOptions, AzureCommunicator
Regira.IO.Storage.SSH                               → SftpService, SftpCommunicator, SftpConfig
Regira.IO.Storage.Abstractions                      → IFileService
```
