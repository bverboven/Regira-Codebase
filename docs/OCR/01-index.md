# Regira OCR

Regira OCR provides optical character recognition via two underlying engines. Both implement the same `IOcrService` interface.

## Projects

| Project | Package | Engine | Languages |
|---------|---------|--------|-----------|
| `OCR.Tesseract` | `Regira.Office.OCR.Tesseract` | Tesseract 5 | English, Dutch (configurable) |
| `OCR.PaddleOCR` | `Regira.Office.OCR.PaddleOCR` | PaddleOCR (ONNX) | English, Chinese |

## Installation

```xml
<!-- Tesseract (configurable languages) -->
<PackageReference Include="Regira.Office.OCR.Tesseract" Version="5.*" />

<!-- PaddleOCR (multilingual, including Chinese) -->
<PackageReference Include="Regira.Office.OCR.PaddleOCR" Version="5.*" />
```

## IOcrService

```csharp
Task<string?> Read(IMemoryFile imgFile, string? lang = null);
```

Pass `lang` as an ISO 639-1 code (e.g. `"en"`, `"nl"`, `"zh"`). When `null`, the implementation's configured default is used.

## Tesseract

Requires language data files in the `tessdata` directory.

```csharp
var ocr = new Regira.Office.OCR.Tesseract.OcrManager(new OcrManager.Options
{
    Language      = "en",              // default language
    DataDirectory = "./tessdata"       // path to .traineddata files
});

string? text = await ocr.Read(imageFile);
string? nl   = await ocr.Read(imageFile, lang: "nl");
```

Download language packs from `github.com/tesseract-ocr/tessdata`.

## PaddleOCR

Uses local model files (bundled via `Sdcb.PaddleOCR.Models.LocalV3`). No external data directory required.

```csharp
var ocr = new Regira.Office.OCR.PaddleOCR.OcrManager();

string? text = await ocr.Read(imageFile);
string? zh   = await ocr.Read(imageFile, lang: "zh");
```

> PaddleOCR depends on **OpenCvSharp4.runtime.win** — Windows only.

## Notes

- Input is `IMemoryFile` — use `bytes.ToBinaryFile()` or any `IFileService.GetBytes()` result.
- Both implementations return `null` when no text is detected.
- Tesseract is cross-platform; PaddleOCR is Windows-only.
