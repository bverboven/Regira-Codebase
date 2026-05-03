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

For a new project that consumes Regira packages, prefer the interactive bootstrap from the repository root instead of copying each bootstrap file by hand.

```powershell
# Consumer repository — vendored script
pwsh tools/ai/sync-consumer-instructions.ps1 -Init

# Consumer repository — run directly from a local Regira-Codebase checkout
pwsh path\to\Regira-Codebase\tools\ai\sync-consumer-instructions.ps1 -Init
```

The init flow asks for `aiVersion`, `projectTemplate`, the initial Regira modules, and optional deep references per module. It ensures `NuGet.Config` contains the Regira feed, writes `regira.modules.json`, copies `AGENTS.md`, optionally vendors the PowerShell sync script into `tools/ai/` when you started from an external checkout, and then runs the normal sync to render `.github/copilot-instructions.md` and `.github/instructions/regira/*.md`.

After the initial bootstrap, re-run the sync whenever you add a module or change `aiVersion`:

```powershell
pwsh tools/ai/sync-consumer-instructions.ps1

# Refresh the cached remote snapshot if needed
pwsh tools/ai/sync-consumer-instructions.ps1 -Force
```

`projectTemplate` in the manifest remains AI-only metadata. The agent reads it to keep setup advice consistent, but the sync scripts do not consume it directly. When you upgrade `aiVersion` or when Regira adds new module families, re-copy [`ai/consumer.agents.stub.md`](ai/consumer.agents.stub.md) into `AGENTS.md` so the static module catalog stays current.

If you need explicit control over each bootstrap file, the manual fallback remains documented in [tools/ai/README.md](tools/ai/README.md). `AGENTS.md` stays the canonical consumer bootstrap for choosing Regira modules from user requirements. See [ai/consumer.agents.stub.md](ai/consumer.agents.stub.md) for the AGENTS bootstrap and [ai/consumer.copilot.stub.md](ai/consumer.copilot.stub.md) for the optional pre-sync Copilot bridge.
