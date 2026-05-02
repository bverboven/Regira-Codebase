# Migration Plan for a Scalable Consumer-Project Model

## Goal

Move from a source-repo-oriented instruction system to a selective, scalable model for downstream projects that consume Regira packages.

## Target Model

Consumer projects should receive:

1. a tiny local bootstrap
2. only the module guides they actually use
3. deep reference files only when needed

## Concrete Consumer Manifest

Use a project-local `regira.modules.json` file as the source of truth for which Regira instructions should be present in a consumer repository.

### Recommended v1 shape

```json
{
  "aiVersion": "5.0.0",
  "projectTemplate": "BasicApi",
  "modules": [
    "Entities",
    "Security",
    "Office.PDF"
  ],
  "references": {
    "Entities": [ "setup", "examples" ],
    "Office.PDF": [ "examples" ]
  }
}
```

### Field meanings

- `aiVersion` — the pinned version of the instruction snapshot to copy from
- `projectTemplate` — the project template selected from [`project.setup.md`](./project.setup.md); this is onboarding metadata only, and the sync script should ignore it
- `modules` — the enabled Regira module guides to copy into the consumer project
- `references` — optional deep-reference files to include per module; valid v1 values are `setup`, `examples`, `signatures`, and `namespaces`

This format is intentionally simple so it can drive both manual onboarding and a future sync script.

## Phase 1 — Stabilize the Source Instruction System

1. Keep [`copilot-instructions.md`](./copilot-instructions.md) as the source-repo bootstrap.
2. Keep it thin so it stays a router instead of becoming a full catalog.
3. Move repeated shared setup into [`shared.setup.md`](./shared.setup.md) or another shared reference.
4. Keep [`learnings.md`](./learnings.md) short, curated, and capped instead of turning it into a historical log.

## Phase 2 — Split Guide vs Reference

For each module, distinguish between:

- **module guide** — purpose, routing rules, decision guidance, common pitfalls
- **setup** — package install and initial project wiring
- **examples** — usage samples and composition patterns
- **exact reference** — namespaces, signatures, and precise API lookups

Keep module guides short. Load setup, examples, and exact references only on demand.

## Phase 3 — Consumer Distribution and Starter Experience

Treat onboarding as one action:

1. Pick the base project template from [`project.setup.md`](./project.setup.md).
2. Add `regira.modules.json` to declare the active Regira modules.
3. Generate or copy:
   - the small bootstrap instruction file
   - the selected module guides
   - only the requested deep-reference files
4. Make this the default starter flow for a new Regira consumer project.

In other words: **pick project template + pick active modules + sync selected instructions** should converge into one onboarding action.

### Default destination layout

For v1, use this destination layout inside the consumer repository:

- `.github/copilot-instructions.md` — the small local bootstrap
- `.github/instructions/regira/` — synced Regira module guides and deep-reference files
- `regira.modules.json` — the project-local manifest at the repository root

## Phase 4 — Add Update / Sync Capability

Start with a simple script-based implementation instead of a custom service:

1. Provide a checked-in PowerShell script and a matching bash script.
2. The script reads `regira.modules.json`.
3. The script copies the matching bootstrap, module guides, and optional deep references from a versioned AI snapshot into the consumer repo.
4. The same script handles both first-time setup and later refreshes.

Recommended v1 filenames:

- `tools/ai/sync-consumer-instructions.ps1`
- `tools/ai/sync-consumer-instructions.sh`

### Who runs it and when

- **New project creation** — run it immediately after choosing the project template
- **Adding a Regira module** — run it after updating `regira.modules.json`
- **Upgrading Regira packages** — run it after changing the pinned `aiVersion`
- **Ongoing maintenance** — consumer-project maintainers run it manually; CI can later verify that checked-in instruction files still match the manifest

### Versioning strategy

- Pin the copied instructions with `aiVersion` instead of always pulling the latest content.
- The concrete v1 rule is: map `aiVersion` to a Git tag in the Regira source repository named `ai/v{aiVersion}` and copy the `ai/` folder from that tag.
- The sync script should support an optional local override path for maintainers working inside a checked-out source repo, but consumer projects should not depend on having the source repo locally.
- If the requested `ai/v{aiVersion}` tag does not exist, the script should fail with a clear error instead of guessing another source.
- Consumer projects update deliberately by changing `aiVersion` and re-running the sync script.
- Keep the instruction snapshot aligned with the Regira package version used by the consumer project whenever possible.

### Source resolution in the sync script

Use a remote-first, tag-based lookup so the same script works for consumer repos that only have their own project checked out:

1. Read `aiVersion` from `regira.modules.json`.
2. Resolve it to the Regira Git tag `ai/v{aiVersion}`.
3. Shallow-clone the tagged snapshot into a temporary location and read the `ai/` folder from there.
4. Copy the bootstrap to `.github/copilot-instructions.md` and copy the selected module guides and requested deep-reference files into `.github/instructions/regira/`.
5. Ignore `projectTemplate` during sync; it documents how the project started, but does not affect which files are copied.

This keeps version resolution deterministic and makes the first script implementable with the shallow-clone path that works on hosted Git providers such as GitHub.

## Phase 5 — Consider MCP Only After the Content Model Is Stable

Add a custom MCP server only when you need centralized, query-based retrieval instead of copying files. Useful MCP scenarios include:

- module discovery
- package-to-instruction mapping
- version-aware reference lookup
- generated API/reference lookup from source or XML docs

MCP should augment a thin local bootstrap, not replace all local guidance from day one.

## Recommended Default for New Consumer Projects

1. Start from a project template.
2. Add a small local bootstrap.
3. Select only the active Regira modules.
4. Add deep reference files only for the modules that matter to that app.
5. Add a simple path to refresh the instruction set later.
