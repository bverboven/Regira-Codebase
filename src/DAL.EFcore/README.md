# Regira DAL — EF Core

Regira DAL.EFcore provides Entity Framework Core extensions and utilities for change tracking, string normalization, auto-truncation, and model configuration.

## Projects

| Project | Package | Description |
|---------|---------|-------------|
| `DAL.EFcore` | `Regira.DAL.EFcore` | EF Core extensions for DbContext, ModelBuilder, and interceptors |

## Installation

```xml
<PackageReference Include="Regira.DAL.EFcore" Version="5.*" />
```

---

## DbContext Extensions

### `GetPendingEntries`

Returns all tracked entries with pending changes (`Added`, `Modified`, or `Deleted`).

```csharp
var pending = dbContext.GetPendingEntries();
var pendingProducts = dbContext.GetPendingEntries<Product>();
```

### `SaveAndCleanUpOnError`

Wraps `SaveChangesAsync` and rolls back only the failing entries on a `DbUpdateException`, allowing the remaining entries to be retried.

```csharp
await dbContext.SaveAndCleanUpOnError();
```

### `AutoNormalizeStringsForEntries`

Runs string normalization (via `NormalizingUtility`) over all pending non-deleted entries.

```csharp
dbContext.AutoNormalizeStringsForEntries();
// or with custom options:
dbContext.AutoNormalizeStringsForEntries(new NormalizingOptions { … });
```

### `AddRegisteredInterceptors`

Discovers all `IInterceptor` registrations from an `IServiceCollection` and adds them to the `DbContextOptionsBuilder`.

```csharp
optionsBuilder.AddRegisteredInterceptors(services);
```

---

## Auto-Truncate

Automatically truncates string values to the maximum length defined by `[MaxLength]` or `[StringLength]` attributes before saving.

### `AutoTruncateStringsToMaxLengthForEntries` (extension method)

Call manually inside `SaveChanges` / `SaveChangesAsync` overrides:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    this.AutoTruncateStringsToMaxLengthForEntries();
    return await base.SaveChangesAsync(ct);
}
```

### `AutoTruncateDbContextInterceptor` (.NET Core 3.1+)

Register as an interceptor for automatic truncation on every save without touching the `DbContext`:

```csharp
// Manual registration
optionsBuilder.AddAutoTruncateInterceptors();

// Or register in DI and auto-discover with AddRegisteredInterceptors
services.AddSingleton<IInterceptor, AutoTruncateDbContextInterceptor>();
optionsBuilder.AddRegisteredInterceptors(services);
```

---

## ModelBuilder Extensions

### `SetDecimalPrecisionConvention`

Applies a uniform precision and scale to all `decimal` properties in the model.

```csharp
// In OnModelCreating (all TFMs)
modelBuilder.SetDecimalPrecisionConvention(precision: 18, scale: 4);

// In ConfigureConventions (.NET 6+)
configurationBuilder.SetDecimalPrecisionConvention(precision: 18, scale: 4);
```

---

## EntityType / Entry Extensions

### `GetPropertyAttributes`

Retrieves the data-annotation attributes for each property of an entity entry, with results cached per entity type to avoid repeated reflection.

```csharp
var attributes = entry.GetPropertyAttributes();
// IDictionary<IProperty, Attribute[]>
```

---

## ServiceCollection Extensions

### `CollectDescriptors<TService>`

Finds all service descriptors in an `IServiceCollection` that implement the given interface — used internally by `AddRegisteredInterceptors`.

```csharp
var interceptorDescriptors = services.CollectDescriptors<IInterceptor>();
```
