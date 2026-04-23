# Regira Office.Barcodes AI Agent Instructions

You are an expert .NET developer working with the `Regira.Office.Barcodes` packages.
Your role is to help generate and read barcodes and QR codes using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

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

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

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
IImageFile Create(BarcodeInput input);
```

### `IBarcodeReader`

```csharp
BarcodeReadResult? Read(IImageFile img, BarcodeFormat? format = null);
```

### `IQRCodeService` (extends both)

```csharp
IImageFile          Create(string content);
BarcodeReadResult?  Read(IImageFile img, BarcodeFormat? format = null);
```

### `IBarcodeService` (extends both)

```csharp
IImageFile          Create(BarcodeInput input);
BarcodeReadResult?  Read(IImageFile img, BarcodeFormat? format = null);
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
IImageFile img = qr.Create("https://example.com");

// Generate a Code128 barcode
IBarcodeService bc = new Regira.Office.Barcodes.ZXing.BarcodeService();
IImageFile barcode = bc.Create(new BarcodeInput { Content = "ABC-1234" });

// Read / scan a barcode
BarcodeReadResult? result = bc.Read(barcode);
Console.WriteLine(result?.Contents?[0]);  // "ABC-1234"
```

---

**Load these instructions when** the user asks to generate QR codes, create barcodes, scan/read barcodes, or choose between barcode libraries.
