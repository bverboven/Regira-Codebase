# Regira Office.Excel AI Agent Instructions

You are an expert .NET developer working with the `Regira.Office.Excel` packages.
Your role is to help read and write Excel workbooks using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<!-- MiniExcel — recommended (streaming, generic, lightweight) -->
<PackageReference Include="Regira.Office.Excel.MiniExcel" Version="5.*" />

<!-- ClosedXML -->
<PackageReference Include="Regira.Office.Excel.ClosedXML" Version="5.*" />

<!-- EPPlus v4 (free licence) -->
<PackageReference Include="Regira.Office.Excel.EPPlus" Version="5.*" />

<!-- NpoiMapper (type-mapped, generic) -->
<PackageReference Include="Regira.Office.Excel.NpoiMapper" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Backend Comparison

| Package | Backend | Generic `<T>` | Streaming | Notes |
|---------|---------|--------------|-----------|-------|
| `Regira.Office.Excel.MiniExcel` | MiniExcel | ✓ | ✓ | Recommended — fast, low memory |
| `Regira.Office.Excel.ClosedXML` | ClosedXML | — | — | Rich formatting support |
| `Regira.Office.Excel.EPPlus` | EPPlus v4 | — | — | Free licence (v4) |
| `Regira.Office.Excel.NpoiMapper` | NPOI + Npoi.Mapper | ✓ | — | Type-mapped via attributes |

**Default recommendation:** Use `MiniExcel` for reading/writing data — supports generics, streaming, and has minimal memory overhead.

---

## Interfaces

### `IExcelReader` / `IExcelReader<T>`

```csharp
IEnumerable<ExcelSheet>    Read(IBinaryFile input, string[]? headers = null);
IEnumerable<ExcelSheet<T>> Read(IBinaryFile input, string[]? headers = null);  // generic
```

`headers` — when supplied, only those columns are returned (by name).

### `IExcelWriter` / `IExcelWriter<T>`

```csharp
IMemoryFile Create(IEnumerable<ExcelSheet> sheets);
IMemoryFile Create(IEnumerable<ExcelSheet<T>> sheets);  // generic
```

### `IExcelManager` / `IExcelManager<T>`

Composite: `IExcelReader + IExcelWriter` (and their generic variants).

---

## Models

### `ExcelSheet` / `ExcelSheet<T>`

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string?` | Sheet tab name |
| `Data` | `IEnumerable<IDictionary<string, object>>?` | Rows as key-value dictionaries |

`ExcelSheet<T>` has `Data` typed as `IEnumerable<T>`.

---

## Usage

```csharp
// Construct directly (no DI extensions — pick any implementation)
IExcelManager excel = new Regira.Office.Excel.MiniExcel.ExcelManager();

// Read all sheets
IBinaryFile file = bytes.ToBinaryFile("report.xlsx");
var sheets       = excel.Read(file);
foreach (var sheet in sheets)
    foreach (var row in sheet.Data!)
        Console.WriteLine(row);   // Dictionary<string, object>

// Read typed
IExcelManager<Product> typed = new Regira.Office.Excel.MiniExcel.ExcelManager<Product>();
var sheets = typed.Read(file);
foreach (var product in sheets.First().Data!)
    Console.WriteLine(product.Name);

// Write sheets to a new workbook
IMemoryFile output = excel.Create(sheets);
```

---

**Load these instructions when** the user asks to read or write Excel files, import spreadsheet data, or export data to `.xlsx` format.
