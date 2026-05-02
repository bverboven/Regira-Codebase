# Migration Plan for a Scalable Consumer-Project Model

## Goal

Move from a source-repo-oriented instruction system to a selective, scalable model for downstream projects that consume Regira packages.

## Target Model

Consumer projects should receive:

1. a tiny local bootstrap
2. only the module guides they actually use
3. deep reference files only when needed

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

## Phase 3 — Define Consumer Distribution

1. Create a small consumer bootstrap template.
2. Introduce a manifest or selection mechanism for enabled Regira modules.
3. Let each consumer project opt into only the modules it uses.
4. Copy or sync only the selected instruction files into the consumer repo.

## Phase 4 — Provide a Starter Experience

1. Offer a starter template for new consumer projects.
2. Include:
   - NuGet feed setup
   - a small bootstrap instruction file
   - selected module guides
   - an optional manifest of enabled modules
3. Make this starter the preferred way to begin a Regira-based app.

## Phase 5 — Add Update / Sync Capability

1. Introduce a lightweight internal process or tool to:
   - add module instructions to a consumer project
   - refresh them when source instructions change
   - keep the selected files aligned with the project
2. Make the sync version-aware so consumers can align instructions with the package versions they use.

## Phase 6 — Consider MCP Only After the Content Model Is Stable

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
