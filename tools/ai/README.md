# tools/ai — Consumer-Project Sync Scripts

These scripts sync versioned Regira AI instruction files into a consuming repository. The PowerShell script also supports an interactive first-run bootstrap via `-Init`; the bash script remains a sync-only alternative once the consumer manifest and bootstrap files already exist.

These scripts are optional. They are not part of the normal consumer flow. The normal consumer flow is a single committed `.github/AGENTS.md` entrypoint plus the installed Regira packages. Use the sync scripts only when you want local cached module guides and deep-reference files inside the consumer repo.

## Recommended consumer flow

Use this flow only when you want local generated instruction files in the consumer repository. If `.github/AGENTS.md` is enough for your team, skip this entire script-based flow.

### 1. Interactive init (preferred)

Run from the consumer repository root:

```powershell
# Vendored script inside the consumer repository
pwsh tools/ai/sync-consumer-instructions.ps1 -Init
```

The init flow asks for `aiVersion`, `projectTemplate`, the initial Regira modules, and optional deep references per module. It then:

- ensures `NuGet.Config` contains the Regira feed alongside `nuget.org`
- writes `regira.modules.json` as the authoritative machine-readable manifest for `aiVersion`, `projectTemplate`, selected modules, and deep references
- copies the consumer bootstrap from [`ai/AGENTS.md`](../../ai/AGENTS.md) to `.github/AGENTS.md`
- optionally vendors the PowerShell sync script into `tools/ai/` when you started from an external checkout
- immediately continues with the normal sync so `.github/copilot-instructions.md` and `.github/instructions/regira/` exist

`.github/AGENTS.md` remains the stable human-facing bootstrap that decides templates, modules, and packages from user input. The sync uses `regira.modules.json` to render `.github/copilot-instructions.md` and copy only the selected module guides.

If the consumer team does not want local generated guide files, stop at `.github/AGENTS.md` and skip the sync workflow entirely.

### 2. Re-run the sync when the manifest changes

After the first bootstrap, keep using the normal sync command whenever you add a module or change `aiVersion`:

```powershell
pwsh tools/ai/sync-consumer-instructions.ps1

# Refresh the cached remote snapshot first when needed
pwsh tools/ai/sync-consumer-instructions.ps1 -Force
```

### 3. Manual bootstrap (fallback)

Use this path when you want explicit control over each bootstrap file or when you cannot use the interactive PowerShell flow.

#### 1. Add the Regira NuGet feed

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

#### 2. Create `regira.modules.json`

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

`projectTemplate` is AI-only metadata that the consumer `.github/AGENTS.md` bootstrap and rendered Copilot bootstrap read to keep setup advice consistent. The sync scripts do not use it to choose files, but they do surface it into the generated bootstrap for downstream guidance.

#### 3. Add the canonical `.github/AGENTS.md` bootstrap

Copy [`ai/AGENTS.md`](../../ai/AGENTS.md) to `.github/AGENTS.md` in the consumer repository. This file tells the AI agent how to choose Regira modules from user requirements and which package families to install from the Regira feed.

When you upgrade `aiVersion` or when Regira adds new module families, re-copy [`ai/AGENTS.md`](../../ai/AGENTS.md) into `.github/AGENTS.md` so the static module tables stay current.

#### 4. Add the optional Copilot bridge

Only do this when a tool explicitly requires `.github/copilot-instructions.md` before any rendered bootstrap exists. In that case, copy [`ai/consumer.copilot.stub.md`](../../ai/consumer.copilot.stub.md) to `.github/copilot-instructions.md`. It is only a thin bridge back to the canonical `.github/AGENTS.md` bootstrap.

#### 5. Add the sync script at repository root

Preferred: vendor [`tools/ai/sync-consumer-instructions.ps1`](./sync-consumer-instructions.ps1) into `tools/ai/` in the consumer repository.

#### 6. Run the sync

Run from the consumer repository root:

```powershell
pwsh tools/ai/sync-consumer-instructions.ps1

# Refresh the cached remote snapshot first when needed
pwsh tools/ai/sync-consumer-instructions.ps1 -Force
```

The sync updates `.github/copilot-instructions.md` with the resolved module list and exact guide paths, and copies the selected Regira instruction files into `.github/instructions/regira/`. It does not overwrite the canonical `.github/AGENTS.md` bootstrap.

#### 7. Re-run the sync when the manifest changes

Re-run the script whenever you add a module or change `aiVersion`.

---

## Prerequisites

- **git** — required by both scripts for the remote-fetch path
- **PowerShell 7+** (`pwsh`) — canonical consumer path; required for the interactive `-Init` bootstrap and works on Windows, macOS, and Linux
- **bash 4.0+** and **python3** — required only for the optional bash script

> **macOS note**: The bash script requires bash 4.0+. macOS ships with bash 3.2.
> Either install a newer bash (`brew install bash`) or use the PowerShell script:
> `pwsh tools/ai/sync-consumer-instructions.ps1`

## Usage

```powershell
# Consumer repository — interactive bootstrap and first sync
pwsh tools/ai/sync-consumer-instructions.ps1 -Init

# Consumer repository — canonical path
pwsh tools/ai/sync-consumer-instructions.ps1

# Consumer repository — refresh the cached remote snapshot first
pwsh tools/ai/sync-consumer-instructions.ps1 -Force
```

```sh
# Bash — optional alternative
./tools/ai/sync-consumer-instructions.sh

# Bash — refresh the cached remote snapshot first
./tools/ai/sync-consumer-instructions.sh --force
```

## What the scripts do

When `-Init` is used, the PowerShell script first ensures `NuGet.Config` contains the Regira feed, writes `regira.modules.json`, copies `.github/AGENTS.md`, optionally vendors itself into `tools/ai/`, and then continues with the sync steps below.

1. Read `regira.modules.json` at the repository root.
2. Resolve the pinned `aiVersion` to the Regira source-repo Git tag `ai-v{aiVersion}`, fetch the shared `ai/` folder, and extend sparse checkout with the module-specific `src/*/ai/` folders required by the selected modules (skipped when `--source` / `-SourcePath` is supplied). Reuse the cached remote snapshot by default, or refresh it with `-Force` / `--force`.
3. Render the bootstrap from `consumer.bootstrap.template.md` and write it to `.github/copilot-instructions.md`, including the selected `projectTemplate` plus the exact guide paths under `.github/instructions/regira/`.
4. Keep `.github/AGENTS.md` as the stable consumer bootstrap that tells the AI agent how to choose modules and when to resync.
5. Copy the selected module guides and requested deep-reference files into `.github/instructions/regira/`.

## Output layout

```
regira.modules.json                ← committed sync manifest; aiVersion + projectTemplate + modules + references
.github/
  AGENTS.md                        ← committed static bootstrap; choose modules here before syncing
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
| New consumer repository | Run `pwsh tools/ai/sync-consumer-instructions.ps1 -Init` from the repository root |
| Adding a Regira module | Update `regira.modules.json`, then re-run |
| Changing the application shape | Update `projectTemplate` in `regira.modules.json`, then re-run so the rendered bootstrap stays aligned |
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
