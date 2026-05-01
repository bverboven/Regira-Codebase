# Regira Codebase

## Repository purpose

Regira is a modular collection of .NET libraries for common application concerns. The recurring pattern across the repository is:

- define reusable abstractions in a shared `Common.*` package
- provide one or more concrete provider/back-end implementations in separate projects
- keep consumer code backend-agnostic through interfaces, DTOs, and extension methods

Do **not** treat the `ai/` folder as the main source of truth for this repository. It exists for other projects that want to consume Regira.

## High-level architecture

### Solution structure

- `src/` contains the production libraries
- `tests/` contains xUnit test projects, generally grouped by module
- `tools/` contains supporting console utilities
- `evaluations/` contains review/evaluation notes

### Main module groups

- `Common` - shared foundation utilities, file abstractions, normalization, caching, serialization, and DAL contracts
- `Common.Entities` + related `Entities.*` projects - generic entity services, repositories, mapping, dependency injection, and web integration
- `DAL.*` - database-specific integrations for EF Core, MongoDB, MySQL, and PostgreSQL
- `Common.IO.Storage` + `IO.Storage.*` - interchangeable storage abstractions and providers
- `Common.Media` + drawing/video implementations - image and media processing
- `Common.Office` + provider packages - barcodes, CSV, Excel, mail, OCR, PDF, printing, vCards, and Word features
- `Common.Security` + `Security.*` - hashing, authentication, and web security
- `Common.Web`, `Web.*`, and `System.Hosting` - web utilities, Razor rendering, Swagger, middleware, and hosting helpers
- smaller focused packages such as globalization, invoicing, payments, serializing, and tree structures follow the same pattern

### Design conventions

- shared contracts usually live in `Abstractions/`
- DTOs, config, and value objects usually live in `Models/`
- helper and utility code usually lives in `Extensions/`, `Utilities/`, or `Helpers/`
- provider-specific projects depend on the shared contract packages instead of redefining interfaces
- README files in module roots are the best starting point for package-specific architecture details

## Platform and tooling

- the repository is a .NET solution centered on `Regira-Codebase.slnx`
- projects commonly multi-target `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`
- projects commonly enable:
  - nullable reference types
  - implicit usings
  - `LangVersion=latest`
- tests use xUnit

Typical validation command:

```bash
dotnet test Regira-Codebase.slnx /p:GeneratePackageOnBuild=false --nologo
```

## Coding style

### C# style

- prefer small, focused types and keep responsibilities narrow
- follow the existing namespace-to-folder structure
- use file-scoped namespaces
- use modern C# features already present in the repo, including primary constructors where they improve clarity
- keep public API names descriptive and use standard .NET naming:
  - `PascalCase` for types, members, enums, and properties
  - `camelCase` for locals and parameters
- keep abstractions generic and reusable before introducing provider-specific behavior
- prefer extension methods and helper classes for cross-cutting utilities instead of duplicating logic
- keep backend implementations interchangeable with their shared interfaces

### Documentation style

- keep documentation concise and practical
- prefer short sections, bullets, and tables over long prose
- place package-specific docs close to the relevant module
- describe architecture and intended usage, not just implementation trivia

## Change guidance

- make minimal, localized changes
- avoid changing unrelated modules in this large multi-package solution
- when adding functionality, first identify whether it belongs in a shared `Common.*` contract package or in a provider-specific implementation package
- when documenting or exploring a feature, start from the root `README.md`, then the relevant module `README.md`
