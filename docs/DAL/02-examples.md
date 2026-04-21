# Regira DAL — Examples

## Example 1: Connect to MongoDB and run a query

```csharp
var settings = MongoSettings.FromConnectionString(
    configuration.GetConnectionString("MongoDB")!);

var comm   = new MongoCommunicator(settings);
var repo   = new ProductRepository(comm);

var products = await repo.List(new ProductSearchObject { Category = "electronics" });
```

---

## Example 2: Custom MongoDB repository

```csharp
public class ProductRepository(MongoCommunicator comm)
    : MongoDbRepositoryBase<Product>(comm)
{
    protected override FilterDefinition<Product> GetFilter(object? so)
    {
        var filter = Builders<Product>.Filter.Empty;

        if (so is ProductSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Category))
                filter &= Builders<Product>.Filter.Eq(p => p.Category, search.Category);

            if (search.MinPrice.HasValue)
                filter &= Builders<Product>.Filter.Gte(p => p.Price, search.MinPrice.Value);
        }

        return filter;
    }

    protected override IFindFluent<Product, Product> SortResult(
        IFindFluent<Product, Product> query, object? so)
        => query.SortBy(p => p.Name);
}
```

---

## Example 3: Backup a MongoDB database

```csharp
var options = new MongoOptions
{
    DbSettings     = new MongoSettings("mongo.example.com", "prod-db", username: "admin", password: "pass"),
    ToolsDirectory = "/usr/bin"
};

IMemoryFile backup = await new MongoBackupService(options).Backup();

// Store the backup via IO.Storage
await fileService.Save($"backups/{DateTime.Today:yyyyMMdd}.archive", backup.Bytes!);
```

---

## Example 4: MySQL — connect and query with Dapper

```csharp
var settings = new MySqlSettings("localhost", "mydb", username: "root", password: "pass");
var comm     = new MySqlCommunicator(settings.BuildConnectionString());

await comm.Open();
var orders = await comm.DbConnection.QueryAsync<Order>(
    "SELECT * FROM orders WHERE status = @status",
    new { status = "pending" });
await comm.Close();
```

---

## Example 5: MySQL — backup and restore

```csharp
var options = new MySqlBackupOptions { DbSettings = settings };

// Backup
IMemoryFile backup = await new MySqlBackupService(options).Backup();
await fileService.Save("backups/mysql-latest.sql.gz", backup.Bytes!);

// Restore to a different database
var restoreOptions = new MySqlBackupOptions
{
    DbSettings = new MySqlSettings("localhost", "mydb-staging", username: "root", password: "pass")
};
IMemoryFile snapshot = (await fileService.GetBytes("backups/mysql-latest.sql.gz"))!
                            .ToBinaryFile("snapshot.sql.gz");
await new MySqlRestoreService(restoreOptions).Restore(snapshot);
```

---

## Example 6: PostgreSQL — schema-specific backup

```csharp
var options = new PgOptions
{
    DbSettings     = new PgSettings("pg.example.com", "prod-db", "postgres", "pass"),
    ToolsDirectory = "/usr/lib/postgresql/16/bin",
    BackupSchemas  = ["public"],
    Overwrite      = true
};

var processHelper = new ProcessHelper();  // or inject IProcessHelper

IMemoryFile backup = await new PgBackupService(options, processHelper).Backup();
```

---

## Example 7: PostgreSQL — create database then restore

```csharp
await using var conn = new NpgsqlConnection(adminConnectionString);
await conn.OpenAsync();

var restorer = new PgRestoreService(options, processHelper);

if (!await restorer.Exists(conn, "staging-db"))
    await restorer.Create(conn, "staging-db");

await restorer.Restore(backup);
```

---

## Overview

1. [Index](01-index.md) — MongoDB, MySQL, PostgreSQL settings, communicators, and backup/restore
1. **[Examples](02-examples.md)** — Connect, query, backup, and restore
