# Entities

Read & write entities based on a Repository pattern.
The base is a ```IEntityService<TEntity, TKey>``` service which can be extended with a search filter, sorting and defining of related entities to include in the resulting collection.

For an API, a ```EntityControllerBase<TEntity, TKey>``` is provided to handle the CRUD operations. This controller will automatically use the matching EntityService. Mapping to DTO is provided by an Automapper implementation.

## Samples

### Basic

Dependency injection

```csharp
services
    .AddDbContext<ContosoContext>(db => db.UseSqlite(ApiConfiguration.ConnectionString))
    .UseEntities<ContosoContext>(o o.UseDefaults())
    .For<Department>();
```

Controller

```csharp
[ApiController]
[Route("departments")]
public class DepartmentController : EntityControllerBase<Department>;
```

### Advanced (with attachments)

Dependency injection

```csharp
services
    .AddDbContext<ContosoContext>((sp, db) =>
    {
        db.UseSqlite(ApiConfiguration.ConnectionString)
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors();
    })
    .UseEntities<ContosoContext>(o =>
    {
        o.UseDefaults();
        o.AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>();
        o.UseAutoMapper([typeof(Person).Assembly]);
    })
    .WithAttachments(_ => new BinaryFileService(new FileSystemOptions { RootFolder = ApiConfiguration.AttachmentsDirectory }))
    .For<Person, PersonSearchObject, PersonSortBy, PersonIncludes>(e =>
    {
        e.Related(x => x.Departments);
        e.HasAttachments(x => x.Attachments);
        e.AddQueryFilter<PersonQueryFilter>();
        e.Includes<PersonIncludableQueryBuilder>();
        e.SortBy<PersonSortQueryBuilder>();
        e.AddNormalizer<PersonNormalizer>();
        e.HasManager<PersonManager>();
        e.AddMapping<PersonDto, PersonInputDto>();
    });
```

Controller

```csharp
[ApiController]
[Route("persons")]
public class PersonController : EntityControllerBase<Person, int, PersonSearchObject, PersonSortBy, PersonIncludes, PersonDto, PersonInputDto>;

[ApiController]
[Route("persons")]
public class PersonAttachmentController : EntityAttachmentControllerBase<PersonAttachment>;
```

## Composition

### Fetching entities

- DbSet (base query)
- Filter
- Sort
- Include related
- Paging
- Process entities

### Saving and removing entities

- Prepare entities
    - e.g. Sort related entities
- React to modifications (interceptors)
    - e.g. [Set LastModified date](../Entities.EFcore/Primers/HasLastModifiedDbPrimer.cs)
    - e.g. [Archive instead of delete entities](../Entities.EFcore/Primers/ArchivablePrimer.cs)
