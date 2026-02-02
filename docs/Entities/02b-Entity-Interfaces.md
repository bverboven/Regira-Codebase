## Entity Interfaces Reference

### Essential Interfaces

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IEntity` | Id (int) | Every entity with int PK |
| `IEntity<TKey>` | Id (TKey) | Every entity without int PK |

### Identity & Keys

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IEntityWithSerial` | Serial (int) | Auto-incrementing int ID |
| `IHasAggregateKey` | AggregateKey (Guid) | Entities needing Guid identifier |
| `IHasCode` | Code (string) | Entities with short code/SKU |

### Display & Description

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasTitle` | Title (string) | Entities with name/title |
| `IHasDescription` | Description (string) | Entities with description field |

### Timestamps & Auditing

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasCreated` | Created (DateTime) | Track creation time |
| `IHasLastModified` | LastModified (DateTime?) | Track modification time |
| `IHasTimestamps` | Created, LastModified | Both timestamps |

### State & Flags

| Interface | Properties | When to Use |
|-----------|-----------|-------------|------------------|
| `IArchivable` | IsArchived (bool) | Soft delete capability |
| `IHasDefault` | IsDefault (bool) | Mark default item |
| `IHasDefault<TKey>` | IsDefault (bool), Id (TKey) | Default with typed ID |

### Date Ranges

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasStartDate` | StartDate (DateTime?) | Items with start time |
| `IHasEndDate` | EndDate (DateTime?) | Items with end time |
| `IHasStartEndDate` | StartDate, EndDate (DateTime?) | Date range (events, promotions) |

### Hierarchy & Relationships

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasObjectId<TKey>` | ObjectId (TKey) | Foreign key without navigation property |
| `IHasParentEntity` | Parent (same type) | Self-referential hierarchy |
| `IHasParentEntity<T>` | Parent (T) | Parent-child relationship |
| `ISortable` | SortOrder (int) | Custom display order |

### Security & Users

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasUserId` | UserId (string) | User-owned entities |
| `IHasPassword` | Password (string, read-only) | Password storage |
| `IHasEncryptedPassword` | EncryptedPassword (string) | Encrypted password storage |

### Web

| Interface | Properties | When to Use |
|-----------|-----------|-------------|
| `IHasUri` | Uri (string) | Entities with URL/URI |
| `IHasSlug` | Slug (string) | Entities with URL-friendly identifier |

