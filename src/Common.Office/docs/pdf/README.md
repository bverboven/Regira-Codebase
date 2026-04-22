# Regira Office.PDF

Regira Office.PDF provides a **unified abstraction** for PDF operations — HTML→PDF, images→PDF, PDF→images, text extraction, merge/split, and printing — across multiple underlying libraries.

## Projects

| Project | Package | Backend | HTML→PDF | PDF ops | Print |
|---------|---------|---------|----------|---------|-------|
| `Common.Office` | *(transitive)* | Shared abstractions | — | — | — |
| `PDF.SelectPdf` | `Regira.Office.PDF.SelectPdf` | Select.HtmlToPdf | ✓ full | — | — |
| `PDF.Puppeteer` | `Regira.Office.PDF.Puppeteer` | PuppeteerSharp | ✓ A4 | — | — |
| `PDF.Playwright` | `Regira.Office.PDF.MsPlaywright` | Microsoft.Playwright | ✓ A4 | — | — |
| `PDF.DocNET` | `Regira.Office.PDF.DocNET` | Docnet.Core | — | merge, split, img↔pdf, text | — |
| `PDF.Spire` | `Regira.Office.PDF.Spire` | FreeSpire.PDF | — | merge, split, img, text | ✓ |
| `PDF.PDFtoPrinter` | `Regira.Office.PDF.PDFtoPrinter` | PDFtoPrinter | — | — | ✓ (Win) |
| `PDF.PockyBum522` | `Regira.Office.PDF.PockyBum522` | SimpleFreePdfPrinter | — | — | ✓ (Win) |

## Installation

```xml
<!-- HTML→PDF (recommended — full options support) -->
<PackageReference Include="Regira.Office.PDF.SelectPdf" Version="5.*" />

<!-- HTML→PDF (headless Chromium) -->
<PackageReference Include="Regira.Office.PDF.Puppeteer" Version="5.*" />
<PackageReference Include="Regira.Office.PDF.MsPlaywright" Version="5.*" />

<!-- PDF operations (merge, split, text, images) -->
<PackageReference Include="Regira.Office.PDF.DocNET" Version="5.*" />
<PackageReference Include="Regira.Office.PDF.Spire" Version="5.*" />

<!-- Print (Windows) -->
<PackageReference Include="Regira.Office.PDF.PDFtoPrinter" Version="5.*" />
<PackageReference Include="Regira.Office.PDF.PockyBum522" Version="5.*" />
```

## Quick Start

```csharp
// HTML → PDF (SelectPdf)
IHtmlToPdfService pdf = new Regira.Office.PDF.SelectPdf.PdfManager();
IMemoryFile file = await pdf.Create(new HtmlInput
{
    HtmlContent = "<h1>Hello</h1>",
    Format      = PageSize.A4,
    Orientation = PageOrientation.Portrait
});

// Merge PDFs (DocNET)
IPdfMerger merger = new Regira.Office.PDF.DocNET.PdfManager(imageService);
IMemoryFile merged = merger.Merge([pdf1, pdf2, pdf3])!;
```

## Interfaces

### IHtmlToPdfService

```csharp
Task<IMemoryFile> Create(HtmlInput input);
```

### IPdfMerger / IPdfSplitter

```csharp
IMemoryFile?              Merge(IEnumerable<IMemoryFile> items);
IEnumerable<IMemoryFile>  Split(IMemoryFile pdf, IEnumerable<PdfSplitRange> ranges);
int                       GetPageCount(IMemoryFile pdf);
IMemoryFile?              RemovePages(IMemoryFile pdf, IEnumerable<int> pages);
```

### IPdfToImageService / IImagesToPdfService

```csharp
IEnumerable<IImageFile>  ToImages(IMemoryFile pdf, PdfToImagesOptions? options = null);
IMemoryFile?             ImagesToPdf(ImagesInput input);
```

### IPdfTextExtractor

```csharp
string          GetText(IMemoryFile pdf);
IList<string>   GetTextPerPage(IMemoryFile pdf);
IMemoryFile?    RemoveEmptyPages(IMemoryFile pdf);
```

### IPdfPrinter

```csharp
string              DefaultPrinter { get; }
IEnumerable<string> List();
void                Print(PdfPrinterInput input);
```

### IPdfService

Composite: `IPdfEditor + IPdfImageService + IPdfTextService`.

## Input / Output Models

### HtmlInput

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HtmlContent` | `string?` | `null` | HTML to convert |
| `HeaderHtmlContent` | `string?` | `null` | Repeating page header |
| `FooterHtmlContent` | `string?` | `null` | Repeating page footer |
| `HeaderHeight` | `int?` | `null` | Header height in mm |
| `FooterHeight` | `int?` | `null` | Footer height in mm |
| `Format` | `PageSize` | `A4` | Paper size |
| `Orientation` | `PageOrientation` | `Portrait` | Portrait / Landscape |
| `Margins` | `Margins` | `10mm` all | Page margins (in points) |
| `DPI` | `int` | `96` | Render resolution |

### ImagesInput

Same base properties as `HtmlInput` plus:

| Property | Type | Description |
|----------|------|-------------|
| `Images` | `ICollection<byte[]>` | One image per page |

### PdfToImagesOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | `ImageSize?` | `1080 × 1920` | Output image dimensions |
| `Format` | `ImageFormat` | `Jpeg` | Output image format |

### PdfSplitRange

| Property | Type | Description |
|----------|------|-------------|
| `Start` | `int` | First page (1-indexed) |
| `End` | `int?` | Last page (`null` = last page of document) |

### PdfPrinterInput

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PdfFile` | `IMemoryFile` | *(required)* | PDF to print |
| `PrinterName` | `string?` | default printer | Target printer |
| `PageSize` | `PageSize` | `A4` | Paper size |
| `PageOrientation` | `PageOrientation` | `Portrait` | Orientation |

## Implementation notes

### SelectPdf — recommended for HTML→PDF

Full support for all `HtmlInput` properties: page size, orientation, margins, headers, footers. Does not require a browser installation.

### Puppeteer / Playwright — headless Chromium

Both download Chromium automatically on first use (thread-safe via semaphore). Fixed to A4 — custom page sizes and margins are not respected. Use for pixel-perfect rendering of complex CSS.

### DocNET — recommended for PDF operations

Implements `IPdfService` (merge, split, images↔pdf, text extraction, page removal). Requires `IImageService` in the constructor.

```csharp
var pdf = new Regira.Office.PDF.DocNET.PdfManager(imageService);
```

### Spire — PDF operations + printing

Similar PDF operations to DocNET. Also ships `PdfPrinter` for Windows printing with page size override support.

## Overview

1. **[Index](README.md)** — Overview, interfaces, models, and implementation notes
1. [Examples](examples.md) — HTML→PDF, merge, split, text extraction, printing
