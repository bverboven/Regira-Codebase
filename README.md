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

Consumer projects receive AI instruction files tailored to the Regira modules they actually use, rather than the full source repository `ai/` folder.

1. Copy [`ai/regira.modules.template.json`](ai/regira.modules.template.json) to your project root as `regira.modules.json` and edit it to list your active modules.
2. Run the sync script to generate the bootstrap, list the exact Regira guide paths, and copy the selected instruction files:

   ```powershell
   # PowerShell (Windows, macOS, Linux)
   pwsh tools/ai/sync-consumer-instructions.ps1

   # Bash (requires bash 4.0+; see note for macOS)
   ./tools/ai/sync-consumer-instructions.sh
   ```

3. Re-run the script whenever you add a module or change `aiVersion`.

See [tools/ai/README.md](tools/ai/README.md) for full usage, output layout, and versioning details.
