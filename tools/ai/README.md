# tools/ai — Consumer-Project Sync Scripts

These scripts sync Regira AI instruction files into a consumer project that consumes Regira packages.

## Getting started with a new consumer project

Follow these steps when creating a project from scratch that will consume Regira packages.

### 1. Scaffold the project

Create your project directory and initialise it with the appropriate template.
Consult [`ai/project.setup.md`](../../ai/project.setup.md) to pick the right template (`ConsoleWithLogging`, `BasicApi`, `SelfHostingApi`, or `SelfHostingApiWithAuth`).
```
dotnet new web -n MyWebshop   # or sln + project, depending on your preference
```

### 2. Add the Regira NuGet feed

Create `NuGet.Config` at the repository root:

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

### 3. Create the manifest

Copy the template and edit it to declare the Regira modules your project uses:

```powershell
# from your project root
Copy-Item path\to\Regira-Codebase\ai\regira.modules.template.json regira.modules.json
```

Edit `regira.modules.json`:

```json
{
  "aiVersion": "5.0.0",
  "projectTemplate": "BasicApi",
  "modules": ["Entities", "Security"],
  "references": {
    "Entities": ["setup", "examples"]
  }
}
```

Valid `references` suffixes: `setup`, `examples`, `signatures`, `namespaces`.

### 4. Run the sync script

```powershell
# PowerShell — remote fetch (recommended for first run)
pwsh path\to\Regira-Codebase\tools\ai\sync-consumer-instructions.ps1

# PowerShell — local source checkout (faster if you have Regira-Codebase checked out)
pwsh path\to\Regira-Codebase\tools\ai\sync-consumer-instructions.ps1 -SourcePath path\to\Regira-Codebase
```

The script generates `AGENTS.md` and `.github/copilot-instructions.md` (same bootstrap content) and populates `.github/instructions/regira/` with the selected instruction files. Your AI assistant picks these up automatically.

Re-run the script whenever you add a module or change `aiVersion`.

---

## Prerequisites

- **git** — required by both scripts for the remote-fetch path
- **python3** — required by the bash script for JSON parsing
- **PowerShell 7+** (`pwsh`) — for the PowerShell script; works on Windows, macOS, and Linux

> **macOS note**: The bash script requires bash 4.0+. macOS ships with bash 3.2.
> Either install a newer bash (`brew install bash`) or use the PowerShell script:
> `pwsh tools/ai/sync-consumer-instructions.ps1`

## Usage

```sh
# Bash — sync from the pinned remote tag
./tools/ai/sync-consumer-instructions.sh

# PowerShell — sync from the pinned remote tag
pwsh tools/ai/sync-consumer-instructions.ps1

# Use a local source checkout (both scripts support this)
./tools/ai/sync-consumer-instructions.sh  --source ../Regira-Codebase
pwsh tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase
```

## What the scripts do

1. Read `regira.modules.json` at the repository root.
2. Resolve the pinned `aiVersion` to the Regira source-repo Git tag `ai-v{aiVersion}` and shallow-clone only the `ai/` folder (skipped when `--source` / `-SourcePath` is supplied).
3. Render the bootstrap from `consumer.bootstrap.template.md` and write it to both `AGENTS.md` (project root) and `.github/copilot-instructions.md`.
4. Copy the selected module guides into `.github/instructions/regira/`.
5. Copy any requested deep-reference files into `.github/instructions/regira/`.

## Output layout

```
AGENTS.md                          ← rendered bootstrap (OpenAI-compatible agents)
.github/
  copilot-instructions.md          ← rendered bootstrap (GitHub Copilot)
  instructions/
    regira/
      entities.instructions.md
      entities.setup.md
      entities.examples.md
      security.instructions.md
      ...
```

## When to run

| Trigger | Action |
|---------|--------|
| New consumer project | Run immediately after creating `regira.modules.json` |
| Adding a Regira module | Update `regira.modules.json`, then re-run |
| Upgrading Regira packages | Change `aiVersion` in `regira.modules.json`, then re-run |

## Creating an `ai-v*` tag (source-repo maintainers only)

Consumer projects pin their instruction snapshot via `aiVersion` in `regira.modules.json`.
The sync scripts map this to a Git tag `ai-v{aiVersion}` in this repository.

To publish a new snapshot, use the **Tag AI release** GitHub Actions workflow
(`.github/workflows/tag-ai-release.yml`) from the Actions tab, or run:

```sh
git tag ai-v5.0.0
git push origin ai-v5.0.0
```
