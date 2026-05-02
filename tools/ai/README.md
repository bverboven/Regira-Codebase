# tools/ai — Consumer-Project Sync Scripts

These scripts sync Regira AI instruction files into a consumer project that consumes Regira packages.

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
./tools/ai/sync-consumer-instructions.sh  --source ../Regira-Codebase/ai
pwsh tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase/ai
```

## What the scripts do

1. Read `regira.modules.json` at the repository root.
2. Resolve the pinned `aiVersion` to the Regira source-repo Git tag `ai-v{aiVersion}` and shallow-clone only the `ai/` folder (skipped when `--source` / `-SourcePath` is supplied).
3. Render `.github/copilot-instructions.md` from `consumer.bootstrap.template.md` using the active module list.
4. Copy the selected module guides into `.github/instructions/regira/`.
5. Copy any requested deep-reference files into `.github/instructions/regira/`.

## Output layout

```
.github/
  copilot-instructions.md          ← rendered bootstrap
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
