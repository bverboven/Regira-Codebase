# Regira Consumer Bootstrap

Use this file as the authoritative downstream bootstrap for choosing the project template, selecting Regira packages, and generating code inside a consumer repository.

This file is also the top-level Regira routing guide inside the source repository. In a consumer repository, use this file plus any local `.github/instructions/regira/*.md` guides as the available sources of truth.

## Section index

Use this grouped index to jump directly to the section you need across all primary instruction files.

### Core bootstrap

- `ai/AGENTS.md`
  - **Pre-flight checklist** — Mandatory readiness checks. [Link](ai/AGENTS.md#pre-flight-checklist)
  - **Default workflow** — Standard execution flow. [Link](ai/AGENTS.md#default-workflow)
  - **Guide loading rules** — What to read and when. [Link](ai/AGENTS.md#guide-loading-rules)
  - **Project template selection** — Pick the right template. [Link](ai/AGENTS.md#project-template-selection)
  - **Setup baseline** — Default setup conventions. [Link](ai/AGENTS.md#setup-baseline)
  - **Code generation workflow** — Ordered generation steps. [Link](ai/AGENTS.md#code-generation-workflow)
  - **Installed package routing** — Map packages to modules. [Link](ai/AGENTS.md#installed-package-routing)
  - **Primary Regira package families** — Core module catalog. [Link](ai/AGENTS.md#primary-regira-package-families)
  - **Additional package families** — Extra package options. [Link](ai/AGENTS.md#additional-package-families)
  - **Optional local cache** — Local instruction sources. [Link](ai/AGENTS.md#optional-local-cache)
  - **General engineering rules** — Fallback coding conventions. [Link](ai/AGENTS.md#general-engineering-rules)

### Setup guides

- `src/Common.Setup/ai/project.setup.md`
  - **Template Selection Guide** — Template choice criteria. [Link](src/Common.Setup/ai/project.setup.md#template-selection-guide)
  - **Shared Conventions** — Cross-template defaults. [Link](src/Common.Setup/ai/project.setup.md#shared-conventions)
  - **Template 1 — `ConsoleWithLogging`** — Console template blueprint. [Link](src/Common.Setup/ai/project.setup.md#template-1-consolewithlogging)
  - **Template 2 — `BasicApi`** — Basic API blueprint. [Link](src/Common.Setup/ai/project.setup.md#template-2-basicapi)
  - **Template 3 — `SelfHostingApi`** — Self-hosting API blueprint. [Link](src/Common.Setup/ai/project.setup.md#template-3-selfhostingapi)
  - **Template 4 — `SelfHostingApiWithAuth`** — Auth API blueprint. [Link](src/Common.Setup/ai/project.setup.md#template-4-selfhostingapiwithauth)

- `src/Common.Setup/ai/shared.setup.md`
  - **NuGet Feed** — Regira feed setup. [Link](src/Common.Setup/ai/shared.setup.md#nuget-feed)
  - **Consumer Local Guides** — Using extracted local docs. [Link](src/Common.Setup/ai/shared.setup.md#consumer-local-guides)
  - **Related Files** — Connected reference files. [Link](src/Common.Setup/ai/shared.setup.md#related-files)
  - **Authoring Rules** — Guide authoring constraints. [Link](src/Common.Setup/ai/shared.setup.md#authoring-rules)

- `src/Common.Entities/ai/entities.setup.md`
  - **Defaults** — Default entities setup. [Link](src/Common.Entities/ai/entities.setup.md#defaults)
  - **Checklist** — Step-by-step setup checks. [Link](src/Common.Entities/ai/entities.setup.md#checklist)
  - **NuGet Feed** — Regira feed setup. [Link](src/Common.Entities/ai/entities.setup.md#nuget-feed)
  - **Packages** — Required package set. [Link](src/Common.Entities/ai/entities.setup.md#packages)
  - **Step 1: Project Files** — Create base project files. [Link](src/Common.Entities/ai/entities.setup.md#step-1-project-files)
  - **Step 2: Create DbContext** — Create EF Core context. [Link](src/Common.Entities/ai/entities.setup.md#step-2-create-dbcontext)
  - **Step 3: Program.cs** — Wire app startup. [Link](src/Common.Entities/ai/entities.setup.md#step-3-programcs)
  - **Step 4: Create the DI Extension Method** — Register services in DI. [Link](src/Common.Entities/ai/entities.setup.md#step-4-create-the-di-extension-method)

### Business and platform modules

- `src/Common.Entities/ai/entities.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Entities/ai/entities.instructions.md#projects)
  - **Quick Agent Playbook** — Fast decision workflow. [Link](src/Common.Entities/ai/entities.instructions.md#quick-agent-playbook)
  - **Namespace Reference** — Namespaces to import. [Link](src/Common.Entities/ai/entities.instructions.md#namespace-reference)
  - **Signatures Reference** — Method signature lookup. [Link](src/Common.Entities/ai/entities.instructions.md#signatures-reference)
  - **Core Understanding** — Core architecture model. [Link](src/Common.Entities/ai/entities.instructions.md#core-understanding)
  - **Decision-Making Guidelines** — How to choose patterns. [Link](src/Common.Entities/ai/entities.instructions.md#decision-making-guidelines)
  - **Project Creation Workflow** — New project sequence. [Link](src/Common.Entities/ai/entities.instructions.md#project-creation-workflow)
  - **Entity Implementation Workflow** — Entity build sequence. [Link](src/Common.Entities/ai/entities.instructions.md#entity-implementation-workflow)
  - **Custom Entity Services** — Custom service patterns. [Link](src/Common.Entities/ai/entities.instructions.md#custom-entity-services)
  - **Global Services** — Global service usage. [Link](src/Common.Entities/ai/entities.instructions.md#global-services)
  - **Normalizing** — Normalization guidance. [Link](src/Common.Entities/ai/entities.instructions.md#normalizing)
  - **Attachments** — File attachment patterns. [Link](src/Common.Entities/ai/entities.instructions.md#attachments)
  - **Error Handling** — Error handling rules. [Link](src/Common.Entities/ai/entities.instructions.md#error-handling)
  - **Built-in Query Extensions** — Query extension helpers. [Link](src/Common.Entities/ai/entities.instructions.md#built-in-query-extensions)
  - **Common Patterns** — Reusable implementation patterns. [Link](src/Common.Entities/ai/entities.instructions.md#common-patterns)
  - **Response Types** — Supported response models. [Link](src/Common.Entities/ai/entities.instructions.md#response-types)
  - **Best Practices** — Recommended defaults. [Link](src/Common.Entities/ai/entities.instructions.md#best-practices)
  - **Quick Reference: Built-in Entity Interfaces** — Entity interface cheat sheet. [Link](src/Common.Entities/ai/entities.instructions.md#quick-reference-built-in-entity-interfaces)
  - **Quick Reference: Built-in Services** — Service cheat sheet. [Link](src/Common.Entities/ai/entities.instructions.md#quick-reference-built-in-services)
  - **Troubleshooting** — Common issue resolution. [Link](src/Common.Entities/ai/entities.instructions.md#troubleshooting)
  - **See Also** — Related references. [Link](src/Common.Entities/ai/entities.instructions.md#see-also)

- `src/Common.IO.Storage/ai/io.storage.instructions.md`
  - **IO Abstractions (from `Regira.Common`)** — Shared file abstractions. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#io-abstractions-from-regiracommon)
  - **Installation** — Installation guidance. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#installation)
  - **Key Concept: Identifier vs. Path vs. URI** — Identifier and path semantics. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#key-concept-identifier-vs-path-vs-uri)
  - **IFileService** — File service contract. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#ifileservice)
  - **FileSearchObject** — File search filter model. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#filesearchobject)
  - **Implementations** — Storage backend implementations. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#implementations)
  - **ZIP / Compression** — Archive support. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#zip-compression)
  - **Helpers** — Utility helpers. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#helpers)
  - **DI Registration** — Dependency registration. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#di-registration)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.IO.Storage/ai/io.storage.instructions.md#backend-comparison)

- `src/Common.Invoicing/ai/invoicing.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#installation)
  - **Billit** — Billit provider integration. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#billit)
  - **UblSharp — UBL Conversion** — UBL conversion flow. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#ublsharp-ubl-conversion)
  - **ViaAdValvas — Peppol Transmission** — Peppol transmission flow. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#viaadvalvas-peppol-transmission)
  - **Typical End-to-End Flow** — Invoice lifecycle overview. [Link](src/Common.Invoicing/ai/invoicing.instructions.md#typical-end-to-end-flow)

- `src/Common.Media/ai/media.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Media/ai/media.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.Media/ai/media.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Media/ai/media.instructions.md#backend-comparison)
  - **Core Models** — Core data models. [Link](src/Common.Media/ai/media.instructions.md#core-models)
  - **IImageService — Image Operations** — Image service API. [Link](src/Common.Media/ai/media.instructions.md#iimageservice-image-operations)
  - **Layer Composition — `ImageBuilder`** — Image layer composition. [Link](src/Common.Media/ai/media.instructions.md#layer-composition-imagebuilder)
  - **Text Images** — Text rendering images. [Link](src/Common.Media/ai/media.instructions.md#text-images)
  - **Simple DI Registration** — Minimal DI setup. [Link](src/Common.Media/ai/media.instructions.md#simple-di-registration)
  - **Quick Example** — Minimal working example. [Link](src/Common.Media/ai/media.instructions.md#quick-example)

- `src/Common.Payments/ai/payments.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Payments/ai/payments.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.Payments/ai/payments.instructions.md#installation)
  - **Shared Abstraction** — Shared payments abstraction. [Link](src/Common.Payments/ai/payments.instructions.md#shared-abstraction)
  - **Mollie** — Mollie provider integration. [Link](src/Common.Payments/ai/payments.instructions.md#mollie)
  - **POM** — POM provider integration. [Link](src/Common.Payments/ai/payments.instructions.md#pom)
  - **Provider Comparison** — Provider capabilities. [Link](src/Common.Payments/ai/payments.instructions.md#provider-comparison)

- `src/Common.Security/ai/security.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Security/ai/security.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.Security/ai/security.instructions.md#installation)
  - **Encryption** — Encryption APIs. [Link](src/Common.Security/ai/security.instructions.md#encryption)
  - **Hashing** — Hashing APIs. [Link](src/Common.Security/ai/security.instructions.md#hashing)
  - **Hashing Decision Guide** — Hashing selection help. [Link](src/Common.Security/ai/security.instructions.md#hashing-decision-guide)
  - **JWT Authentication** — JWT auth setup. [Link](src/Common.Security/ai/security.instructions.md#jwt-authentication)
  - **API Key Authentication** — API key auth setup. [Link](src/Common.Security/ai/security.instructions.md#api-key-authentication)
  - **Pre-built Auth Controllers (`Security.Authentication.Web`)** — Ready-made auth endpoints. [Link](src/Common.Security/ai/security.instructions.md#pre-built-auth-controllers-securityauthenticationweb)

- `src/Common.System/ai/system.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.System/ai/system.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.System/ai/system.instructions.md#installation)
  - **System.Hosting** — Hosting utilities. [Link](src/Common.System/ai/system.instructions.md#systemhosting)
  - **System.Projects — `.csproj` Parsing** — Project parsing helpers. [Link](src/Common.System/ai/system.instructions.md#systemprojects-csproj-parsing)

- `src/Common.Web/ai/web.instructions.md`
  - **Projects** — Supported projects/packages. [Link](src/Common.Web/ai/web.instructions.md#projects)
  - **Installation** — Installation guidance. [Link](src/Common.Web/ai/web.instructions.md#installation)
  - **HTML Template Parsing** — HTML parsing services. [Link](src/Common.Web/ai/web.instructions.md#html-template-parsing)
  - **Template Engine Comparison** — Template engine trade-offs. [Link](src/Common.Web/ai/web.instructions.md#template-engine-comparison)
  - **Common.Web Middleware** — Built-in middleware. [Link](src/Common.Web/ai/web.instructions.md#commonweb-middleware)
  - **Web.Swagger** — Swagger helpers. [Link](src/Common.Web/ai/web.instructions.md#webswagger)
  - **System.Hosting — `WebHostOptions`** — Web host options. [Link](src/Common.Web/ai/web.instructions.md#systemhosting-webhostoptions)
  - **System.Hosting — Background Tasks** — Hosted background tasks. [Link](src/Common.Web/ai/web.instructions.md#systemhosting-background-tasks)

- `src/TreeList/ai/treelist.instructions.md`
  - **Installation** — Installation guidance. [Link](src/TreeList/ai/treelist.instructions.md#installation)
  - **Namespaces** — Namespace quick list. [Link](src/TreeList/ai/treelist.instructions.md#namespaces)
  - **Core Types** — Core tree types. [Link](src/TreeList/ai/treelist.instructions.md#core-types)
  - **Building a Tree** — Tree construction flow. [Link](src/TreeList/ai/treelist.instructions.md#building-a-tree)
  - **Single-Node Navigation Extensions** — Single-node traversal helpers. [Link](src/TreeList/ai/treelist.instructions.md#single-node-navigation-extensions)
  - **Collection Navigation Extensions** — Collection traversal helpers. [Link](src/TreeList/ai/treelist.instructions.md#collection-navigation-extensions)
  - **`ToTreeList` Extension Overloads** — Tree conversion overloads. [Link](src/TreeList/ai/treelist.instructions.md#totreelist-extension-overloads)
  - **`ToTreeView`** — Tree view projection. [Link](src/TreeList/ai/treelist.instructions.md#totreeview)
  - **`OrderByHierarchy`** — Hierarchy ordering. [Link](src/TreeList/ai/treelist.instructions.md#orderbyhierarchy)
  - **`ReverseTree`** — Reverse hierarchy transform. [Link](src/TreeList/ai/treelist.instructions.md#reversetree)
  - **Error Handling & Circular Reference Detection** — Safety and cycle handling. [Link](src/TreeList/ai/treelist.instructions.md#error-handling-circular-reference-detection)

### Office modules

- `src/Common.Office/ai/office.instructions.md`
  - **Sub-Modules** — Office module map. [Link](src/Common.Office/ai/office.instructions.md#sub-modules)
  - **When to Load Which File** — File-loading decision guide. [Link](src/Common.Office/ai/office.instructions.md#when-to-load-which-file)
  - **Related Modules** — Neighboring modules. [Link](src/Common.Office/ai/office.instructions.md#related-modules)

- `src/Common.Office/ai/office.pdf.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.pdf.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.pdf.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Office/ai/office.pdf.instructions.md#backend-comparison)
  - **Interfaces** — Interfaces guidance. [Link](src/Common.Office/ai/office.pdf.instructions.md#interfaces)
  - **Models** — Models guidance. [Link](src/Common.Office/ai/office.pdf.instructions.md#models)
  - **Usage** — Usage guidance. [Link](src/Common.Office/ai/office.pdf.instructions.md#usage)

- `src/Common.Office/ai/office.excel.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.excel.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.excel.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Office/ai/office.excel.instructions.md#backend-comparison)
  - **Interfaces** — Interfaces guidance. [Link](src/Common.Office/ai/office.excel.instructions.md#interfaces)
  - **Models** — Models guidance. [Link](src/Common.Office/ai/office.excel.instructions.md#models)
  - **Usage** — Usage guidance. [Link](src/Common.Office/ai/office.excel.instructions.md#usage)

- `src/Common.Office/ai/office.word.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.word.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.word.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Office/ai/office.word.instructions.md#backend-comparison)
  - **Interfaces** — Interfaces guidance. [Link](src/Common.Office/ai/office.word.instructions.md#interfaces)
  - **Models** — Models guidance. [Link](src/Common.Office/ai/office.word.instructions.md#models)
  - **Usage** — Usage guidance. [Link](src/Common.Office/ai/office.word.instructions.md#usage)

- `src/Common.Office/ai/office.mail.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.mail.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.mail.instructions.md#installation)
  - **`IMailer`** — Mailer contract. [Link](src/Common.Office/ai/office.mail.instructions.md#imailer)
  - **Core Models** — Core data models. [Link](src/Common.Office/ai/office.mail.instructions.md#core-models)
  - **Configuration** — Mailer configuration. [Link](src/Common.Office/ai/office.mail.instructions.md#configuration)
  - **DI Registration** — Dependency registration. [Link](src/Common.Office/ai/office.mail.instructions.md#di-registration)
  - **Exceptions** — Exception behavior. [Link](src/Common.Office/ai/office.mail.instructions.md#exceptions)
  - **Testing — `DummyMailer`** — Test mailer usage. [Link](src/Common.Office/ai/office.mail.instructions.md#testing-dummymailer)
  - **Web DTOs — `MailInput`** — Web DTO mapping. [Link](src/Common.Office/ai/office.mail.instructions.md#web-dtos-mailinput)
  - **ASP.NET Identity Integration** — Identity integration hooks. [Link](src/Common.Office/ai/office.mail.instructions.md#aspnet-identity-integration)

- `src/Common.Office/ai/office.csv.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.csv.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.csv.instructions.md#installation)
  - **`ICsvManager` / `ICsvManager<T>`** — CSV manager contracts. [Link](src/Common.Office/ai/office.csv.instructions.md#icsvmanager-icsvmanagert)
  - **`CsvOptions`** — CSV option model. [Link](src/Common.Office/ai/office.csv.instructions.md#csvoptions)
  - **Notes** — Important caveats. [Link](src/Common.Office/ai/office.csv.instructions.md#notes)
  - **Usage** — Usage guidance. [Link](src/Common.Office/ai/office.csv.instructions.md#usage)

- `src/Common.Office/ai/office.barcodes.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.barcodes.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.barcodes.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Office/ai/office.barcodes.instructions.md#backend-comparison)
  - **Interfaces** — Interfaces guidance. [Link](src/Common.Office/ai/office.barcodes.instructions.md#interfaces)
  - **Models** — Models guidance. [Link](src/Common.Office/ai/office.barcodes.instructions.md#models)
  - **Usage** — Usage guidance. [Link](src/Common.Office/ai/office.barcodes.instructions.md#usage)

- `src/Common.Office/ai/office.ocr.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.ocr.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.ocr.instructions.md#installation)
  - **Backend Comparison** — Provider comparison table. [Link](src/Common.Office/ai/office.ocr.instructions.md#backend-comparison)
  - **`IOcrService`** — OCR service contract. [Link](src/Common.Office/ai/office.ocr.instructions.md#iocrservice)
  - **Tesseract** — Tesseract backend notes. [Link](src/Common.Office/ai/office.ocr.instructions.md#tesseract)
  - **PaddleOCR** — PaddleOCR backend notes. [Link](src/Common.Office/ai/office.ocr.instructions.md#paddleocr)
  - **Notes** — Important caveats. [Link](src/Common.Office/ai/office.ocr.instructions.md#notes)

- `src/Common.Office/ai/office.vcards.instructions.md`
  - **Module Context** — Module scope and purpose. [Link](src/Common.Office/ai/office.vcards.instructions.md#module-context)
  - **Installation** — Installation guidance. [Link](src/Common.Office/ai/office.vcards.instructions.md#installation)
  - **`VCardManager`** — vCard manager API. [Link](src/Common.Office/ai/office.vcards.instructions.md#vcardmanager)
  - **Reading** — vCard read flow. [Link](src/Common.Office/ai/office.vcards.instructions.md#reading)
  - **Writing** — vCard write flow. [Link](src/Common.Office/ai/office.vcards.instructions.md#writing)
  - **`VCardVersion`** — Version selection rules. [Link](src/Common.Office/ai/office.vcards.instructions.md#vcardversion)
  - **Notes** — Important caveats. [Link](src/Common.Office/ai/office.vcards.instructions.md#notes)

___BEGIN___COMMAND_DONE_MARKER___0

## Pre-flight checklist

Run this checklist before any code generation:

- [ ] `NuGet.Config` includes the Regira feed `https://packages.regira.com/v3/index.json` alongside `nuget.org`
- [ ] `dotnet restore` succeeded when package changes or first-time setup required it
- [ ] `dotnet build` succeeded when installed Regira packages were expected to extract local AI guides
- [ ] `.github/instructions/regira/` was checked for extracted `*.instructions.md` files and relevant setup references in the consuming project directory (relative to the project that references the Regira packages, not assumed to be the solution root)
- [ ] If `.github/instructions/regira/project.setup.md` exists locally, it was read before generating project shape, hosting, logging, authentication, or OpenAPI/UI setup
- [ ] Every extracted primary guide relevant to the selected modules (`project.setup.md`, `shared.setup.md`, matching `*.instructions.md`) was read in full before writing application code in that area
- [ ] Deep references such as `*.setup.md`, `*.examples.md`, `*.signatures.md`, and `*.namespaces.md` were consulted on demand by section when the current task needed them

Only proceed to project scaffolding, infrastructure changes, or domain code once all applicable checks are satisfied.

## Default workflow

Ask the user what they're building if it isn't already clear, then follow the **Code generation workflow** below. For an existing project, inspect current `*.csproj` files before choosing packages or scaffolding. Prefer project-local instructions over shared Regira guidance when both exist, and ask for feedback rather than guessing missing APIs or conventions.

If the project contains `.github/instructions/regira/*.md`, treat the extracted shared setup guides plus the matching module guides as the primary local instructions. Regira packages that carry AI files embed them inside the NuGet package under `ai/`. During `dotnet build`, their bundled `.targets` files copy those files into `.github/instructions/regira/` under the consuming project directory (`$(ProjectDir)`), which may be a nested application folder rather than the solution root. Use `Regira.Setup` when the consumer needs local copies of `project.setup.md` and `shared.setup.md` through the same package-extraction workflow.

## Guide loading rules

Use the narrowest relevant guidance instead of loading broad instruction sets up front.

Primary guides (`project.setup.md`, `shared.setup.md`, matching `*.instructions.md`) should be read in full before generating code in that area. Deep references (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) should be consulted surgically by section when the current task needs them.

1. Never load the whole `ai/` folder when a narrower guide exists.
2. For project scaffolding or app-shape changes, load `project.setup.md`.
3. For shared setup concerns reused across modules, load `shared.setup.md`.
4. For module-specific work, load the matching `*.instructions.md` guide before writing code.
5. Load deep references such as `*.setup.md`, `*.examples.md`, `*.signatures.md`, and `*.namespaces.md` only when the current task needs them.
6. When details are missing, do not guess syntax or signatures. Stop, describe the gap, and ask the user for confirmation.

When working in the Regira source repository itself:

- `src/Common.Setup/ai/project.setup.md` owns reusable project-template and app-shape setup.
- `src/Common.Setup/ai/shared.setup.md` owns shared setup rules reused across modules.
- Module-specific guides live under the owning module's `src/*/ai/` folder.
- Read `ai/learnings.md` before starting substantial work and keep it curated when a task exposes a durable lesson.

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

Keep setup aligned with the selected `projectTemplate`. This file must remain enough for the normal one-file consumer flow even when no extracted local guides exist yet.

- Use the latest stable .NET framework and latest C# features unless the consumer project already targets something else.
- Add the Regira feed to `NuGet.Config` alongside `nuget.org` before restoring Regira packages.
- Keep `Program.cs` thin and move service registration or middleware setup into extension methods.
- Prefer `Microsoft.Extensions.DependencyInjection` and depend on abstractions instead of concrete implementations.
- Use file-scoped namespaces.
- For standard Web APIs, use `app.MapOpenApi()` plus `app.MapScalarApiReference()`. Do not add `Swashbuckle.AspNetCore` or call `UseSwaggerUI()` on the standard Regira API path.
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
6. **New project checkpoint**: When creating a project from scratch, stop here after completing steps 1–5. Summarize what was set up (template chosen, packages added, restore/build outcome) and explicitly ask the user whether to continue before writing any application code. This gives the user the opportunity to review the initial setup, switch to plan mode, or adjust the package selection before any code is generated.
7. Before writing any application code, check `.github/instructions/regira/` for extracted `*.instructions.md` guides, shared setup files, and relevant deep references.
8. If extracted guides exist, read the applicable primary guides in full before generating entity models, services, controllers, DI registrations, or infrastructure code. Use deep references by section when the current task needs exact examples, signatures, namespaces, or setup details. Skipping the relevant primary guides is a workflow violation.
9. If no extracted guides exist, verify the feed is reachable and the restore/build succeeded, then continue with the setup baseline, package mapping tables, and general engineering rules in this file.
10. Generate code that stays consistent with the selected `projectTemplate`, installed Regira packages, any extracted local guides, and local project conventions.

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
| `Regira.Setup` | `Setup` |
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
| Web | Razor rendering, middleware, optional Swagger/OpenAPI auth helpers | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`; `Regira.Web.Swagger` only for explicit Swagger/OpenAPI auth support |
| System | Windows Service hosting, project tooling | `Regira.System`, `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL, Peppol, AP gateway integrations | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

For Web APIs, the shared `project.setup.md` guide already standardizes OpenAPI plus Scalar as the default API surface. Swagger is deprecated, only use it when asked.

Use `Office` for the family overview or shared Office conventions. Add one or more concrete `Office.*` modules when the requested capability is already clear.

Modules with multiple provider packages, such as `Office.PDF` or `Office.Excel`, require a deliberate provider choice. Do not guess when the requested behavior is still ambiguous.

## Additional package families

These package families are available from the Regira feed but do not currently have dedicated AI guides. Choose them from user needs, install the matching package, and rely on general project conventions plus package-specific code references.

| Package family | Use when | Main packages |
|----------------|----------|---------------|
| Common | Shared abstractions, utilities, normalizing helpers, base contracts | `Regira.Common` |
| Caching | Runtime caching on top of the common abstractions | `Regira.Caching.Runtime` |
| DAL.EFcore | EF Core extensions and repository utilities | `Regira.DAL.EFcore` |
| DAL.MongoDB | MongoDB connectivity and backup or restore workflows | `Regira.DAL.MongoDB` |
| DAL.MySQL | MySQL or MariaDB connectivity and backup workflows | `Regira.DAL.MySQL`, `Regira.DAL.MySQL.MySqlBackup` |
| DAL.PostgreSQL | PostgreSQL connectivity | `Regira.DAL.PostgreSQL` |
| Globalization | Phone number parsing and formatting | `Regira.Globalization.LibPhoneNumber` |
| Setup | Shared project-template and setup-guide extraction for local AI guidance | `Regira.Setup` |
| Serializing | Newtonsoft.Json-based serialization | `Regira.Serializing.Newtonsoft` |

## Optional local cache

If the application repository already contains local Regira metadata files, use them as extra context:

- `.github/instructions/regira/*.md` can provide richer shared setup and module-specific guidance. Installed Regira packages that ship AI files can extract them there from their packaged `ai/` content on build via their package targets.
- `Regira.Setup` can be installed when the consumer needs `project.setup.md` and `shared.setup.md` extracted locally through the package-based guide flow.

These files are optional. `AGENTS.md` must remain enough for the normal consumer flow.

## General engineering rules

Apply these conventions when no narrower module guide exists, or as a supplement when the module guide does not cover the topic. Reuse the setup baseline above for framework, feed, namespace, and web-API defaults instead of re-stating them elsewhere.

### Project conventions

- Unless the project already constrains you, prefer the latest stable .NET and C# features that fit the local code style.

### Naming

- Follow normal C# naming conventions.
- Keep names descriptive but concise.
- Prefer meaningful generic type names such as `TEntity`, `TKey`, and `TDto` over bare single-letter names when context allows.
- Use generic names like `item` when the surrounding type already makes the meaning obvious.

### Dependency injection

- Prefer `Microsoft.Extensions.DependencyInjection` with feature-focused `IServiceCollection` extension methods.
- Avoid service-locator patterns unless a framework explicitly requires them.

### Testing

- Choose the smallest suitable test surface for the task.
- Keep tests focused and behavior-oriented.

### SOLID and simplicity

- Default to SOLID design principles, but do not introduce abstractions that the current task does not need.
- Prefer the simplest solution that correctly solves the current problem.
- Avoid speculative flexibility and premature indirection.
- Depend on abstractions instead of concrete implementations when defining business logic.
