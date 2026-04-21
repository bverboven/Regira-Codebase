# Regira DAL

Regira DAL provides lightweight database connectivity and backup/restore abstractions for MongoDB, MySQL, and PostgreSQL. All settings classes extend the shared `DbSettingsBase` from [Common](../Common/01-index.md#dal-abstractions).

## Projects

| Project | Package | Backend | CRUD | Backup / Restore |
|---------|---------|---------|------|-----------------|
| `DAL.MongoDB` | `Regira.DAL.MongoDB` | MongoDB | via MongoDB Driver | ✓ (mongodump) |
| `DAL.MySQL` | `Regira.DAL.MySQL` | MySQL / MariaDB | via Dapper | — |
| `DAL.MySQL.MySqlBackup` | `Regira.DAL.MySQL.MySqlBackup` | MySQL / MariaDB | — | ✓ (MySqlBackup.NET) |
| `DAL.PostgreSQL` | `Regira.DAL.PostgreSQL` | PostgreSQL | via Dapper | ✓ (pg_dump) |

## Installation

```xml
<PackageReference Include="Regira.DAL.MongoDB"        Version="5.*" />
<PackageReference Include="Regira.DAL.MySQL"          Version="5.*" />
<PackageReference Include="Regira.DAL.MySQL.MySqlBackup" Version="5.*" />
<PackageReference Include="Regira.DAL.PostgreSQL"     Version="5.*" />
```

---

## MongoDB

### MongoSettings

| Property | Type | Description |
|----------|------|-------------|
| `Host` | `string` | Hostname / replica set |
| `DatabaseName` | `string` | Target database |
| `Port` | `string` | Port (default `27017`) |
| `Username` | `string?` | Auth username |
| `Password` | `string?` | Auth password |
| `UseSecure` (UseTls) | `bool` | TLS/SSL |

```csharp
var settings = new MongoSettings("localhost", "mydb");
// or parse from connection string:
var settings = MongoSettings.FromConnectionString("mongodb://user:pass@host:27017/mydb");
```

### MongoCommunicator

```csharp
var comm = new MongoCommunicator(settings);
// Access underlying IMongoDatabase via comm.Database
var names = comm.ListCollectionNames();   // IAsyncEnumerable<string>
```

### MongoDbRepositoryBase\<TEntity\>

Extend this to build a repository. Override `GetFilter()`, `SortResult()`, and `PageResult()` for custom queries.

```csharp
public class ProductRepository(MongoCommunicator comm)
    : MongoDbRepositoryBase<Product>(comm)
{
    protected override FilterDefinition<Product> GetFilter(object? so) { … }
}
```

CRUD methods:

```csharp
Task<TEntity?>             Details(object id)
Task<IEnumerable<TEntity>> List(object? searchObject)
Task<int>                  Count(object? searchObject)
Task                       Save(TEntity item)
Task                       Delete(TEntity item)
```

### MongoBackupService / MongoRestoreService

Requires `mongodump` / `mongorestore` executables in `MongoOptions.ToolsDirectory`.

```csharp
var svc = new MongoBackupService(new MongoOptions
{
    DbSettings     = settings,
    ToolsDirectory = "/usr/bin"
});
IMemoryFile backup = await svc.Backup();
```

---

## MySQL

### MySqlSettings

| Property | Type | Description |
|----------|------|-------------|
| `Host` | `string` | Hostname |
| `DatabaseName` | `string` | Target database |
| `Port` | `string` | Default `"3306"` |
| `Username` | `string?` | Auth username |
| `Password` | `string?` | Auth password |

```csharp
var settings = new MySqlSettings("localhost", "mydb", username: "root", password: "pass");
string cs    = settings.BuildConnectionString();
```

### MySqlCommunicator

Extends `DbCommunicator<MySqlConnection>` (Dapper). Execute raw queries via the underlying `DbConnection`.

```csharp
var comm = new MySqlCommunicator(settings.BuildConnectionString());
await comm.Open();
var rows = await comm.DbConnection.QueryAsync<Product>("SELECT * FROM products");
```

### SQLDumpManager

Corrects query ordering in a SQL dump file to ensure foreign key constraints are satisfied during restoration.

```csharp
var mgr = new SQLDumpManager(settings);
mgr.OnAction = (msg, data) => Console.WriteLine(msg);

var result = await mgr.CorrectQuerySequence(sqlDumpContent);
// result.Output  — corrected SQL
// result.Failed  — queries that could not be ordered
```

### MySqlBackupService / MySqlRestoreService

Stream-based — no temp files.

```csharp
var options = new MySqlBackupOptions { DbSettings = settings };

IMemoryFile backup = await new MySqlBackupService(options).Backup();
await new MySqlRestoreService(options).Restore(backup);
```

---

## PostgreSQL

### PgSettings

| Property | Type | Description |
|----------|------|-------------|
| `Host` | `string` | Hostname |
| `DatabaseName` | `string` | Target database |
| `Username` | `string?` | Auth username |
| `Password` | `string?` | Auth password |
| `Port` | `string` | Default `"5432"` |

```csharp
var settings = new PgSettings("localhost", "mydb", "postgres", "pass");
```

### PgBackupService / PgRestoreService

Requires `pg_dump` / `pg_restore` executables. Supports schema-specific backups.

```csharp
var options = new PgOptions
{
    DbSettings     = settings,
    ToolsDirectory = "/usr/lib/postgresql/16/bin",
    BackupSchemas  = ["public", "reports"],
    Overwrite      = true
};

IMemoryFile backup = await new PgBackupService(options, processHelper).Backup();
await new PgRestoreService(options, processHelper).Restore(backup);
```

`PgRestoreService` can also create the target database if it does not exist:

```csharp
await pgRestore.Create(connection, "new_database");
bool exists = await pgRestore.Exists(connection, "new_database");
```

### BackupRestoreManager

Standalone manager — useful when you want both backup and restore from the same object.

```csharp
var mgr = new BackupRestoreManager(processHelper, options);
await mgr.Backup(settings, "sourceDb", "/backups/snapshot.dump");
await mgr.Restore(settings, "targetDb", "/backups/snapshot.dump", overwrite: true);
```

---

## Backup/Restore contracts (from Common)

```csharp
public interface IDbBackupService
{
    Task<IMemoryFile> Backup();
}

public interface IDbRestoreService
{
    Task Restore(IMemoryFile file);
}
```

---

## Overview

1. **[Index](01-index.md)** — MongoDB, MySQL, PostgreSQL settings, communicators, and backup/restore
1. [Examples](02-examples.md) — Connect, query, backup, and restore
