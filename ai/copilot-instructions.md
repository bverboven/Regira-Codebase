# Regira Codebase — Copilot Bootstrap

Use this file as a **router**, not as a full reference. Load the smallest relevant instruction set, then stop.

This bootstrap is for the **source repository** where the full `ai/` folder is available locally. For a downstream app that only consumes Regira packages, start from [`consumer.bootstrap.template.md`](./consumer.bootstrap.template.md) and follow [`consumer-project.migration-plan.md`](./consumer-project.migration-plan.md).

## Core Rules

1. **Never load the whole `ai/` folder.** Start with one module guide.
2. **Load the module guide before deep references.** Only open `*.setup.md`, `*.examples.md`, `*.signatures.md`, or `*.namespaces.md` when the task needs them.
3. **Do not guess syntax, namespaces, or signatures.** If the available files do not answer the question, stop and ask for feedback.
4. **Use [`shared.setup.md`](./shared.setup.md) for shared setup concerns** such as the Regira NuGet feed instead of repeating that setup in new module files.
5. **Treat [`learnings.md`](./learnings.md) as curated memory, not a running log.** Only rely on it when it stays short and maintained. If it becomes noisy, summarize it before using it as bootstrap context.

## Loading Order

1. Identify the module that best matches the request.
2. Load the module guide from the table below.
3. Load deep references for that module only when needed.
4. Fall back to the general instructions in this file only for gaps not covered by the module guide.

## Module Router

| Module | Load first | Use when |
|--------|------------|----------|
| Project Templates | `./project.setup.md` | Scaffolding a new console app, API, Windows Service, or starter project |
| Entities | `./entities.instructions.md` | Working with `Regira.Entities`, CRUD APIs, DTO mapping, or EF Core-backed entity services |
| TreeList | `./treelist.instructions.md` | Working with hierarchical data or `TreeList<T>` |
| IO.Storage | `./io.storage.instructions.md` | Working with `IFileService`, file storage backends, ZIP, Azure Blob, SFTP, or GitHub storage |
| Office | `./office.instructions.md` | Working with document-processing modules; route to the exact Office sub-module from there |
| Media | `./media.instructions.md` | Working with images, drawing, resize/crop/rotate, or `IImageService` |
| Web | `./web.instructions.md` | Working with HTML rendering, middleware, Swagger, or background task hosting |
| Security | `./security.instructions.md` | Working with encryption, hashing, JWT, or API key authentication |
| System | `./system.instructions.md` | Working with hosting utilities or `.csproj` tooling |
| Invoicing | `./invoicing.instructions.md` | Working with invoice creation, UBL/Peppol conversion, or AP gateway integrations |
| Payments | `./payments.instructions.md` | Working with payment providers, payment links, or payment webhooks |

## Shared References

- [`shared.setup.md`](./shared.setup.md) — shared setup rules and the Regira NuGet feed
- [`consumer.bootstrap.template.md`](./consumer.bootstrap.template.md) — tiny bootstrap template for consumer projects
- [`consumer-project.migration-plan.md`](./consumer-project.migration-plan.md) — phased plan for a scalable consumer-project model

## General Instructions

Apply these only when the selected module guide does not cover the topic.

### Project Conventions

- Use the latest .NET version for new projects unless the user specifies otherwise.
- Use modern C# features where they improve clarity.
- Use file-scoped namespaces.
- Keep package-specific setup in shared references rather than duplicating it across module guides.

### Naming

- Follow standard C# naming conventions.
- Prefer descriptive but concise identifiers.
- Use meaningful generic type names where context allows (`TEntity`, `TKey`, `TDto`).

### Dependency Injection

- Use `Microsoft.Extensions.DependencyInjection`.
- Register services with focused `IServiceCollection` extension methods.
- Avoid service locator patterns unless a framework explicitly requires them.

### Testing

- Choose the best-suited test framework for the context.
- Keep tests small and focused.

### Design Bias

- Prefer simple, direct solutions over flexible-but-unused abstractions.
- Solve the current problem; do not build speculative extension points.
- Use data annotations when they drive both validation and schema.
