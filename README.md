# Regira Codebase

Regira is a collection of .NET libraries providing unified abstractions for common application concerns. All packages follow the same pattern: a shared interface in a `Common.*` project, with one or more backend implementations as separate packages.

---

## Core

| Module | Description |
|--------|-------------|
| [Common](src/Common) | Shared foundation — IO abstractions, utilities, normalizing, caching, serializing, DAL contracts |
| [Common.Entities](src/Common.Entities) | Generic entity framework for CRUD, filtering, sorting, and EF Core integration |
| [DAL.MongoDB](src/DAL.MongoDB) | MongoDB connectivity and backup/restore |
| [DAL.MySQL](src/DAL.MySQL) | MySQL/MariaDB connectivity and backup/restore |
| [DAL.PostgreSQL](src/DAL.PostgreSQL) | PostgreSQL connectivity and backup/restore |
| [IO.Storage](src/Common.IO.Storage) | Unified file storage — local, Azure Blob, SFTP, GitHub, ZIP |

---

## Media

| Module | Description |
|--------|-------------|
| [Drawing / Images](src/Common.Media) | Image processing, format conversion, and layer composition |
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
