# Regira Serializing

Regira Serializing provides JSON serialisation via the `ISerializer` contract defined in [Common](../Common#serializing).

## Projects

| Project | Package | Backend |
|---------|---------|---------|
| `Serializing.Newtonsoft` | `Regira.Serializing.Newtonsoft` | Newtonsoft.Json |

## Installation

```xml
<PackageReference Include="Regira.Serializing.Newtonsoft" Version="5.*" />
```

## JsonSerializer

Implements `ISerializer`. Registers as a singleton in most consuming projects.

```csharp
ISerializer json = new Regira.Serializing.Newtonsoft.Json.JsonSerializer();

string s    = json.Serialize(myObject);
MyType obj  = json.Deserialize<MyType>(s)!;
object dyn  = json.Deserialize(s, typeof(MyType))!;
```

### Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnumAsString` | `bool` | `true` | Serialise enums as their name, not integer |
| `BoolAsNumber` | `bool` | `true` | Serialise `bool` as `1`/`0` |
| `IgnoreNullValues` | `bool` | `true` | Omit null properties |
| `WriteIndented` | `bool` | `false` | Pretty-print JSON |

```csharp
ISerializer json = new JsonSerializer(new JsonSerializer.Options
{
    EnumAsString  = true,
    WriteIndented = true
});
```

### Built-in converters

| Converter | Handles |
|-----------|---------|
| `BoolNumberConverter` | `bool` ↔ `0`/`1` |
| `DateAndTimeConverter` | `DateTime` / `DateTimeOffset` |
| `DateOnlyJsonConverter` | `DateOnly` (.NET 5+) |
| `GuidConverter` | `Guid` |

All converters apply camelCase property naming and reference loop ignoring.

### DI Registration

```csharp
services.AddSingleton<ISerializer, Regira.Serializing.Newtonsoft.Json.JsonSerializer>();
```
