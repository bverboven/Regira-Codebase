# Regira MCP Server — Setup Guide

This guide walks through everything needed to build, run, and deploy the Regira MCP server so any MCP-compatible AI tool can discover and use the full Regira package catalog.

## Architecture

```
Regira source repo
  └── src/*/ai/*.md  ──▶  build-mcp-knowledge  ──▶  knowledge-base.json
                                                           │
                                                    mcp-server (ASP.NET Core)
                                                           │
                                              https://mcp.regira.com/mcp
                                                           │
                                         Claude Desktop / VS Code / Cursor
```

- **`tools/build-mcp-knowledge/`** — C# console app that reads all `src/*/ai/*.md` files and outputs `knowledge-base.json`
- **`tools/mcp-server/`** — ASP.NET Core app that serves the knowledge base as an MCP server over HTTP

---

## Prerequisites

- .NET 10 SDK (dev machine)
- .NET 10 Hosting Bundle installed on the server (includes the ASP.NET Core Module for IIS)
- IIS with ARR + URL Rewrite modules, or direct in-process hosting via ANCM
- SSH or deployment access to the server

---

## Step 1 — Generate the knowledge base

Run this from the repository root:

```bash
cd tools/build-mcp-knowledge
dotnet run
```

Or pass the repo root path explicitly:

```bash
dotnet run -- /path/to/Regira-Codebase
```

This reads every `src/*/ai/*.md` file and package metadata from `.csproj` files, then writes:

```
tools/mcp-server/knowledge-base.json
```

The file is gitignored — regenerate it whenever AI guide files change.

---

## Step 2 — Run the server locally

```bash
cd tools/mcp-server
dotnet run
```

The server starts on port 60859 by default. You can override it:

```bash
ASPNETCORE_URLS=http://localhost:9999 dotnet run
```

Test the health endpoint:

```bash
curl https://localhost:60859/health
curl http://localhost:60860/health
```

Expected response:
```json
{ "status": "ok", "packages": 74, "packagesWithDocs": 11, "generated": "..." }
```

---

## Step 3 — Test with MCP Inspector

MCP Inspector is the official tool for verifying MCP servers.

> **Important:** pass `--transport streamableHttp` so the Inspector uses HTTP instead of the default stdio transport. Without this flag it will try to execute the URL as a local command and fail.

```bash
npx @modelcontextprotocol/inspector --transport streamableHttp http://localhost:60860/mcp
npx @modelcontextprotocol/inspector --transport streamableHttp https://mcp.regira.com/mcp
```

Open the Inspector UI in your browser and verify:

- `list_packages` returns all 74 packages
- `search_packages` with query `"QR code"` returns barcode packages
- `recommend_packages` with `"shopping list API with QR codes"` returns `Regira.Entities` and `Regira.Office.Barcodes.*`
- `get_package` with `"Regira.Entities"` returns full documentation
- `get_bootstrap_guide` returns the consumer setup guide

---

## Step 4 — Server setup (one-time)

### 4a — Add a subdomain

Point `mcp.regira.com` to your server via a DNS A or CNAME record.

### 4b — Install the .NET 10 Hosting Bundle on the server

Download from [https://dot.net](https://dotnet.microsoft.com/download) and install. This registers the ASP.NET Core Module (ANCM) with IIS so IIS can host .NET apps directly — no separate process manager needed.

### 4c — Create an IIS site

1. Open IIS Manager → **Add Website**
2. Site name: `regira-mcp`
3. Physical path: your deploy folder, e.g. `C:\sites\regira-mcp`
4. Binding: HTTPS, hostname `mcp.regira.com`, select your existing wildcard certificate
5. Application pool: set **Managed pipeline mode** → `Integrated`, **.NET CLR version** → `No Managed Code`

The `dotnet publish` output already includes a `web.config` that configures ANCM in-process hosting — IIS picks it up automatically.

> **Streaming responses:** Streamable HTTP uses chunked transfer. In IIS Manager → site → **Configuration Editor** → `system.webServer/serverRuntime`, set `uploadReadAheadSize` to `0` to prevent response buffering.

---

## Step 5 — Deploy manually (first time)

Build and publish on your dev machine:

```bash
# From repo root
cd tools/build-mcp-knowledge && dotnet run
cd ../mcp-server && dotnet publish -c Release -o publish
cp knowledge-base.json publish/
```

Copy the `publish/` folder contents to the IIS site's physical path on the server (e.g. via SFTP, robocopy, or your preferred method):

```bash
scp -r tools/mcp-server/publish/* user@yourserver:C:/sites/regira-mcp/
```

IIS + ANCM starts and manages the process automatically. To restart after a redeploy:

```powershell
# On the server — recycle the app pool
& "$env:windir\system32\inetsrv\appcmd" recycle apppool /apppool.name:"regira-mcp"
```

---

## Step 6 — Configure GitHub Actions (for automatic updates)

The workflow at `.github/workflows/deploy-mcp.yml` rebuilds and redeploys whenever AI guide files or package metadata change.

Add these secrets in **GitHub → Settings → Secrets and variables → Actions**:

| Secret | Value |
|--------|-------|
| `SSH_HOST` | Your server's IP or hostname |
| `SSH_USER` | SSH username |
| `SSH_KEY` | Private SSH key (the server must have the matching public key in `~/.ssh/authorized_keys`) |
| `SSH_DEPLOY_PATH` | Deploy path on server, e.g. `/opt/regira-mcp` |

After adding secrets, push any change to `src/*/ai/*.md` or trigger the workflow manually via **Actions → Deploy Regira MCP Server → Run workflow**.

---

## Step 7 — Configure your AI tool

### Claude Desktop

Edit `~/.claude/claude_desktop_config.json` (create if missing):

```json
{
  "mcpServers": {
    "regira": {
      "url": "https://mcp.regira.com/mcp"
    }
  }
}
```

Restart Claude Desktop. The five Regira tools will appear automatically.

### VS Code / Claude Code

Add to `.claude/settings.json` in your consumer project:

```json
{
  "mcpServers": {
    "regira": {
      "url": "https://mcp.regira.com/mcp"
    }
  }
}
```

### Cursor

Settings → MCP Servers → Add server → paste `https://mcp.regira.com/mcp`.

### GitHub Copilot (VS Code)

Add a `.vscode/mcp.json` file to your consumer project:

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

Or add it to your VS Code user settings (`settings.json`) to make it available in all projects:

```json
{
  "mcp": {
    "servers": {
      "regira": {
        "type": "http",
        "url": "https://mcp.regira.com/mcp"
      }
    }
  }
}
```

Once configured, Copilot Chat will list Regira as an available tool source. Use **Agent mode** (`@workspace` → switch to Agent) and the tools appear automatically.

---

## Available tools

| Tool | Description |
|------|-------------|
| `list_packages` | All 74 packages, optionally filtered by category |
| `get_package` | Full docs (instructions, examples, setup, signatures) for one package |
| `search_packages` | Keyword/use-case search, returns ranked results |
| `recommend_packages` | Describe a feature, get package suggestions |
| `get_bootstrap_guide` | Consumer project setup guide (NuGet config, DI, workflow) |

---

## Maintenance

**When AI guide files change** — the CI workflow runs automatically. To trigger manually:

```bash
cd tools/build-mcp-knowledge && dotnet run
```

Then redeploy (or push to `master` to let CI handle it).

**When a new package is added** — add an `ai/` folder with at least `{name}.instructions.md` and `{name}.examples.md`, following the pattern in `src/Common.Entities/ai/`. The build script picks it up automatically on the next run.

**Check server status**:

```bash
curl https://mcp.regira.com/health
```

```powershell
# On the server
& "$env:windir\system32\inetsrv\appcmd" list site /site.name:"regira-mcp"
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```
