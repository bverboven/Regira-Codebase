# Regira Codebase — Copilot Instructions

**Primary goal:** When a request relates to a specific module below, consult the corresponding instruction file for detailed, context-specific guidance. Fall back to the [General Instructions](#general-instructions) only when no module instruction file applies.

> **Scope**: This file is for the **source repository** where the full source tree is available locally. Module instruction files live in each project's `src/<project>/ai/` subdirectory. For a downstream project that consumes Regira packages, keep a repo-root `regira.modules.json` plus an `AGENTS.md` bootstrap based on [`consumer.agents.stub.md`](./consumer.agents.stub.md), optionally mirror [`consumer.copilot.stub.md`](./consumer.copilot.stub.md) into `.github/copilot-instructions.md`, then run the repo-root sync described in [`tools/ai/README.md`](../tools/ai/README.md).

Don't assume syntax or signatures! 
When you have to guess and the instruction files don't give you the required information: 
- Stop and describe the problem!
- ASK feedback from the user!

After every significant task, analyze what went well and what failed. Update `./learnings.md` with a 'Lessons Learned' section. Before starting any new task, read `learnings.md` to avoid repeating past mistakes. Treat it as **curated memory, not a running log** — if it becomes noisy, summarize it before relying on it as context.

---

## Module Instructions

This codebase provides specialized AI instruction sets for its modules. **When a user's request relates to a Regira module, load the corresponding instruction file and follow its guidance precisely.** Follow the module's instruction file as the primary source. Apply General Instructions only for topics not covered by the module guide, or when strictly necessary to fulfil the request.

**Loading rules:**
1. **Never load the whole `ai/` folder.** Identify the module and load only its guide.
2. Load deep references (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) only when the task specifically needs them.
3. For shared setup concerns (NuGet feed, DI patterns), use [`shared.setup.md`](./shared.setup.md) instead of duplicating them in module guides.

Only modules with dedicated AI guides are listed below. Consumer manifests use the names defined in [`module-sources.json`](./module-sources.json); this table groups them by source-repo guide family. In particular, consumers can select `Office` for the family overview or narrower `Office.*` submodules for PDF, Excel, Mail, OCR, barcodes, CSV, and vCards.

### Available Modules

| Module | Namespace | Covers | Main packages | Instructions |
|--------|-----------|--------|---------------|--------------| 
| **Project Templates** | *(scaffolding)* | Scaffolding new projects from reusable starter templates | *(not a NuGet module)* | `./project.setup.md` |
| **Entities** | `Regira.Entities` | CRUD services with built-in extras | `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` | `../src/Common.Entities/ai/entities.instructions.md` |
| **TreeList** | `Regira.TreeList` | Hierarchical tree structures: build, navigate, and query nodes | `Regira.TreeList` | `../src/TreeList/ai/treelist.instructions.md` |
| **IO.Storage** | `Regira.IO.Storage` | File storage: local filesystem, Azure Blob, SFTP/SSH, GitHub, TCP, compression | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` | `../src/Common.IO.Storage/ai/io.storage.instructions.md` |
| **Office** | `Regira.Office` | Document processing: Excel, Word, PDF, OCR, Barcodes, CSV, vCards | `Regira.Office` | `../src/Common.Office/ai/office.instructions.md` |
| **Media** | `Regira.Media` | Image and video processing: drawing, resize, crop, rotate, FFmpeg | `Regira.Media`, `Regira.Drawing.SkiaSharp`, `Regira.Drawing.GDI`, `Regira.Media.FFMpeg` | `../src/Common.Media/ai/media.instructions.md` |
| **Web** | `Regira.Web` | Web utilities: HTML generation, middleware, Swagger, mail providers | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`, `Regira.Web.Swagger` | `../src/Common.Web/ai/web.instructions.md` |
| **Security** | `Regira.Security` | Authentication, hashing, and cryptography | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` | `../src/Common.Security/ai/security.instructions.md` |
| **System** | `Regira.System` | Hosting utilities and project or solution file tooling | `Regira.System`, `Regira.System.Hosting` | `../src/Common.System/ai/system.instructions.md` |
| **Invoicing** | `Regira.Invoicing` | Invoice creation and integrations (Billit, UBL, ViaAdValvas) | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` | `../src/Common.Invoicing/ai/invoicing.instructions.md` |
| **Payments** | `Regira.Payments` | Payment provider integrations (Mollie, Pom) | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` | `../src/Common.Payments/ai/payments.instructions.md` |

When a module's instruction file is not yet available (or the topic isn't covered), apply general .NET best practices and the conventions in this file.

## Consumer Package Selection

When helping a downstream project adopt Regira packages:

1. Ask the user what they are building if not already clear.
2. Match the requested capability to one or more sync-supported modules from the table above.
3. Add the Regira NuGet feed (`https://packages.regira.com/v3/index.json`) alongside `nuget.org` if it is not already configured.
4. Treat `projectTemplate` in `regira.modules.json` as AI-only metadata about the consumer app shape. Use it to steer setup advice, but do not expect the sync scripts to consume it directly.
5. Update `regira.modules.json` with the selected sync-supported modules and any needed deep references (`setup`, `examples`, `signatures`, `namespaces`).
6. Add the matching `Regira.*` packages from the Regira feed to the relevant consumer project(s).
7. Ensure the consumer repository has `AGENTS.md` based on [`consumer.agents.stub.md`](./consumer.agents.stub.md). If the project uses GitHub Copilot before the first sync, also mirror [`consumer.copilot.stub.md`](./consumer.copilot.stub.md) into `.github/copilot-instructions.md`.
8. Run the repo-root sync so `.github/instructions/regira/*.md` exists for the selected modules. Use `-Force` or `--force` when the cached remote snapshot may be stale or corrupted.
9. After the sync, load only the matching generated guides for the selected modules.
10. Refresh the downstream `AGENTS.md` by re-copying [`consumer.agents.stub.md`](./consumer.agents.stub.md) when you upgrade `aiVersion` or when Regira adds new module families that the static tables should know about.

### Additional Package Families

These package families are available on the Regira feed but do not currently have dedicated synced AI guides. Choose them from user needs, install the matching package, and rely on general project conventions plus package-specific code references.

| Package family | Use when | Main packages |
|----------------|----------|---------------|
| **Common** | Shared abstractions, utilities, normalizing helpers, base contracts | `Regira.Common` |
| **Caching** | Runtime caching on top of the common abstractions | `Regira.Caching.Runtime` |
| **DAL.EFcore** | EF Core extensions and repository utilities | `Regira.DAL.EFcore` |
| **DAL.MongoDB** | MongoDB connectivity and backup or restore workflows | `Regira.DAL.MongoDB` |
| **DAL.MySQL** | MySQL or MariaDB connectivity and backup workflows | `Regira.DAL.MySQL`, `Regira.DAL.MySQL.MySqlBackup` |
| **DAL.PostgreSQL** | PostgreSQL connectivity | `Regira.DAL.PostgreSQL` |
| **Globalization** | Phone number parsing and formatting | `Regira.Globalization.LibPhoneNumber` |
| **Serializing** | Newtonsoft.Json-based serialization | `Regira.Serializing.Newtonsoft` |

### Shared References

- [`shared.setup.md`](./shared.setup.md) — shared setup rules and the Regira NuGet feed
- [`consumer.agents.stub.md`](./consumer.agents.stub.md) — canonical downstream `AGENTS.md` bootstrap for module selection and package setup
- [`consumer.copilot.stub.md`](./consumer.copilot.stub.md) — optional GitHub Copilot bridge that points to `AGENTS.md` before the first sync
- [`consumer.bootstrap.template.md`](./consumer.bootstrap.template.md) — bootstrap template for consumer projects
- [`tools/ai/README.md`](../tools/ai/README.md) — sync script usage, output layout, and versioning for consumer projects

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
