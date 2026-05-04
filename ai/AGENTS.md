# Regira Consumer Bootstrap

Copy this file into `.github/AGENTS.md` in the application repository for a project that consumes Regira packages.

This file is the consumer entrypoint. After copying it, ask the agent what to build or what Regira feature to add. The agent must choose the project template, choose the Regira NuGet packages, add them to the project, and write the code.

[`regira.capabilities.md`](./regira.capabilities.md) in the Regira repository is the canonical catalog of Regira capabilities and module families. This file is the consumer-facing bootstrap that turns that catalog into concrete package selection and code-generation behavior inside an application repository.

## Default workflow

1. Ask the user what they are building if not already clear.
2. Decide whether the user is creating a new project or extending an existing application.
3. Choose the `projectTemplate` first.
4. Select the relevant Regira modules from the tables below.
5. Ensure the Regira NuGet feed (`https://packages.regira.com/v3/index.json`) is configured alongside `nuget.org`.
6. Add the matching `Regira.*` packages from the Regira feed to the appropriate consumer project(s).
7. After adding Regira packages, run `dotnet build` when needed so AI instruction files bundled inside the NuGet package `ai/` content can be extracted into `.github/instructions/regira/` in the consumer repository.
8. For an existing application, inspect the current `*.csproj` files and existing code before choosing more packages or scaffolding.
9. Generate or extend the code so it matches the selected `projectTemplate`, installed Regira packages, local package-provided guides, and local project conventions.
10. Prefer project-local instructions over shared Regira guidance when both exist.
11. Ask for feedback instead of guessing missing APIs or project-specific conventions.

If the project also contains `regira.modules.json`, use it as extra local context. If the project contains `.github/instructions/regira/*.md`, treat those guides as the primary package-specific instructions for the matching installed Regira packages. Regira packages that carry AI files embed them inside the NuGet package under `ai/`. During `dotnet build`, their bundled `.targets` files copy those files into `.github/instructions/regira/` in the consumer repository.

## Project template selection

Choose `projectTemplate` before selecting modules when the user is creating a new project or requesting major scaffolding changes.

| Requirement | `projectTemplate` |
|-------------|-------------------|
| Script, batch job, or CLI utility | `ConsoleWithLogging` |
| Standard hosted API, Minimal API and Controllers, no auth | `BasicApi` |
| Lightweight self-hosted internal API, no auth | `SelfHostingApi` |
| Must be deployable as a Windows Service | `SelfHostingApi` |
| API protected by API key and/or JWT Bearer | `SelfHostingApiWithAuth` |
| Controller-based routing with enforced authorization | `SelfHostingApiWithAuth` |
| Minimal API endpoints with authentication | `SelfHostingApiWithAuth` |

For a new project, choose the template before creating files. For an existing project, infer the nearest matching template from the current app structure and stay consistent with it.

## Code generation workflow

1. Choose or confirm the `projectTemplate`.
2. Choose the smallest Regira module set that covers the user's request.
3. Ensure the NuGet feed exists and add the matching packages.
4. Inspect existing `PackageReference` items when the installed Regira package set is part of the decision.
5. Run `dotnet build` when needed so installed Regira packages can extract any embedded `ai/*.md` files from the NuGet package into `.github/instructions/regira/`.
6. If local `.github/instructions/regira/*.instructions.md` guides exist, load the matching guide first.
7. Otherwise use the shared rules and package mapping tables in this file.
8. Generate code that stays consistent with the selected `projectTemplate`, installed Regira packages, any extracted local guides, and local project conventions.

## Shared code-generation rules

- Use the latest stable .NET framework and latest C# features unless the consumer project already targets something else.
- Keep `Program.cs` thin and move DI registration to extension methods.
- Depend on abstractions rather than concrete implementations.
- Prefer `Microsoft.Extensions.DependencyInjection` for registration.
- Use file-scoped namespaces.
- Keep names descriptive and prefer meaningful generic type names such as `TEntity`, `TKey`, and `TDto`.
- Keep the solution simple. Avoid speculative abstractions.
- Do not guess missing APIs, namespaces, or signatures. If exact package APIs are unclear from the local code, ask the user for confirmation.
- When multiple provider packages exist, choose deliberately instead of guessing from the family name alone.

## Installed package routing

When the consumer project already contains Regira packages, inspect the project's `PackageReference` items and map them back to the nearest module family before generating code.

| Installed package pattern | Treat as module |
|---------------------------|-----------------|
| `Regira.Entities*` | `Entities` |
| `Regira.IO.Storage*` | `IO.Storage` |
| `Regira.Office.PDF*` | `Office.PDF` |
| `Regira.Office.Excel*` | `Office.Excel` |
| `Regira.Office.Word*` | `Office.Word` |
| `Regira.Mail.*` | `Office.Mail` |
| `Regira.Office.Csv*` | `Office.CSV` |
| `Regira.Office.Barcodes*` | `Office.Barcodes` |
| `Regira.Office.OCR*` | `Office.OCR` |
| `Regira.Office.VCards*` | `Office.VCards` |
| `Regira.Media*`, `Regira.Drawing.*` | `Media` |
| `Regira.Security*` | `Security` |
| `Regira.Web*` | `Web` |
| `Regira.System*` | `System` |
| `Regira.Invoicing*` | `Invoicing` |
| `Regira.Payments*` | `Payments` |
| `Regira.TreeList` | `TreeList` |

## Primary Regira package families

| Module | Use when | Main packages |
|--------|----------|---------------|
| Entities | CRUD APIs, entity services, DTO mapping, EF Core repositories | `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` |
| IO.Storage | File storage, uploads, Azure Blob, SFTP, ZIP | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` |
| Office | Office family overview or when the user still needs to choose between PDF, Excel, Word, Mail, OCR, and related submodules | `Regira.Office` |
| Office.PDF | HTML to PDF, PDF operations, printing | `Regira.Office.PDF.SelectPdf`, `Regira.Office.PDF.Puppeteer` |
| Office.Excel | Excel read and write | `Regira.Office.Excel.ClosedXML`, `Regira.Office.Excel.MiniExcel` |
| Office.Word | Word document generation | `Regira.Office.Word.Spire` |
| Office.Mail | Email sending | `Regira.Mail.SendGrid`, `Regira.Mail.MailGun` |
| Office.CSV | CSV read and write | `Regira.Office.Csv.CsvHelper` |
| Office.Barcodes | Barcode or QR code generation | `Regira.Office.Barcodes.ZXing`, `Regira.Office.Barcodes.QRCoder` |
| Office.OCR | OCR text extraction | `Regira.Office.OCR.Tesseract` |
| Office.VCards | vCard contact files | `Regira.Office.VCards.FolkerKinzel` |
| Media | Image processing, resize, crop, rotate, FFmpeg workflows | `Regira.Media`, `Regira.Drawing.SkiaSharp`, `Regira.Drawing.GDI`, `Regira.Media.FFMpeg` |
| Security | Hashing, JWT, API key authentication, cryptography | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` |
| Web | Razor rendering, middleware, Swagger | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`, `Regira.Web.Swagger` |
| System | Windows Service hosting, project tooling | `Regira.System`, `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL, Peppol, AP gateway integrations | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

Use `Office` for the family overview or shared Office conventions. Add one or more concrete `Office.*` modules when the requested capability is already clear.

Modules with multiple provider packages, such as `Office.PDF` or `Office.Excel`, require a deliberate provider choice. Do not guess when the requested behavior is still ambiguous.

## Additional package families

These package families are available from the Regira feed but do not currently have dedicated synced AI guides. Choose them from user needs, install the matching package, and rely on general project conventions plus package-specific code references.

| Package family | Use when | Main packages |
|----------------|----------|---------------|
| Common | Shared abstractions, utilities, normalizing helpers, base contracts | `Regira.Common` |
| Caching | Runtime caching on top of the common abstractions | `Regira.Caching.Runtime` |
| DAL.EFcore | EF Core extensions and repository utilities | `Regira.DAL.EFcore` |
| DAL.MongoDB | MongoDB connectivity and backup or restore workflows | `Regira.DAL.MongoDB` |
| DAL.MySQL | MySQL or MariaDB connectivity and backup workflows | `Regira.DAL.MySQL`, `Regira.DAL.MySQL.MySqlBackup` |
| DAL.PostgreSQL | PostgreSQL connectivity | `Regira.DAL.PostgreSQL` |
| Globalization | Phone number parsing and formatting | `Regira.Globalization.LibPhoneNumber` |
| Serializing | Newtonsoft.Json-based serialization | `Regira.Serializing.Newtonsoft` |

## Optional local cache

If the application repository already contains local Regira metadata files, use them as extra context:

- `regira.modules.json` can record the chosen template and selected module set.
- `.github/instructions/regira/*.md` can provide richer module-specific guidance. Installed Regira packages that ship AI files can extract them there from their packaged `ai/` content on build via their package targets.

These files are optional. `AGENTS.md` must remain enough for the normal consumer flow.
