# Dependency Injection Configuration - AI Agent Instructions

## Overview

The Regira Entity Framework uses a fluent configuration API for dependency injection. This guide shows how to properly configure services, mapping, and entity-specific behaviors.

## Basic Configuration

### Minimal Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure Entity Framework
builder.Services.UseEntities<MyDbContext>();

// 3. Build and run
var app = builder.Build();
app.Run();
```

### Standard Setup

```csharp
builder.Services
    .AddDbContext<MyDbContext>(options =>
        options.UseSqlServer(connectionString))
    .UseEntities<MyDbContext>(config =>
    {
        // Add default features
        config.UseDefaults();
        
        // Configure mapping
        config.UseMapsterMapping();
        // OR
        // config.UseAutoMapper();
    });
```

## UseEntities Configuration

### Configuration Options

```csharp
builder.Services.UseEntities<MyDbContext>(config =>
{
    // === Primers (SaveChanges interceptors) ===
    config.AddPrimer<CustomPrimer>();
    
    // === Global Query Filters ===
    config.AddGlobalQueryFilter<TenantFilterQueryBuilder>();
    
    // === Entity Normalizer ===
    config.AddEntityNormalizer<CustomEntityNormalizer>();
    
    // === Mapping ===
    config.UseAutoMapper();
    // OR
    config.UseMapsterMapping();
    
    // === Defaults (recommended) ===
    config.UseDefaults(); // Adds primers, normalizer, and global filters
});
```

### UseDefaults()

The `UseDefaults()` method adds commonly used features:

```csharp
config.UseDefaults();

// Equivalent to:
config.AddDefaultPrimers();          // Archivable, HasCreated, HasLastModified
config.AddDefaultEntityNormalizer(); // Default normalization logic
config.AddDefaultGlobalQueryFilters(); // Filter IDs, archivable, timestamps
```

**Recommendation**: Always use `UseDefaults()` unless you need granular control.

## Entity-Specific Configuration

### Basic Entity Registration

```csharp
builder.Services
    .UseEntities<MyDbContext>(config => config.UseDefaults())
    .For<Product>(p =>
    {
        // Minimal - uses defaults
    });
```

### Full Entity Configuration

```csharp
builder.Services
    .UseEntities<MyDbContext>(config => config.UseDefaults())
    .For<Product>(e =>
    {
        // === Query Filters ===
        e.AddQueryFilter<ProductQueryFilter>();
        
        // === Sorting ===
        e.SortBy<ProductSortedQueryBuilder>();
        
        // === Includes ===
        e.Includes<ProductIncludableQueryBuilder>();
        
        // === Processors (post-fetch) ===
        e.AddProcessor<ProductProcessor>();
        
        // === Preppers (pre-save) ===
        e.AddPrepper<ProductPrepper>();
        
        // === Related Entities ===
        e.Related(x => x.Reviews);
    });
```

### Related Entities (Navigation Properties)

The `Related` method declares navigation/child properties and optionally handles their preparation during save operations:

```csharp
.For<Invoice>(e =>
{    
    // Related with prepare function for child collection
    e.Related(item => item.InvoiceLines, item => item.InvoiceLines?.Prepare());
});
```

**Key Points:**
- The `Related` method creates a Prepper that handles add, modify, or delete operations on child entities
- The prepare function receives the parent entity and the collection of children to process
- **Related properties can only go 1 level deep** (no nested child-of-child relationships)
- Use for managing one-to-many relationships (e.g., Invoice → InvoiceLines)

### Shorthand Configuration Methods

```csharp
.For<Product>(e =>
{
    // Direct sorting lambda (instead of builder class)
    e.SortBy((query, sortBy) => sortBy switch
    {
        ProductSortBy.Title => query.OrderBy(x => x.Title),
        ProductSortBy.Price => query.OrderBy(x => x.Price),
        _ => query.OrderBy(x => x.Id)
    });
    
    // Direct includes lambda
    e.Includes((query, include) =>
    {
        if (include.HasFlag(ProductIncludes.Category))
            query = query.Include(x => x.Category);
        return query;
    });
})
```

## Multiple Entity Configuration

### Configuring Multiple Entities

```csharp
builder.Services
    .UseEntities<MyDbContext>(config => 
    {
        config.UseDefaults();
        config.UseMapsterMapping();
    })
    .For<Category>(e =>
    {
        e.SortBy((q, _) => q.OrderBy(x => x.Title));
        e.Includes((q, _) => q.Include(x => x.Products));
        e.Related(c => c.Products);
    })
    .For<Product>(e =>
    {
        e.AddQueryFilter<ProductQueryFilter>();
        e.SortBy<ProductSortedQueryBuilder>();
        e.Includes<ProductIncludableQueryBuilder>();
        e.AddProcessor<ProductProcessor>();
        e.AddPrepper<ProductPrepper>();
        e.Related(p => p.Category);
        e.Related(p => p.Reviews);
    })
    .For<Review>(e =>
    {
        e.SortBy((q, _) => q.OrderByDescending(x => x.Created));
        e.Related(r => r.Product);
    });
```

## Mapping Configuration

### Using AutoMapper

```csharp
// 1. Configure in UseEntities
builder.Services
    .UseEntities<MyDbContext>(config => 
    {
        config.UseDefaults();
        config.UseAutoMapper();
    });

// 2. Create mapping profiles
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category!.Title));
            
        CreateMap<ProductInput, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Created, o => o.Ignore());
    }
}

// 3. Register profiles
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
```

### Using Mapster

```csharp
// 1. Configure in UseEntities
builder.Services
    .UseEntities<MyDbContext>(config => 
    {
        config.UseDefaults();
        config.UseMapsterMapping();
    });

// 2. Configure mappings
public static class MappingConfiguration
{
    public static void ConfigureMappings()
    {
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category!.Title);
            
        TypeAdapterConfig<ProductInput, Product>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Created);
    }
}

// 3. Call during startup
MappingConfiguration.ConfigureMappings();
```

## Primers (Database Interceptors)

### Built-in Primers

```csharp
builder.Services.UseEntities<MyDbContext>(config =>
{
    // Add individual primers
    config.AddPrimer<ArchivablePrimer>();
    config.AddPrimer<HasCreatedDbPrimer>();
    config.AddPrimer<HasLastModifiedDbPrimer>();
    
    // OR use defaults (includes all above)
    config.AddDefaultPrimers();
});
```

### Custom Primers

```csharp
public class TenantPrimer : EntityPrimerBase<IHasTenantId>
{
    private readonly ITenantProvider _tenantProvider;
    
    public TenantPrimer(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }
    
    protected override Task ProcessAsync(
        IHasTenantId entity, 
        EntityState state, 
        CancellationToken cancellationToken)
    {
        if (state == EntityState.Added)
        {
            entity.TenantId = _tenantProvider.GetCurrentTenantId();
        }
        return Task.CompletedTask;
    }
}

// Register
builder.Services.UseEntities<MyDbContext>(config =>
{
    config.AddPrimer<TenantPrimer>();
});
```

## Global Query Filters

### Built-in Global Filters

```csharp
builder.Services.UseEntities<MyDbContext>(config =>
{
    // Add individual filters
    config.AddGlobalQueryFilter<FilterIdsQueryBuilder>();
    config.AddGlobalQueryFilter<FilterArchivablesQueryBuilder>();
    config.AddGlobalQueryFilter<FilterHasCreatedQueryBuilder>();
    config.AddGlobalQueryFilter<FilterHasLastModifiedQueryBuilder>();
    
    // OR use defaults (includes all above)
    config.AddDefaultGlobalQueryFilters();
});
```

### Custom Global Filters

```csharp
public class TenantGlobalFilter : IGlobalFilteredQueryBuilder
{
    private readonly ITenantProvider _tenantProvider;
    
    public TenantGlobalFilter(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }
    
    public IQueryable<TEntity> Build<TEntity>(
        IQueryable<TEntity> query, 
        object? search) 
        where TEntity : class
    {
        if (typeof(IHasTenantId).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantId = _tenantProvider.GetCurrentTenantId();
            return query.Where(e => ((IHasTenantId)e).TenantId == tenantId);
        }
        return query;
    }
}

// Register
builder.Services.UseEntities<MyDbContext>(config =>
{
    config.AddGlobalQueryFilter<TenantGlobalFilter>();
});
```

## Custom Services

### Registering Custom Services

```csharp
builder.Services
    .UseEntities<MyDbContext>(config => config.UseDefaults())
    .For<Product>(e =>
    {
        e.AddQueryFilter<ProductQueryFilter>();
    });

// Override with custom service
builder.Services.AddScoped<IEntityService<Product, int, ProductSearchObject>, ProductService>();

// OR if using custom interface
builder.Services.AddScoped<IProductService, ProductService>();
```

## Complete Configuration Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ContosoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure controllers
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure entity framework
builder.Services
    .UseEntities<ContosoContext>(config =>
    {
        // Use all defaults
        config.UseDefaults();
        
        // Add custom primers
        config.AddPrimer<TenantPrimer>();
        
        // Add custom global filters
        config.AddGlobalQueryFilter<TenantGlobalFilter>();
        
        // Configure mapping
        config.UseMapsterMapping();
    })
    .For<Category>(e =>
    {
        e.SortBy((q, _) => q.OrderBy(x => x.Title));
        e.Includes((q, _) => q.Include(x => x.Products));
        e.Related(c => c.Products);
    })
    .For<Product>(e =>
    {
        e.AddQueryFilter<ProductQueryFilter>();
        e.SortBy<ProductSortedQueryBuilder>();
        e.Includes<ProductIncludableQueryBuilder>();
        e.AddProcessor<ProductProcessor>();
        e.AddPrepper<ProductPrepper>();
        e.Related(p => p.Category);
        e.Related(p => p.Reviews);
    })
    .For<Review>(e =>
    {
        e.SortBy((q, _) => q.OrderByDescending(x => x.Created));
        e.AddPrepper<ReviewPrepper>();
        e.Related(r => r.Product);
        e.Related(r => r.User);
    });

// Add custom services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Configure mapping
MappingConfiguration.ConfigureMappings();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Configuration Checklist

When setting up a new application:

- [ ] Add DbContext with connection string
- [ ] Call `UseEntities<TContext>()`
- [ ] Add global Queryfilters, Preppers, Processors, ... or optionally Call `UseDefaults()`
- [ ] Choose mapping strategy (AutoMapper or Mapster)
- [ ] Configure each entity with `.For<TEntity, ...>()`
- [ ] Add entity-specific filters, sorting, includes
- [ ] Register custom primers if needed
- [ ] Register custom global filters if needed
- [ ] Register custom services if needed
- [ ] Configure mapping profiles/configuration with AfterMappers if required

## Next Steps

- See complete examples: [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
- Review entity models: [Models Instructions](AI-INSTRUCTIONS-MODELS.md)
- Review services: [Services Instructions](AI-INSTRUCTIONS-SERVICES.md)
- Review controllers: [Controllers Instructions](AI-INSTRUCTIONS-CONTROLLERS.md)
