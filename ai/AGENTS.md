# Regira Consumer Bootstrap

Use this file as the authoritative downstream bootstrap for choosing the project template, selecting Regira packages, and generating code inside a consumer repository.

[`regira.capabilities.md`](./regira.capabilities.md) in the Regira source repository is the canonical catalog of Regira capabilities and module families. That file is not present in consumer repositories. Do not attempt to fetch it or guess its contents from memory. In a consumer repository, use this file plus any local `.github/instructions/regira/*.md` guides as the available sources of truth.

## Pre-flight checklist

Run this checklist before any code generation:

- [ ] `NuGet.Config` includes the Regira feed `https://packages.regira.com/v3/index.json` alongside `nuget.org`
- [ ] `dotnet restore` succeeded when package changes or first-time setup required it
- [ ] `dotnet build` succeeded when installed Regira packages were expected to extract local AI guides
- [ ] `.github/instructions/regira/` was checked for extracted `*.instructions.md` files and relevant setup references
- [ ] Every extracted guide relevant to the selected modules was read in full before writing application code

Only proceed to project scaffolding, infrastructure changes, or domain code once all applicable checks are satisfied.

## Default workflow

1. Ask the user what they are building if not already clear.
2. Decide whether the user is creating a new project or extending an existing application.
3. Choose the `projectTemplate` first.
4. Select the relevant Regira modules from the tables below.
5. Ensure the Regira NuGet feed (`https://packages.regira.com/v3/index.json`) is configured alongside `nuget.org`.
6. Add the matching `Regira.*` packages from the Regira feed to the appropriate consumer project(s).
7. After adding Regira packages, run `dotnet restore` and `dotnet build` when needed so AI instruction files bundled inside the NuGet package `ai/` content can be extracted into `.github/instructions/regira/` in the consumer repository.
8. Stop and read guides before generating any application code. After restore/build, check whether `.github/instructions/regira/` contains matching `*.instructions.md` files, shared setup files, or relevant deep references. If it does, read every relevant guide in full before writing entity, service, controller, DI, or infrastructure code. If no files were extracted, verify the feed is reachable and the restore/build succeeded before continuing.
9. For an existing application, inspect the current `*.csproj` files and existing code before choosing more packages or scaffolding.
10. Generate or extend the code so it matches the selected `projectTemplate`, installed Regira packages, local package-provided guides, and local project conventions.
11. Prefer project-local instructions over shared Regira guidance when both exist.
12. Ask for feedback instead of guessing missing APIs or project-specific conventions.

If the project also contains `regira.modules.json`, use it as extra local context. If the project contains `.github/instructions/regira/*.md`, treat the extracted shared setup guides plus the matching module guides as the primary local instructions. Regira packages that carry AI files embed them inside the NuGet package under `ai/`. During `dotnet build`, their bundled `.targets` files copy those files into `.github/instructions/regira/` in the consumer repository.

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

## Setup baseline

Keep setup aligned with the selected `projectTemplate`. This file must remain enough for the normal one-file consumer flow even when no synced guides exist yet.

- Use the latest stable .NET framework and latest C# features unless the consumer project already targets something else.
- Add the Regira feed to `NuGet.Config` alongside `nuget.org` before restoring Regira packages.
- Keep `Program.cs` thin and move service registration or middleware setup into extension methods.
- Prefer `Microsoft.Extensions.DependencyInjection` and depend on abstractions instead of concrete implementations.
- Use file-scoped namespaces.
- Ask for feedback instead of guessing missing APIs, namespaces, signatures, or project-specific conventions.

Template consequences:

- `ConsoleWithLogging`: use a host-based console or CLI setup with configuration and structured logging.
- `BasicApi`: use an ASP.NET Core Web API baseline without authentication unless the user explicitly asks for auth.
- `SelfHostingApi`: use a self-hosted internal API baseline and keep it compatible with Windows Service deployment when requested.
- `SelfHostingApiWithAuth`: use self-hosted API scaffolding with API key and/or JWT Bearer authentication, and keep application endpoints protected by default.

## Code generation workflow

1. Choose or confirm the `projectTemplate`.
2. Choose the smallest Regira module set that covers the user's request.
3. Ensure the NuGet feed exists and add the matching packages.
4. Inspect existing `PackageReference` items when the installed Regira package set is part of the decision.
5. Run `dotnet restore` and `dotnet build` when needed so installed Regira packages can extract any embedded `ai/*.md` files from the NuGet package into `.github/instructions/regira/`.
6. Before writing any application code, check `.github/instructions/regira/` for extracted `*.instructions.md` guides, shared setup files, and relevant deep references.
7. If extracted guides exist, read every relevant guide in full before generating entity models, services, controllers, DI registrations, or infrastructure code. Skipping this step is a workflow violation.
8. If no extracted guides exist, verify the feed is reachable and the restore/build succeeded, then continue with the setup baseline and package mapping tables in this file.
9. Generate code that stays consistent with the selected `projectTemplate`, installed Regira packages, any extracted local guides, and local project conventions.

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
- `.github/instructions/regira/*.md` can provide richer shared setup and module-specific guidance. Installed Regira packages that ship AI files can extract them there from their packaged `ai/` content on build via their package targets.

These files are optional. `AGENTS.md` must remain enough for the normal consumer flow.
