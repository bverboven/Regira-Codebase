# Regira Codebase

Regira is a collection of .NET libraries providing unified abstractions for common application concerns. All packages follow the same pattern: a shared interface in a `Common.*` project, with one or more backend implementations as separate packages.

---

## Core

| Module | Description |
|--------|-------------|
| [Common](src/Common) | Shared foundation — IO abstractions, utilities, normalizing, caching, serializing, DAL contracts |
| [Entities](src/Common.Entities) | Generic entity framework for CRUD, filtering, sorting, and EF Core integration |
| [IO.Storage](src/Common.IO.Storage) | Unified file storage — local, Azure Blob, SFTP, GitHub, ZIP |

---

## DAL

| Module | Description |
|--------|-------------|
| [EF Core](src/DAL.EFCore) | Entity Framework Core extensions and utilities |
| [MongoDB](src/DAL.MongoDB) | MongoDB connectivity and backup/restore |
| [MySQL](src/DAL.MySQL) | MySQL/MariaDB connectivity and backup/restore |
| [PostgreSQL](src/DAL.PostgreSQL) | PostgreSQL connectivity and backup/restore |

---

## Media

| Module | Description |
|--------|-------------|
| [Drawing](src/Common.Media) | Image processing, format conversion, and layer composition |
| [Video](src/Common.Media/docs/video.md) | Video compression and snapshot extraction via FFMpeg |

---

## Office

| Module | Description |
|--------|-------------|
| [Office (overview)](src/Common.Office) | All Office submodule index |
| [Barcodes](src/Common.Office/docs/barcodes) | Barcode and QR code generation and scanning |
| [CSV](src/Common.Office/docs/csv) | CSV reading and writing |
| [Excel](src/Common.Office/docs/excel) | Excel workbook reading and writing |
| [Mail](src/Common.Office/docs/mail) | Email sending via SendGrid and Mailgun |
| [OCR](src/Common.Office/docs/ocr) | Optical character recognition |
| [PDF](src/Common.Office/docs/pdf) | HTML→PDF, PDF operations, and printing |
| [VCards](src/Common.Office/docs/vcards) | vCard contact file reading and writing |
| [Word](src/Common.Office/docs/word) | Word document creation, conversion, merge, and extraction |

---

## Infrastructure

| Module | Description |
|--------|-------------|
| [Security](src/Common.Security) | Encryption, hashing, JWT, and API Key authentication |
| [Serializing](src/Serializing.Newtonsoft) | JSON serialization via Newtonsoft.Json |
| [System](src/Common.System) | Process management, scheduling, and system utilities |

---

## Web

| Module | Description |
|--------|-------------|
| [Web / HTML](src/Common.Web) | Razor template rendering, middleware, Swagger, and background tasks |

---

## Utilities

| Module | Description |
|--------|-------------|
| [Globalization](src/Globalization.LibPhoneNumber) | Phone number parsing and formatting via libphonenumber |
| [Invoicing](src/Common.Invoicing) | Invoice models and structured number parsing |
| [Payments](src/Common.Payments) | Payment abstractions and structured reference numbers |
| [TreeList](src/TreeList) | Generic hierarchical tree structures with navigation extension methods |

---

## Using Regira in your project

For a new project that consumes Regira packages, the default entrypoint is the `Regira.Setup` package. It installs a small local bootstrap for the consuming project's AI agent instead of copying the full source-repository `ai/` folder.

1. Add the Regira NuGet feed to your repository root:

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <clear />
       <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
       <add key="Regira" value="https://packages.regira.com/v3/index.json" />
     </packageSources>
   </configuration>
   ```

2. Add `Regira.Setup` to the consuming project and build once so it can extract the local setup files:

   ```powershell
   dotnet add <project>.csproj package Regira.Setup
   dotnet build
   ```

3. After the first build, ask the AI agent in the consuming project to read `ai/regira.setup.instructions.md`.
4. Let that local setup guide drive the next steps: selecting modules, creating `regira.modules.json`, running the local sync script, and loading only the generated `.github/instructions/regira/*.md` files that match the selected modules.
5. Re-run the sync whenever you add a module or change `aiVersion`.

`Regira.Setup` installs the consumer bootstrap, manifest template, and sync scripts into the consuming project on first build. See [src/Common.Setup/content/ai/regira.setup.instructions.md](src/Common.Setup/content/ai/regira.setup.instructions.md) for the authoritative consumer flow and [tools/ai/README.md](tools/ai/README.md) for the lower-level sync-script details and manual source-checkout workflow.
