# Regira Consumer Project — Copilot Instructions

> **Source of truth:** `ai/AGENTS.md` is the authoritative routing guide for this project. This file is a self-contained subset of that guide optimised for GitHub Copilot. If `ai/AGENTS.md` is available in your context, it takes precedence over anything here.

This file covers the essential rules, templates, and package tables needed before generating code in a project that uses Regira packages.

---

## Pre-flight checklist

Run this before generating any code:

- [ ] `NuGet.Config` includes the Regira feed `https://packages.regira.com/v3/index.json` alongside `nuget.org`
- [ ] `dotnet restore` succeeded
- [ ] `dotnet build` succeeded (this extracts Regira AI guides from installed packages)
- [ ] `.github/instructions/regira/` was checked for extracted `*.instructions.md` files
- [ ] Every extracted primary guide relevant to the current task was read before writing application code

---

## Guide loading rules

Use the narrowest relevant guidance. Never load every guide up front.

1. For project scaffolding or app-shape changes → read `project.setup.md` in full
2. For shared setup concerns (NuGet feed, logging, OpenAPI) → read `shared.setup.md` in full
3. For module-specific work → read the matching `*.instructions.md` in full before writing code
4. For exact method signatures, namespaces, or examples → consult `*.signatures.md`, `*.namespaces.md`, `*.examples.md` by section on demand
5. **Never guess** a namespace, method name, or package name — look it up or ask

---

## Project template selection

Choose one template before creating any files. For an existing project, infer the nearest match and stay consistent.

| Requirement | Template |
|---|---|
| Script, batch job, or CLI utility | `ConsoleWithLogging` |
| Standard hosted API, no auth | `BasicApi` |
| Lightweight internal API, no auth | `SelfHostingApi` |
| Must be deployable as a Windows Service | `SelfHostingApi` |
| API protected by API key and/or JWT Bearer | `SelfHostingApiWithAuth` |
| Controller-based routing with enforced authorization | `SelfHostingApiWithAuth` |

Template consequences:
- `ConsoleWithLogging`: host-based console setup with configuration and structured logging
- `BasicApi`: ASP.NET Core Web API without authentication
- `SelfHostingApi`: self-hosted baseline, compatible with Windows Service deployment
- `SelfHostingApiWithAuth`: self-hosted with API key and/or JWT Bearer; keep endpoints protected by default

---

## Code generation workflow

1. Choose or confirm the `projectTemplate`
2. Choose the smallest Regira module set that covers the request
3. Ensure `NuGet.Config` includes the Regira feed; add matching packages
4. Run `dotnet restore` and `dotnet build` to extract embedded guide files
5. Check `.github/instructions/regira/` for extracted guides
6. Read all applicable primary guides in full before writing entity models, services, controllers, DI registrations, or infrastructure code
7. Generate code consistent with the template, installed packages, extracted guides, and local conventions

---

## Primary Regira package families

| Module | Use when | Main packages |
|---|---|---|
| Entities | CRUD APIs, entity services, DTO mapping, EF Core repositories | `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` |
| IO.Storage | File storage, uploads, Azure Blob, SFTP, ZIP | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` |
| Office.PDF | HTML to PDF, PDF operations, printing | `Regira.Office.PDF.SelectPdf`, `Regira.Office.PDF.Puppeteer` |
| Office.Excel | Excel read and write | `Regira.Office.Excel.ClosedXML`, `Regira.Office.Excel.MiniExcel` |
| Office.Word | Word document generation | `Regira.Office.Word.Spire` |
| Office.Mail | Email sending | `Regira.Mail.SendGrid`, `Regira.Mail.MailGun` |
| Office.CSV | CSV read and write | `Regira.Office.Csv.CsvHelper` |
| Office.Barcodes | Barcode or QR code generation | `Regira.Office.Barcodes.ZXing`, `Regira.Office.Barcodes.QRCoder` |
| Office.OCR | OCR text extraction | `Regira.Office.OCR.Tesseract` |
| Office.VCards | vCard contact files | `Regira.Office.VCards.FolkerKinzel` |
| Media | Image processing, resize, crop, FFmpeg | `Regira.Media`, `Regira.Drawing.SkiaSharp`, `Regira.Media.FFMpeg` |
| Security | Hashing, JWT, API key auth, cryptography | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` |
| Web | Razor rendering, middleware, OpenAPI helpers | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore` |
| System | Windows Service hosting, project tooling | `Regira.System`, `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL, Peppol | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

For modules with multiple provider packages (PDF, Excel, etc.), do not guess — ask the user to choose a provider when the request is ambiguous.

Web APIs use `app.MapOpenApi()` plus `app.MapScalarApiReference()` as the standard API surface. Do not add Swashbuckle unless explicitly requested.

---

## General engineering rules

- Use the latest stable .NET and C# features unless the project already constrains otherwise
- Add the Regira feed to `NuGet.Config` before restoring Regira packages
- Keep `Program.cs` thin; move service registration into `IServiceCollection` extension methods
- Prefer `Microsoft.Extensions.DependencyInjection`; depend on abstractions, not concrete implementations
- Use file-scoped namespaces
- Follow standard C# naming: descriptive but concise; prefer `TEntity`, `TKey`, `TDto` for generic parameters
- Default to SOLID principles; do not introduce abstractions the current task does not need
- Prefer the simplest solution that correctly solves the current problem
- Only validate at system boundaries (user input, external APIs); trust internal code and framework guarantees
- Ask rather than guess when a required API, namespace, or convention is not covered by the loaded guides
