# tools/ai — Consumer-Project Sync Scripts

These scripts sync versioned Regira AI instruction files into a consuming repository. The recommended flow is explicit and repository-root based: commit a small manifest plus a canonical `AGENTS.md` bootstrap, then run the sync command when you need the Regira guides.

## Recommended consumer flow

Start here for a new project that consumes Regira packages.

### 1. Add the Regira NuGet feed

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

### 2. Create `regira.modules.json`

Copy [`ai/regira.modules.template.json`](../../ai/regira.modules.template.json) into the consumer repository root as `regira.modules.json`, then edit it to declare the sync-supported Regira modules your project uses:

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

`projectTemplate` is AI-only metadata that the consumer `AGENTS.md` bootstrap reads to keep setup advice consistent. The sync scripts do not consume it directly.

### 3. Add the canonical `AGENTS.md` bootstrap

Copy [`ai/consumer.agents.stub.md`](../../ai/consumer.agents.stub.md) to `AGENTS.md` in the consumer repository. This file tells the AI agent how to choose Regira modules from user requirements and which package families to install from the Regira feed.

When you upgrade `aiVersion` or when Regira adds new module families, re-copy [`ai/consumer.agents.stub.md`](../../ai/consumer.agents.stub.md) into `AGENTS.md` so the static module tables stay current.

### 4. Add the optional Copilot bridge

If the consumer repository uses GitHub Copilot before the first sync, also copy [`ai/consumer.copilot.stub.md`](../../ai/consumer.copilot.stub.md) to `.github/copilot-instructions.md`. That file points Copilot at the canonical `AGENTS.md` bootstrap until the sync renders a Copilot-specific bootstrap.

### 5. Add the sync script at repository root

Preferred: vendor [`tools/ai/sync-consumer-instructions.ps1`](./sync-consumer-instructions.ps1) into `tools/ai/` in the consumer repository.

Alternative: keep using the script directly from a local `Regira-Codebase` checkout.

### 6. Run the sync

Run from the consumer repository root:

```powershell
pwsh tools/ai/sync-consumer-instructions.ps1

# Refresh the cached remote snapshot first when needed
pwsh tools/ai/sync-consumer-instructions.ps1 -Force
```

The sync updates `.github/copilot-instructions.md` with the resolved module list and exact guide paths, and copies the selected Regira instruction files into `.github/instructions/regira/`. It does not overwrite the canonical `AGENTS.md` bootstrap.

### 7. Re-run the sync when the manifest changes

Re-run the script whenever you add a module or change `aiVersion`.

---

## Alternative: run from a source checkout

Use this path when you do not want to vendor the script into the consumer repository.

```powershell
# PowerShell — remote fetch via the source checkout script
pwsh path\to\Regira-Codebase\tools\ai\sync-consumer-instructions.ps1

# PowerShell — use the local source checkout directly instead of the pinned remote tag
pwsh path\to\Regira-Codebase\tools\ai\sync-consumer-instructions.ps1 -SourcePath path\to\Regira-Codebase
```

---

## Prerequisites

- **git** — required by both scripts for the remote-fetch path
- **PowerShell 7+** (`pwsh`) — canonical consumer path; works on Windows, macOS, and Linux
- **bash 4.0+** and **python3** — required only for the optional bash script

> **macOS note**: The bash script requires bash 4.0+. macOS ships with bash 3.2.
> Either install a newer bash (`brew install bash`) or use the PowerShell script:
> `pwsh tools/ai/sync-consumer-instructions.ps1`

## Usage

```powershell
# Consumer repository — canonical path
pwsh tools/ai/sync-consumer-instructions.ps1

# Consumer repository — refresh the cached remote snapshot first
pwsh tools/ai/sync-consumer-instructions.ps1 -Force

# Consumer repository — use a local source checkout instead of the pinned remote tag
pwsh tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase

# Run directly from a Regira-Codebase checkout
pwsh path/to/Regira-Codebase/tools/ai/sync-consumer-instructions.ps1
```

```sh
# Bash — optional alternative
./tools/ai/sync-consumer-instructions.sh

# Bash — refresh the cached remote snapshot first
./tools/ai/sync-consumer-instructions.sh --force

# Bash — use a local source checkout instead of the pinned remote tag
./tools/ai/sync-consumer-instructions.sh --source ../Regira-Codebase
```

## What the scripts do

1. Read `regira.modules.json` at the repository root.
2. Resolve the pinned `aiVersion` to the Regira source-repo Git tag `ai-v{aiVersion}`, fetch the shared `ai/` folder, and extend sparse checkout with the module-specific `src/*/ai/` folders required by the selected modules (skipped when `--source` / `-SourcePath` is supplied). Reuse the cached remote snapshot by default, or refresh it with `-Force` / `--force`.
3. Render the bootstrap from `consumer.bootstrap.template.md` and write it to `.github/copilot-instructions.md`, including the exact guide paths under `.github/instructions/regira/`.
4. Keep `AGENTS.md` as the stable consumer bootstrap that tells the AI agent how to choose modules and when to resync.
5. Copy the selected module guides and requested deep-reference files into `.github/instructions/regira/`.

## Output layout

```
AGENTS.md                          ← committed static bootstrap; choose modules here before syncing
.github/
  copilot-instructions.md          ← rendered bootstrap (GitHub Copilot)
  instructions/
    regira/
      entities.instructions.md
      entities.setup.md
      entities.examples.md
      office.pdf.instructions.md
      security.instructions.md
      ...
```

## When to run

| Trigger | Action |
|---------|--------|
| New consumer repository | After creating `regira.modules.json`, `AGENTS.md`, and optionally `.github/copilot-instructions.md`, run the sync |
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
