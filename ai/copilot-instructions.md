# Regira Codebase — Copilot Instructions

**Primary goal:** match the user's request to one of the module orchestrators below and load that file. The orchestrator provides precise, context-specific instructions. Fall back to the [General Instructions](#general-instructions) only when no orchestrator applies.

---

## Module Orchestrators

This codebase provides specialized AI instruction sets for its modules. **When a user's request relates to a Regira module, load the corresponding orchestrator and follow its instructions precisely.** Deviate from the orchestrator's instructions only when strictly necessary to fulfil the request — and only as little as needed.

### Available Modules

| Module | Namespace | Covers | Orchestrator |
|--------|-----------|--------|--------------|
| **Entities** | `Regira.Entities` | CRUD APIs, EF Core, controllers, mapping, normalizing, attachments | `./Entities/orchestrator.instructions.md` |
| **IO.Storage** | `Regira.IO.Storage` | File storage: local filesystem, Azure Blob, SFTP/SSH, GitHub, TCP, compression | *(not yet available)* |
| **Office** | `Regira.Office` | Document processing: Excel, Word, PDF, OCR, Barcodes, CSV, vCards | *(not yet available)* |
| **Media** | `Regira.Media` | Image & video processing: drawing, resize/crop/rotate, FFmpeg | *(not yet available)* |
| **Invoicing** | `Regira.Invoicing` | Invoice creation and integrations (Billit, UBL, ViaAdValvas) | *(not yet available)* |
| **Payments** | `Regira.Payments` | Payment provider integrations (Mollie, Pom) | *(not yet available)* |
| **Security** | `Regira.Security` | Authentication, hashing (BCrypt), cryptography | *(not yet available)* |
| **Web** | `Regira.Web` | Web utilities: HTML generation (RazorLight), Swagger, mail providers | *(not yet available)* |
| **System** | `Regira.System` | Hosting utilities, project/solution file tooling | *(not yet available)* |

When a module's orchestrator is not yet available, apply general .NET best practices and the conventions in this file.

---

### Regira Entities

**Orchestrator:** `./Entities/orchestrator.instructions.md`

A generic, extensible framework for building ASP.NET Core APIs with standardized CRUD operations on top of EF Core. It covers the full stack from database to HTTP endpoint:

- **Project setup** — scaffolding a new ASP.NET Core Web API with EF Core, NuGet config, `DbContext`, `Program.cs`, and DI wiring
- **Entity modeling** — POCO entity classes, marker interfaces (`IHasTimestamps`, `IArchivable`, `ISortable`, …), `SearchObject` for filtering, `SortBy` enum, `Includes` flags enum, and DTOs
- **CRUD service layer** — `IEntityService` / `EntityRepository` backed by EF Core; custom query filters, sorting, navigation-property includes, processors (post-fetch enrichment), preppers (pre-save preparation), and primers (EF Core `SaveChanges` interceptors)
- **DTO mapping** — Mapster or AutoMapper integration with inline or class-based `AfterMapper` for computed properties
- **API controllers** — `EntityControllerBase` variants that expose standardized List / Search / Details / Save / Delete endpoints
- **Text search & normalization** — `[Normalized]` attribute, `IEntityNormalizer`, and keyword helpers for diacritic-insensitive full-text search
- **File attachments** — `EntityAttachment` model, attachment controllers, and pluggable file storage backends (local filesystem, Azure Blob, SFTP)
- **Soft delete, audit trails, caching** — built-in and custom wrappers using `EntityWrappingServiceBase`
- **Troubleshooting** — diagnosing runtime issues in Entities-based projects

**Load this orchestrator when the user's request is about any of the above**, including when building a new data-driven API, adding or changing an entity in an existing project, or fixing issues in an Entities-based solution.

---

## General Instructions

Apply the following conventions when no module orchestrator applies, or as a supplement when an orchestrator does not cover the topic.

### Project Conventions

- **Target frameworks**: Use the latest .NET version for new projects unless the user specifies otherwise.
- **Language version**: use the latest C# features (primary constructors, collection expressions, pattern matching, etc.)
- **File-scoped namespaces**: `namespace My.Something;` (not block form)
- **NuGet feed**: Regira packages are published at `https://packages.regira.com/v3/index.json` — add this source to `NuGet.Config` alongside the default nuget.org feed

### Naming

Follow standard C# naming conventions. Additionally:
- Names must be **descriptive** but concise — avoid cryptic abbreviations and overly long identifiers
- Generic type parameters: use a meaningful `T`-prefixed name (`TEntity`, `TKey`, `TDto`) rather than a bare single-letter `T` where context allows

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