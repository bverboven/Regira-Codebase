# Regira Codebase — Copilot Instructions

**Primary goal:** When a request relates to a specific module below, consult the corresponding instruction file for detailed, context-specific guidance. Fall back to the [General Instructions](#general-instructions) only when no module instruction file applies.

Don't assume syntax or signatures! 
When you have to guess and the instruction files don't give you the required information: 
- Stop and describe the problem!
- ASK feedback from the user!

After every significant task, analyze what went well and what failed. Update ./learnings.md with a 'Lessons Learned' section. Before starting any new task, read learnings.md to avoid repeating past mistakes.

---

## Module Instructions

This codebase provides specialized AI instruction sets for its modules. **When a user's request relates to a Regira module, load the corresponding instruction file and follow its guidance precisely.** Follow the module's instruction file as the primary source. Apply General Instructions only for topics not covered by the module guide, or when strictly necessary to fulfil the request.

### Available Modules

| Module | Namespace | Covers | Instructions |
|--------|-----------|--------|--------------| 
| **Project Templates** | *(scaffolding)* | Scaffolding new projects from reusable starter templates | `./project.setup.md` |
| **Entities** | `Regira.Entities` | CRUD Services with built-in extras | `./entities.instructions.md` |
| **TreeList** | `Regira.TreeList` | Hierarchical tree structures: build, navigate, and query nodes | `./treelist.instructions.md` |
| **IO.Storage** | `Regira.IO.Storage` | File storage: local filesystem, Azure Blob, SFTP/SSH, GitHub, TCP, compression | `./io.storage.instructions.md` |
| **Office** | `Regira.Office` | Document processing: Excel, Word, PDF, OCR, Barcodes, CSV, vCards | `./office.instructions.md` |
| **Media** | `Regira.Media` | Image & video processing: drawing, resize/crop/rotate, FFmpeg | `./media.instructions.md` |
| **Web** | `Regira.Web` | Web utilities: HTML generation (RazorLight), Swagger, mail providers | `./web.instructions.md` |
| **Security** | `Regira.Security` | Authentication, hashing (BCrypt), cryptography | `./security.instructions.md` |
| **System** | `Regira.System` | Hosting utilities, project/solution file tooling | `./system.instructions.md` |
| **Invoicing** | `Regira.Invoicing` | Invoice creation and integrations (Billit, UBL, ViaAdValvas) | `./invoicing.instructions.md` |
| **Payments** | `Regira.Payments` | Payment provider integrations (Mollie, Pom) | `./payments.instructions.md` |

When a module's instruction file is not yet available (or the topic isn't covered), apply general .NET best practices and the conventions in this file.

---

### Project Templates

**Instructions:** `./project.setup.md`

Reusable starter templates for bootstrapping new .NET projects. Covers four templates:

- **`ConsoleWithLogging`** — standalone console app with Serilog, DI, and configuration
- **`BasicApi`** — standard ASP.NET Core API (IIS / Azure / Docker) with controllers and Minimal API endpoints, Serilog, and Scalar UI
- **`SelfHostingMinimalApi`** — self-hosted Kestrel API with Minimal API endpoints, Swagger + Scalar, and optional Windows Service support
- **`SelfHostingApiWithAuth`** — self-hosted API with API key and JWT Bearer authentication, OpenAPI security transformers, and Swagger + Scalar

**Load these instructions when the user asks to** scaffold a new project, create a new console app or API, set up a Windows Service, or choose between hosting and authentication options.

---

### Regira Entities

**Instructions:** `./entities.instructions.md`

A generic, extensible framework for building ASP.NET Core APIs with standardized CRUD operations on top of EF Core. It covers the full stack from database to HTTP endpoint:

- **Entity Project setup** — scaffolding a new ASP.NET Core Web API with EF Core, NuGet config, `DbContext`, `Program.cs`, and DI wiring
- **Entity modeling** — POCO entity classes, marker interfaces (`IHasTimestamps`, `IArchivable`, `ISortable`, …), `SearchObject` for filtering, `SortBy` enum, `Includes` flags enum, and DTOs
- **CRUD service layer** — `IEntityService` / `EntityRepository` backed by EF Core; custom query filters, sorting, navigation-property includes, processors (post-fetch enrichment), preppers (pre-save preparation), and primers (EF Core `SaveChanges` interceptors)
- **DTO mapping** — Mapster or AutoMapper integration with inline or class-based `AfterMapper` for computed properties
- **API controllers** — `EntityControllerBase` variants that expose standardized List / Search / Details / Save / Delete endpoints
- **Text search & normalization** — `[Normalized]` attribute, `IEntityNormalizer`, and keyword helpers for diacritic-insensitive full-text search
- **File attachments** — `EntityAttachment` model, attachment controllers, and pluggable file storage backends (local filesystem, Azure Blob, SFTP)
- **Soft delete, audit trails, caching** — built-in and custom wrappers using `EntityWrappingServiceBase`
- **Troubleshooting** — diagnosing runtime issues in Entities-based projects

**Load these instructions when the user's request is about any of the above**, including when building a new data-driven API, adding or changing an entity in an existing project, or fixing issues in an Entities-based solution.

---

### Regira TreeList

**Instructions:** `./treelist.instructions.md`

A generic, lightweight library for building and querying **hierarchical tree structures** over any type `T`.
The main container is `TreeList<T>` (inherits `List<TreeNode<T>>`), which keeps all nodes in a flat list while maintaining full parent/child links.

Key capabilities:

- **Building trees** — four strategies: children-selector (`Fill` / `ToTreeList` with roots + child func), single-parent selector, multi-parent selector for many-to-many, and manual node-by-node construction (`AddValue` / `AddChild`)
- **Node navigation** — extension methods on a single `TreeNode<T>`: `GetRoot`, `GetAncestors`, `GetChildren`, `GetOffspring`, `GetBrothers`, `GetUncles`, `GetNephews`
- **Collection navigation** — extension methods on `IEnumerable<TreeNode<T>>`: `GetSelf`, `GetParents`, `GetAncestors`, `GetRoots`, `GetBottom`, `GetOffspring`, `WithOffspring`, `GetBrothers`, `GetUncles`
- **Ordering** — `OrderByHierarchy()` for depth-first traversal with optional sibling sort key
- **TreeView** — `ToTreeView()` produces a `ReadOnlyCollection<T>` of raw values in hierarchical order
- **ReverseTree** — inverts the hierarchy (leaves become roots)
- **Many-to-many** — the same value object can appear as multiple `TreeNode<T>` instances at different positions
- **Circular reference detection** — configurable via `TreeOptions` (`EnableAutoCheck`, `ThrowOnError`)

**Load these instructions when the user asks to** build or traverse a tree, use `TreeList<T>`, work with hierarchical data structures, or add tree navigation to a .NET project.

---

### Regira IO.Storage

**Instructions:** `./io.storage.instructions.md`

A unified abstraction for reading and writing files across interchangeable storage backends. All backends implement the same `IFileService` interface, so consuming code is backend-independent.

- **Backends** — `BinaryFileService` (local disk), `BinaryBlobService` (Azure Blob), `SftpService` (SSH/SFTP), `GitHubService` (read-only GitHub), `ZipFileService` (in-memory archives)
- **File addressing** — Identifier (relative, portable), Path (absolute), URI helpers
- **Search** — `FileSearchObject` for folder-scoped, extension-filtered, recursive listing
- **ZIP utilities** — `ZipBuilder`, `ZipUtility` for creating and extracting archives
- **Helpers** — `FileProcessor` (recursive batch processing), `FileNameHelper` (unique filenames), `ExportHelper` (copy between backends)

**Load these instructions when the user asks to** store, retrieve, list, or manage files; work with Azure Blob, SFTP, GitHub, or ZIP storage; swap storage backends; or use `IFileService`.

---

### Regira Office

**Instructions:** `./office.instructions.md` — routes to individual sub-module instruction files

A collection of document and communication libraries, all sharing the same abstractions from `Common.Office`. **When a request targets a specific Office sub-module, load that sub-module's instruction file directly.**

| Sub-Module | File |
|-----------|------|
| Barcodes — QR code and barcode generation/scanning | `./office.barcodes.instructions.md` |
| CSV — read/write CSV files (typed + dictionary) | `./office.csv.instructions.md` |
| Excel — read/write Excel workbooks | `./office.excel.instructions.md` |
| Mail — send email via SendGrid or Mailgun | `./office.mail.instructions.md` |
| OCR — extract text from images | `./office.ocr.instructions.md` |
| PDF — HTML→PDF, merge/split/extract, print | `./office.pdf.instructions.md` |
| VCards — read/write `.vcf` contact files | `./office.vcards.instructions.md` |
| Word — create/convert/merge Word documents | `./office.word.instructions.md` |

**Load these instructions when the user's request involves any of the above Office sub-modules.** Start with `office.instructions.md` if the sub-module is unclear, then load the specific sub-module file.

---

### Regira Media (Drawing)

**Instructions:** `./media.instructions.md`

A cross-platform image processing library with a single `IImageService` interface backed by SkiaSharp (recommended) or GDI+ (Windows-only). Covers the full image pipeline from parsing to composition.

- **Operations** — parse, resize, crop, rotate, flip, format conversion, pixel color access, transparency
- **Composition** — `ImageBuilder` fluent API for multi-layer canvas: image layers, colored canvas layers, text label layers
- **Models** — `IImageFile`, `ImageSize`, `Color` (hex), `ImageFormat`, `ImagePosition` (flags), `ImageLayerOptions`
- **Extensibility** — `IImageCreator<T>` for custom layer sources (e.g. QR codes, charts)
- **Backends** — `Drawing.SkiaSharp` (cross-platform, recommended), `Drawing.GDI` (Windows-only, EXIF auto-rotate)

**Load these instructions when the user asks to** resize, crop, rotate, convert, or compose images; use `IImageService`; build multi-layer canvases; or create watermarks, thumbnails, or text overlays.

---

### Regira Invoicing

**Instructions:** `./invoicing.instructions.md`

Electronic invoice creation, UBL/Peppol format conversion, and document transmission via an AP gateway. Three packages cover the full invoicing pipeline.

- **Billit** — create and send invoices via the Billit platform; registers `IInvoiceManager`, `IPartyManager`, `IPeppolManager`
- **UblSharp** — convert domain invoice models to UBL 2.1 XML (Peppol BIS Billing 3.0) via `IUblConverter`
- **ViaAdValvas** — transmit UBL documents via the AdValVas Peppol AP gateway using HMAC-signed requests

**Load these instructions when the user asks to** create or send electronic invoices, convert to UBL/Peppol format, transmit via an AP gateway, or integrate with Billit or AdValVas.

---

### Regira Payments

**Instructions:** `./payments.instructions.md`

A unified payment processing abstraction over Mollie and POM payment gateways. Both services implement a shared `IPaymentService` interface and are stateless singletons.

- **Mollie** — full CRUD, checkout URL generation on `Save`, and webhook handling via `WebHook()`
- **POM** — payment link creation (`CreatePaylinkApi`), status polling, HMAC webhook verification

**Load these instructions when the user asks to** process payments, integrate Mollie or POM, create payment links, or handle payment webhooks.

---

### Regira Security

**Instructions:** `./security.instructions.md`

Encryption, password hashing, JWT authentication, API key authentication, and pre-built auth controllers for ASP.NET Core.

- **Encryption** — `SymmetricEncrypter` (static key, fast) and `AesEncrypter` (random salt, recommended for stored secrets) via `IEncrypter`
- **Hashing** — `BCryptNet.Hasher` (recommended for passwords), `Hasher` (PBKDF2), `SimpleHasher` (non-password) via `IHasher`
- **JWT** — `ITokenHelper`, `AddJwtAuthentication()` DI extension, `ClaimsPrincipal` extension methods
- **API Key** — `IApiKeyOwnerService`, in-memory + config-based registration via `AddApiKeyAuthentication()`
- **Controllers** — `AccountControllerBase<T>`, `UserControllerBase<T>`, `PasswordControllerBase<T>` (pre-built auth endpoints)

**Load these instructions when the user asks to** encrypt or decrypt data, hash passwords, set up JWT authentication, implement API key auth, or add pre-built authentication controllers.

---

### Regira Web

**Instructions:** `./web.instructions.md`

Web utilities for ASP.NET Core: Razor-based HTML template rendering, middleware, Swagger configuration, and background task hosting.

- **HTML templates** — `IHtmlParser` with three engines: `HtmlTemplateParser` (`{{token}}` placeholders), `RazorEngineCore` (full Razor), `RazorLight` (Razor with caching)
- **Middleware** — global exception handling, request culture, central route prefix, `TextPlainInputFormatter`, controller file helpers
- **Swagger** — `AddJwtAuthentication()` and `AddApiKeyAuthentication()` extensions for Swagger UI; enum-as-string display
- **Hosting** — `WebHostOptions` (appsettings `"Hosting"` section), background task queues, Windows Service installer

**Load these instructions when the user asks to** render Razor or token-based HTML templates, configure Swagger security, add web middleware (exception handling, culture, route prefix), or set up background task queues.

---

### Regira System

**Instructions:** `./system.instructions.md`

Application hosting utilities and `.csproj` file tooling.

- **Hosting** — `WebHostOptions` (port, Swagger, CORS, HTTPS, route prefix via appsettings), background task queues, Windows Service installer scripts
- **Projects** — `ProjectParser` (parse/update `.csproj` XML), `ProjectService` (CRUD on disk), `ProjectManager` + `ProjectTree` (dependency tree built on `TreeList<Project>`)

**Load these instructions when the user asks to** configure host options, set up background task queues, install as a Windows Service, or parse and manipulate `.csproj` project files.

---

## General Instructions

Apply the following conventions when no module instruction file applies, or as a supplement when an instruction file does not cover the topic.

### Project Conventions

- **Target frameworks**: Use the latest .NET framework for new projects unless the user specifies otherwise.
- **Language version**: use the latest C# features (primary constructors, collection expressions, pattern matching, etc.)
- **File-scoped namespaces**: `namespace My.Something;` (not block form)
- **NuGet feed**: Regira packages are published at `https://packages.regira.com/v3/index.json` — add this source to `NuGet.Config` alongside the default nuget.org feed

### Naming

Follow standard C# naming conventions. Additionally:
- Names must be **descriptive** but concise — avoid cryptic abbreviations and overly long identifiers
- Generic type parameters: use a meaningful `T`-prefixed name (`TEntity`, `TKey`, `TDto`) rather than a bare single-letter `T` where context allows
- Variable and Property names can be generic, when the context is clear. 
  For example, in a `ProductService` class, a method parameter named `item` or `Items` is perfectly clear. This even enables to use of generic interfaces.
  However, when referring to a specific entity type, it's mandatory to use the entity name (e.g. `category`).
 
### Dependency Injection

- Use `Microsoft.Extensions.DependencyInjection`
- Register services via extension methods on `IServiceCollection`, grouped by feature
- Avoid `ServiceLocator` patterns unless a framework explicitly requires it

### Testing

- Choose the best-suited framework for the purpose and context
- Keep tests small and focused

### SOLID Principles

Always apply SOLID as a default design guide:

| Principle | Rule |
|-----------|------|
| **S** — Single Responsibility | A class or method does one thing only. Split when concerns diverge. |
| **O** — Open / Closed | Extend behavior through new classes or configuration; avoid modifying existing stable code. |
| **L** — Liskov Substitution | Subtypes must be usable wherever the base type is expected — no surprising behavior changes. |
| **I** — Interface Segregation | Prefer small, focused interfaces over large, catch-all ones. Only expose what callers need. |
| **D** — Dependency Inversion | Depend on abstractions (`IService`), not concrete implementations. Inject dependencies; don't `new` them up inside business logic. |

### Code Quality

**Keep it simple.** The simplest solution that correctly solves the problem is always preferred. Avoid over-engineering, premature abstraction, and unnecessary indirection.

- If a solution feels complex, it probably is — step back and look for the simpler path
- Solve the current problem; do not add flexibility or abstractions for imagined future requirements
- Use `null!` only when the value is guaranteed by DI/framework (e.g. `DbSet<T>` properties)
- Apply `[Required]`, `[MaxLength]`, `[Range]` on entity/DTO properties — these drive both validation and EF schema
