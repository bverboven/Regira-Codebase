# Regira Codebase

Regira is a collection of .NET libraries providing unified abstractions for common application concerns. All packages follow the same pattern: a shared interface in a `Common.*` project, with one or more backend implementations as separate packages.

---

## Core

| Module | Description |
|--------|-------------|
| [Common](src/Common/README.md) | Shared foundation — IO abstractions, utilities, normalizing, caching, serializing, DAL contracts |
| [Common.Entities](src/Common.Entities/README.md) | Generic entity framework for CRUD, filtering, sorting, and EF Core integration |
| [DAL.MongoDB](src/DAL.MongoDB/README.md) | MongoDB connectivity and backup/restore |
| [DAL.MySQL](src/DAL.MySQL/README.md) | MySQL/MariaDB connectivity and backup/restore |
| [DAL.PostgreSQL](src/DAL.PostgreSQL/README.md) | PostgreSQL connectivity and backup/restore |
| [IO.Storage](src/Common.IO.Storage/README.md) | Unified file storage — local, Azure Blob, SFTP, GitHub, ZIP |

---

## Media

| Module | Description |
|--------|-------------|
| [Drawing / Images](src/Common.Media/README.md) | Image processing, format conversion, and layer composition |
| [Video](src/Common.Media/docs/video.md) | Video compression and snapshot extraction via FFMpeg |

---

## Office

| Module | Description |
|--------|-------------|
| [Office (overview)](src/Common.Office/README.md) | All Office submodule index |
| [Barcodes](src/Common.Office/docs/barcodes/README.md) | Barcode and QR code generation and scanning |
| [CSV](src/Common.Office/docs/csv/README.md) | CSV reading and writing |
| [Excel](src/Common.Office/docs/excel/README.md) | Excel workbook reading and writing |
| [Mail](src/Common.Office/docs/mail/README.md) | Email sending via SendGrid and Mailgun |
| [OCR](src/Common.Office/docs/ocr/README.md) | Optical character recognition |
| [PDF](src/Common.Office/docs/pdf/README.md) | HTML→PDF, PDF operations, and printing |
| [VCards](src/Common.Office/docs/vcards/README.md) | vCard contact file reading and writing |
| [Word](src/Common.Office/docs/word/README.md) | Word document creation, conversion, merge, and extraction |

---

## Infrastructure

| Module | Description |
|--------|-------------|
| [Security](src/Common.Security/README.md) | Encryption, hashing, JWT, and API Key authentication |
| [Serializing](src/Serializing.Newtonsoft/README.md) | JSON serialization via Newtonsoft.Json |
| [System](src/Common.System/README.md) | Process management, scheduling, and system utilities |

---

## Web

| Module | Description |
|--------|-------------|
| [Web / HTML](src/Common.Web/README.md) | Razor template rendering, middleware, Swagger, and background tasks |

---

## Utilities

| Module | Description |
|--------|-------------|
| [Globalization](src/Globalization.LibPhoneNumber/README.md) | Phone number parsing and formatting via libphonenumber |
| [Invoicing](src/Common.Invoicing/README.md) | Invoice models and structured number parsing |
| [Payments](src/Common.Payments/README.md) | Payment abstractions and structured reference numbers |
| [TreeList](src/TreeList/README.md) | Generic hierarchical tree structures with navigation extension methods |
