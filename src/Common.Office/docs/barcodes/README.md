# Regira Barcodes

Regira Barcodes provides a **unified abstraction** for generating and reading barcodes and QR codes across multiple underlying libraries. All implementations share the same `IBarcodeService` / `IQRCodeService` interfaces.

## Projects

| Project | Package | Backend | Formats | Read | Write | Cross-platform |
|---------|---------|---------|---------|------|-------|----------------|
| `Common.Office` | *(transitive)* | Shared abstractions | — | — | — | — |
| `Barcodes.ZXing` | `Regira.Office.Barcodes.ZXing` | ZXing.Net (SkiaSharp) | All 13 | ✓ | ✓ | ✓ |
| `Barcodes.Spire` | `Regira.Office.Barcodes.Spire` | FreeSpire.Barcode | All 13 | ✓ | ✓ | net8+ |
| `Barcodes.QRCoder` | `Regira.Office.Barcodes.QRCoder` | QRCoder | QR only | — | ✓ | ✓ |
| `Barcodes.UziGranot` | `Regira.Office.Barcodes.UziGranot` | Embedded (CPOL) | QR only | ✓ | ✓ | ✓ |

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

## Quick Start

```csharp
// Generate a QR code
IQRCodeService qr = new Regira.Office.Barcodes.ZXing.QRCodeService();
IImageFile img = qr.Create("https://example.com");

// Generate a Code128 barcode
IBarcodeService bc = new Regira.Office.Barcodes.ZXing.BarcodeService();
IImageFile barcode = bc.Create(new BarcodeInput { Content = "ABC-1234" });

// Read / scan
BarcodeReadResult? result = bc.Read(barcode);
Console.WriteLine(result?.Contents?[0]);   // "ABC-1234"
```

## Interfaces

### IBarcodeWriter

```csharp
IImageFile Create(BarcodeInput input);
```

### IBarcodeReader

```csharp
BarcodeReadResult? Read(IImageFile img, BarcodeFormat? format = null);
```

Pass `format` to narrow the scanner to a specific type; omit to try all supported formats.

### IQRCodeWriter / IQRCodeReader

```csharp
IImageFile        Create(QRCodeInput input);
BarcodeReadResult? Read(IImageFile qrCode);
```

### IBarcodeService / IQRCodeService

Composite interfaces that combine read + write. Use these as the injection target.

## Input Models

### BarcodeInput

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `string` | *(required)* | Data to encode |
| `Format` | `BarcodeFormat` | `Code128` | Barcode symbology |
| `Size` | `ImageSize` | `400 × 100` | Output image dimensions |
| `Color` | `Color` | `Black` | Bar / module colour |
| `BackgroundColor` | `Color` | `White` | Background colour |

Implicit conversion from `string` creates a `BarcodeInput` with default options:

```csharp
BarcodeInput input = "ABC-1234";
```

### QRCodeInput

Inherits `BarcodeInput`. Format is locked to `QRCode`. Default size is square (`Width × Width`).

```csharp
QRCodeInput qr = "https://example.com";   // implicit
var qr = new QRCodeInput { Content = "Hello", Size = new ImageSize(300, 300) };
```

## Output — BarcodeReadResult

| Property | Type | Description |
|----------|------|-------------|
| `Format` | `BarcodeFormat?` | Detected symbology |
| `Contents` | `string[]?` | Decoded values (some implementations can detect multiple codes in one image) |

## BarcodeFormat

Flags enum — can be combined with `|`. The `Any` value scans for all supported symbologies.

```
Code39  Code93  Code128  CodaBar  DataMatrix
Ean8    Ean13   Itf      Upca     Upce
QRCode  Pdf417  Aztec    Any
```

## Implementation notes

### ZXing (recommended)

Uses SkiaSharp — cross-platform. Full 13-format bidirectional support. Has a two-phase read: first attempt, then retry with `TryHarder = true` and `AutoRotate = true` for difficult images. Respects `Color` and `BackgroundColor` from input.

### Spire

Uses GDI+ (net8+). All formats. Background colour is always white regardless of input. Format detection on read returns `null` for the detected format field.

### QRCoder

Write-only. No `IBarcodeReader`. Uses GDI+ internally.

### UziGranot

Self-contained — no NuGet dependency. Generates PNG directly with zlib compression. Can detect multiple QR codes in a single image. Error correction level fixed at M.

## Implementation comparison

| Feature | ZXing | Spire | QRCoder | UziGranot |
|---------|-------|-------|---------|-----------|
| **All 13 formats** | ✓ | ✓ | — | — |
| **Read support** | ✓ | ✓ | — | ✓ |
| **Cross-platform** | ✓ | net8+ | ✓ | ✓ |
| **Custom colours** | ✓ | Foreground only | — | — |
| **TryHarder fallback** | ✓ | — | — | — |
| **Multi-code in image** | — | ✓ | — | ✓ |
| **External dependency** | ZXing.Net | FreeSpire | QRCoder | None |

## Overview

1. **[Index](README.md)** — Overview, interfaces, models, and implementation notes
1. [Examples](examples.md) — QR code, barcode generation, scanning, and layer composition
