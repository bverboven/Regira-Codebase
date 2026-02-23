---
applyTo: "**/*.cs"
---

# Regira Entities — Namespace Reference

**Never guess namespaces. Use the exact entries listed here.**

## Core Entity Interfaces & Models
```csharp
using Regira.Entities.Models;                // SearchObject, SearchObject<TKey>, EntitySortBy
using Regira.Entities.Models.Abstractions;   // IEntity<TKey>, IEntityWithSerial
                                             // IHasTimestamps, IHasCreated, IHasLastModified
                                             // IHasTitle, IHasDescription, IHasCode
                                             // IArchivable, ISortable
                                             // IHasAttachments, IHasAttachments<T>
                                             // IHasNormalizedContent
                                             // IHasObjectId<TKey>
```

## Services
```csharp
using Regira.Entities.Services.Abstractions; // IEntityService<TEntity, TKey, ...>
                                             // EntityWrappingServiceBase<TEntity, TKey, ...>
```

## Processors & Preppers
```csharp
using Regira.Entities.EFcore.Processing.Abstractions;
                                             // IEntityProcessor<TEntity, TIncludes>
using Regira.Entities.EFcore.Preppers.Abstractions;
                                             // IEntityPrepper<TEntity>
                                             // EntityPrepperBase<TEntity>
```

## Query Builders
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
                                             // FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
                                             // GlobalFilteredQueryBuilderBase<TEntity>
                                             // GlobalFilteredQueryBuilderBase<TEntity, TKey>
                                             // IFilteredQueryBuilder<...>
                                             // ISortedQueryBuilder<...>
                                             // IIncludableQueryBuilder<...>
using Regira.Entities.EFcore.QueryBuilders;  // QueryExtensions (FilterId, FilterIds, FilterQ, …)
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
                                             // FilterIdsQueryBuilder
                                             // FilterArchivablesQueryBuilder
                                             // FilterHasCreatedQueryBuilder
                                             // FilterHasLastModifiedQueryBuilder
                                             // FilterHasNormalizedContentQueryBuilder
```

## Primers (EF Core Interceptors)
```csharp
using Regira.Entities.EFcore.Primers;        // ArchivablePrimer, HasCreatedDbPrimer
                                             // HasLastModifiedDbPrimer, AutoTruncatePrimer
                                             // AddPrimerInterceptors(IServiceProvider)
using Regira.Entities.EFcore.Primers.Abstractions;
                                             // IEntityPrimer<T>, EntityPrimerBase<T>
```

## Normalizing
```csharp
using Regira.Normalizing;                    // [NormalizedAttribute], ObjectNormalizer
using Regira.Normalizing.Abstractions;       // INormalizer, IObjectNormalizer
using Regira.Entities.EFcore.Normalizing;    // DefaultEntityNormalizer
                                             // AddNormalizerInterceptors(IServiceProvider)
using Regira.Entities.EFcore.Normalizing.Abstractions;
                                             // IEntityNormalizer<T>, EntityNormalizerBase<T>
using Regira.Entities.Keywords.Abstractions; // IQKeywordHelper
using Regira.Entities.Keywords;              // QKeywordHelper
```

## Mapping
```csharp
using Regira.Entities.Mapping.Mapster;       // UseMapsterMapping()
using Regira.Entities.Mapping.AutoMapper;    // UseAutoMapper()
using Regira.Entities.Mapping.Abstractions;  // IEntityMapper, IEntityAfterMapper
                                             // IEntityAfterMapper<TSource, TTarget>
                                             // EntityAfterMapperBase<TSource, TTarget>
```

## Dependency Injection
```csharp
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
                                             // UseEntities<TContext>(), UseDefaults()
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
                                             // IEntityServiceCollection<TContext>
```

## Controllers
```csharp
using Regira.Entities.Web.Controllers.Abstractions;
                                             // EntityControllerBase<...> (all overloads)
```

## Attachments
```csharp
using Regira.Entities.Attachments.Models;           // EntityAttachment<TKey, TObjectKey>, Attachment
using Regira.Entities.Web.Attachments.Abstractions; // EntityAttachmentControllerBase<...>
using Regira.IO.Storage.FileSystem;                 // BinaryFileService, FileSystemOptions
using Regira.IO.Storage.Abstractions;               // IFileService
using Regira.IO.Storage.Azure;                      // BinaryBlobService, AzureOptions, AzureCommunicator
using Regira.IO.Storage.SSH;                        // SftpService, SftpCommunicator, SftpConfig
```

## EF Core Extensions
```csharp
using Regira.DAL.EFcore.Services;            // AddAutoTruncateInterceptors()
                                             // AutoTruncateDbContextInterceptor
using Regira.DAL.EFcore.Extensions;          // SetDecimalPrecisionConvention(precision, scale)
using Regira.DAL.Paging;                     // PagingInfo
```

## Exceptions & Validation
```csharp
using Regira.Entities.Models;                // EntityInputException<T>
```

## Standard System / EF Namespaces
```csharp
using System.ComponentModel.DataAnnotations; // [Required], [MaxLength], [Range]
using Microsoft.EntityFrameworkCore;         // DbContext, Include(), EF.Functions.Like()
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry, EntityState
using Microsoft.AspNetCore.Mvc;              // [ApiController], [Route], [FromBody]
```
