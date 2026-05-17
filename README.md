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

AI assistance for Regira works in three complementary layers, each covering a different phase of development:

1. **Discover** — the MCP server helps the agent find the right packages before anything is installed
2. **Bootstrap** — an `AGENTS.md` file in your repo gives the agent the project setup workflow
3. **Implement** — per-package guide files extracted locally on `dotnet build` give the agent detailed implementation instructions

You can use any layer in isolation, but together they give the agent everything it needs from first idea to working code.

### Step 1 — Connect the MCP server (discovery)

Regira exposes a hosted MCP server at `https://mcp.regira.com/mcp`. Connect it to your AI tool once and the agent can search the full package catalog, get recommendations, and fetch setup instructions — without you having to install anything first.

#### GitHub Copilot (VS Code)

Add `.vscode/mcp.json` to your project:

```json
{
  "servers": {
    "regira": {
      "type": "http",
      "url": "https://mcp.regira.com/mcp"
    }
  }
}
```

Then switch Copilot Chat to **Agent mode** — the Regira tools appear automatically.

#### Claude Desktop

Edit `~/.claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "regira": {
      "url": "https://mcp.regira.com/mcp"
    }
  }
}
```

Restart Claude Desktop. The Regira tools appear automatically.

#### Claude Code (VS Code extension)

Add to `.claude/settings.json` in your project:

```json
{
  "mcpServers": {
    "regira": {
      "url": "https://mcp.regira.com/mcp"
    }
  }
}
```

#### Cursor

Settings → MCP Servers → Add server → paste `https://mcp.regira.com/mcp`.

#### Available tools

| Tool | What it does |
|------|-------------|
| `list_packages` | Browse all packages, optionally filtered by category |
| `search_packages` | Keyword/use-case search, returns ranked results |
| `recommend_packages` | Describe a feature, get package suggestions |
| `get_package` | Full docs, examples, and setup instructions for one package |
| `get_bootstrap_guide` | Consumer project setup guide (NuGet config, DI, workflow) |

### Step 2 — Bootstrap your project

Copy [ai/AGENTS.md](ai/AGENTS.md) into the root of your application repository as `AGENTS.md`. This gives the agent the full Regira project setup workflow: NuGet configuration, project templates, and which packages to install for common scenarios.

This works for both a new empty folder and an existing application that needs extra Regira features. When the MCP server is also connected, the agent can call `get_bootstrap_guide` to retrieve this same guide without you copying the file manually.

### Step 3 — Per-package guides (post-install)

When a Regira package is installed and you run `dotnet build`, the package extracts its AI guide files into `.github/instructions/regira/` in your repository. These local files give the agent detailed implementation instructions, code examples, and API signatures for the specific packages you have installed.

Install `Regira.Setup` to also extract the shared setup guides `project.setup.md` and `shared.setup.md`. Individual module packages extract their own guides the same way.
