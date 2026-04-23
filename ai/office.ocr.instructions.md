# Regira Office.OCR AI Agent Instructions

You are an expert .NET developer working with the `Regira.Office.OCR` packages.
Your role is to help extract text from images using optical character recognition, using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<!-- Tesseract — configurable languages, cross-platform -->
<PackageReference Include="Regira.Office.OCR.Tesseract" Version="5.*" />

<!-- PaddleOCR — multilingual including Chinese, Windows-only -->
<PackageReference Include="Regira.Office.OCR.PaddleOCR" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Backend Comparison

| Package | Engine | Languages | Platform |
|---------|--------|-----------|----------|
| `Regira.Office.OCR.Tesseract` | Tesseract 5 | English, Dutch + configurable | Cross-platform |
| `Regira.Office.OCR.PaddleOCR` | PaddleOCR (ONNX) | English, Chinese | Windows only |

---

## `IOcrService`

```csharp
Task<string?> Read(IMemoryFile imgFile, string? lang = null);
```

Pass `lang` as an ISO 639-1 code (`"en"`, `"nl"`, `"zh"`). When `null`, the implementation's configured default is used.
Both implementations return `null` when no text is detected.

---

## Tesseract

Requires language data files (`tessdata` directory with `.traineddata` files).

```csharp
var ocr = new Regira.Office.OCR.Tesseract.OcrManager(new OcrManager.Options
{
    Language      = "en",
    DataDirectory = "./tessdata"
});

string? text = await ocr.Read(imageFile);
string? nl   = await ocr.Read(imageFile, lang: "nl");
```

Download language packs from `github.com/tesseract-ocr/tessdata`.

---

## PaddleOCR

Uses bundled local model files (`Sdcb.PaddleOCR.Models.LocalV3`). No external data directory required.

```csharp
var ocr = new Regira.Office.OCR.PaddleOCR.OcrManager();

string? text = await ocr.Read(imageFile);
string? zh   = await ocr.Read(imageFile, lang: "zh");
```

> Depends on `OpenCvSharp4.runtime.win` — **Windows only**.

---

## Notes

- Input is `IMemoryFile` — use `bytes.ToBinaryFile()` or any `IFileService.GetBytes()` result.
- Tesseract is the cross-platform choice; PaddleOCR is Windows-only but bundles models automatically.

---

**Load these instructions when** the user asks to extract text from images, scan documents with OCR, or choose between Tesseract and PaddleOCR.
