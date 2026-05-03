# Regira Setup

> This file was installed by `Regira.Setup`. Load it when helping a user set up a new project that consumes Regira packages.

## Your role

1. Ask the user what they are building if not already clear.
2. Select the relevant modules from the table below.
3. Follow the setup steps in order.
4. After the sync, load `.github/instructions/regira/*.instructions.md` for detailed per-module guidance.

## Available modules

| Module | Use when | Main packages |
|--------|----------|---------------|
| Entities | CRUD APIs, entity services, DTO mapping, EF Core repositories | `Regira.Entities`, `Regira.Entities.EFcore`, `Regira.Entities.Web` |
| IO.Storage | File storage, uploads, Azure Blob, SFTP, ZIP | `Regira.IO.Storage`, `Regira.IO.Storage.Azure`, `Regira.IO.Storage.SSH` |
| Office.PDF | HTML → PDF, PDF operations | `Regira.Office.PDF.SelectPdf`, `Regira.Office.PDF.Puppeteer` |
| Office.Excel | Excel read/write | `Regira.Office.Excel.ClosedXML`, `Regira.Office.Excel.MiniExcel` |
| Office.Word | Word document generation | `Regira.Office.Word.Spire` |
| Office.Mail | Email sending | `Regira.Mail.SendGrid`, `Regira.Mail.MailGun` |
| Office.CSV | CSV read/write | `Regira.Office.Csv.CsvHelper` |
| Office.Barcodes | Barcode/QR code generation | `Regira.Office.Barcodes.ZXing`, `Regira.Office.Barcodes.QRCoder` |
| Office.OCR | OCR text extraction | `Regira.Office.OCR.Tesseract` |
| Office.VCards | vCard contact files | `Regira.Office.VCards.FolkerKinzel` |
| Media | Image processing, resize/crop/rotate | `Regira.Drawing.SkiaSharp`, `Regira.Drawing.GDI` |
| Security | Hashing, JWT, API key authentication | `Regira.Security`, `Regira.Security.Authentication`, `Regira.Security.Authentication.Web` |
| Web | Razor rendering, middleware, Swagger | `Regira.Web`, `Regira.Web.HTML.RazorEngineCore`, `Regira.Web.Swagger` |
| System | Windows Service hosting, project tooling | `Regira.System.Hosting` |
| Invoicing | Invoice models, UBL/Peppol, AP gateway integrations | `Regira.Invoicing.UblSharp` |
| Payments | Payment providers, payment links, webhooks | `Regira.Payments.Mollie`, `Regira.Payments.Pom` |
| TreeList | Hierarchical tree structures | `Regira.TreeList` |

> Modules with multiple package options (e.g. Office.PDF, Office.Excel) have provider-selection guidance in their module guide. Load it after the sync to choose the right provider.

## Setup steps

### 1 — NuGet feed

Add the Regira feed to `NuGet.Config` at the **repository root** if it is not already present:

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

### 2 — Create `regira.modules.json`

Copy `regira.modules.template.json` (installed alongside this file) to the **repository root** as `regira.modules.json`, then fill in the selected modules:

```json
{
  "aiVersion": "5.0.0",
  "projectTemplate": "<ConsoleWithLogging|BasicApi|SelfHostingApi|SelfHostingApiWithAuth>",
  "modules": ["<selected modules>"],
  "references": {}
}
```

### 3 — Run the sync script

Run from the **repository root**, pointing to the scripts installed in this project's `tools/ai/` directory:

```powershell
pwsh <this-project-dir>/tools/ai/sync-consumer-instructions.ps1
```

The script fetches the versioned instruction snapshot and writes:
- `AGENTS.md` — project bootstrap (picked up by OpenAI-compatible agents)
- `.github/copilot-instructions.md` — same bootstrap (picked up by GitHub Copilot)
- `.github/instructions/regira/` — selected module guides

The generated bootstrap files also list the exact `.github/instructions/regira/*.md` paths that the agent should load for the selected modules.

### 4 — Load module guides and complete setup

After the sync, load the matching guide paths listed in `AGENTS.md` or `.github/copilot-instructions.md`, then follow the selected `*.instructions.md` files in `.github/instructions/regira/` for package installation and dependency injection setup guidance.
