# Regira Office.Barcodes AI Agent Instructions

---

## Module Context

Part of **Regira Office**. For routing and full module overview, see [`office.instructions.md`](./office.instructions.md).

| Namespace | Covers |
|-----------|--------|
| `Regira.Office.Barcodes` | Barcode and QR code generation and scanning |

**Related:**
- [Media / Drawing](./media.instructions.md) — `IImageFile` is the return type of `Create()`
- [IO.Storage](./io.storage.instructions.md) — `IMemoryFile` for file input/output

---

## Installation

```xml
<!-- ZXing — recommended (all formats, cross-platform) -->
<PackageReference Include="Regira.Office.Barcodes.ZXing" Version="5.*" />

<!-- Spire — all formats, net8+ -->
<PackageReference Include="Regira.Office.Barcodes.Spire" Version="5.*" />

<!-- QRCoder — write-only QR -->
<PackageReference Include="Regira.Office.Barcodes.QRCoder" Version="5.*" />

<!-- UziGranot — embedded QR, no external dependency -->
<PackageReference Include="Regira.Office.Barcodes.UziGranot" Version="5.*" />
```

> Shared setup: see [`shared.setup.md`](./shared.setup.md) — **NuGet feed**.

---

## Backend Comparison

| Package | Backend | Formats | Read | Write | Cross-platform |
|---------|---------|---------|------|-------|----------------|
| `Regira.Office.Barcodes.ZXing` | ZXing.Net (SkiaSharp) | All 13 | ✓ | ✓ | ✓ |
| `Regira.Office.Barcodes.Spire` | FreeSpire.Barcode | All 13 | ✓ | ✓ | net8+ |
| `Regira.Office.Barcodes.QRCoder` | QRCoder | QR only | — | ✓ | ✓ |
| `Regira.Office.Barcodes.UziGranot` | Embedded (CPOL) | QR only | ✓ | ✓ | ✓ |

**Default recommendation:** Use `ZXing` for general use — supports all formats, read+write, fully cross-platform.

---

## Interfaces

### `IBarcodeWriter`

```csharp
Task<IImageFile> Create(BarcodeInput input, CancellationToken cancellationToken = default);
```

### `IBarcodeReader`

```csharp
Task<BarcodeReadResult?> Read(IImageFile img, BarcodeFormat? format = null, CancellationToken cancellationToken = default);
```

### `IQRCodeService` (extends both)

```csharp
Task<IImageFile>          Create(string content, CancellationToken cancellationToken = default);
Task<BarcodeReadResult?>  Read(IImageFile img, BarcodeFormat? format = null, CancellationToken cancellationToken = default);
```

### `IBarcodeService` (extends both)

```csharp
Task<IImageFile>          Create(BarcodeInput input, CancellationToken cancellationToken = default);
Task<BarcodeReadResult?>  Read(IImageFile img, BarcodeFormat? format = null, CancellationToken cancellationToken = default);
```

---

## Models

### `BarcodeInput`

| Property | Type | Description |
|----------|------|-------------|
| `Content` | `string` | Data to encode |
| `Format` | `BarcodeFormat?` | Barcode format (e.g. `BarcodeFormat.Code128`) |
| `Size` | `ImageSize?` | Output image dimensions |

### `BarcodeReadResult`

| Property | Type | Description |
|----------|------|-------------|
| `Contents` | `IList<string>?` | Decoded values (one per barcode found) |
| `Format` | `BarcodeFormat?` | Detected format |

---

## Usage

```csharp
// Generate a QR code
IQRCodeService qr = new Regira.Office.Barcodes.ZXing.QRCodeService();
IImageFile img = await qr.Create("https://example.com");

// Generate a Code128 barcode
IBarcodeService bc = new Regira.Office.Barcodes.ZXing.BarcodeService();
IImageFile barcode = await bc.Create(new BarcodeInput { Content = "ABC-1234" });

// Read / scan a barcode
BarcodeReadResult? result = await bc.Read(barcode);
Console.WriteLine(result?.Contents?[0]);  // "ABC-1234"
```

