# Regira Consumer Bootstrap

Use this file as `AGENTS.md` at the repository root of a project that consumes Regira packages.

## Your role

1. Ask the user what they are building if not already clear.
2. Select the relevant Regira modules from the tables below.
3. Ensure the Regira NuGet feed (`https://packages.regira.com/v3/index.json`) is configured alongside `nuget.org`.
4. Read `projectTemplate` from `regira.modules.json` to keep project-setup advice consistent with the template the project was initialized with. Treat it as AI-only metadata; the sync script does not consume it directly.
5. Update `regira.modules.json` so it matches the selected sync-supported modules and any required deep references (`setup`, `examples`, `signatures`, `namespaces`).
6. Add the matching `Regira.*` packages from the Regira feed to the appropriate consumer project(s).
7. If `.github/instructions/regira/` is missing or stale relative to `regira.modules.json`, run or ask the user to run `pwsh tools/ai/sync-consumer-instructions.ps1` from the repository root. Use `-Force` if the cached remote snapshot may be stale or corrupted.
8. After the sync, load only the matching `.github/instructions/regira/*.instructions.md` files for the selected modules.
9. Prefer project-local instructions over shared Regira guidance when both exist.
10. Ask for feedback instead of guessing missing APIs or project-specific conventions.

## Sync-supported modules

| Module | Use when | Main packages |
|--------|----------|---------------|
| Entities | CRUD APIs, entity services, DTO mapping, EF Core repositories | `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` |
| IO.Storage | File storage, uploads, Azure Blob, SFTP, ZIP | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` |
| Office | Office family overview or when the user still needs to choose between PDF, Excel, Word, Mail, OCR, and related submodules | `Regira.Office` |
| Office.PDF | HTML to PDF, PDF operations, printing | `Regira.Office.PDF.SelectPdf`, `Regira.Office.PDF.Puppeteer` |
| Office.Excel | Excel read and write | `Regira.Office.Excel.ClosedXML`, `Regira.Office.Excel.MiniExcel` |
| Office.Word | Word document generation | `Regira.Office.Word.Spire` |
| Office.Mail | Email sending | `Regira.Mail.SendGrid`, `Regira.Mail.MailGun` |
| Office.CSV | CSV read and write | `Regira.Office.Csv.CsvHelper` |
| Office.Barcodes | Barcode or QR code generation | `Regira.Office.Barcodes.ZXing`, `Regira.Office.Barcodes.QRCoder` |
| Office.OCR | OCR text extraction | `Regira.Office.OCR.Tesseract` |
| Office.VCards | vCard contact files | `Regira.Office.VCards.FolkerKinzel` |
| Media | Image processing, resize, crop, rotate, FFmpeg workflows | `Regira.Media`, `Regira.Drawing.SkiaSharp`, `Regira.Drawing.GDI`, `Regira.Media.FFMpeg` |
| Security | Hashing, JWT, API key authentication, cryptography | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` |
| Web | Razor rendering, middleware, Swagger | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`, `Regira.Web.Swagger` |
| System | Windows Service hosting, project tooling | `Regira.System`, `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL, Peppol, AP gateway integrations | `Regira.Invoicing`, `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments`, `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

Use `Office` for the family overview or shared Office conventions. Add one or more concrete `Office.*` modules when the requested capability is already clear.

Modules with multiple provider packages, such as `Office.PDF` or `Office.Excel`, require the generated module guide after the sync to choose the provider package deliberately rather than guessing.

## Additional package families

These package families are available from the Regira feed but do not currently have dedicated synced AI guides. Choose them from user needs, install the matching package, and rely on general project conventions plus package-specific code references.

| Package family | Use when | Main packages |
|----------------|----------|---------------|
| Common | Shared abstractions, utilities, normalizing helpers, base contracts | `Regira.Common` |
| Caching | Runtime caching on top of the common abstractions | `Regira.Caching.Runtime` |
| DAL.EFcore | EF Core extensions and repository utilities | `Regira.DAL.EFcore` |
| DAL.MongoDB | MongoDB connectivity and backup or restore workflows | `Regira.DAL.MongoDB` |
| DAL.MySQL | MySQL or MariaDB connectivity and backup workflows | `Regira.DAL.MySQL`, `Regira.DAL.MySQL.MySqlBackup` |
| DAL.PostgreSQL | PostgreSQL connectivity | `Regira.DAL.PostgreSQL` |
| Globalization | Phone number parsing and formatting | `Regira.Globalization.LibPhoneNumber` |
| Serializing | Newtonsoft.Json-based serialization | `Regira.Serializing.Newtonsoft` |

## Sync expectations

- `regira.modules.json` is the authoritative manifest for sync-supported Regira modules.
- `projectTemplate` in `regira.modules.json` is AI-only metadata for the agent; the sync scripts do not consume it directly.
- The sync script renders `.github/copilot-instructions.md` and copies selected module guides into `.github/instructions/regira/`.
- The generated `.github/instructions/regira/*.instructions.md` files are the primary module-specific guidance after the sync.
- Refresh `AGENTS.md` from `ai/consumer.agents.stub.md` when you upgrade `aiVersion` or when Regira adds new module families that the static tables should know about.