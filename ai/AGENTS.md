# Regira Codebase — Copilot Instructions

**Primary goal:** When a request relates to a specific module below, consult the corresponding instruction file for detailed, context-specific guidance. Fall back to the [General Instructions](#general-instructions) only when no module instruction file applies.

Do not assume syntax or signatures!
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
  For example, in a `ProductService` class, a method parameter named `item` or `Items` is perfectly clear. This even enables the use of generic interfaces.
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
