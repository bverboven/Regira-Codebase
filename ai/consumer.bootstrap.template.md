# Project AI Bootstrap

Selected `projectTemplate`: `{{PROJECT_TEMPLATE}}`

Template guidance: {{PROJECT_TEMPLATE_SUMMARY}}

This project consumes the following Regira modules:
{{MODULES}}

Rules:
1. `regira.modules.json` is the authoritative manifest for the selected Regira modules, deep references, and the current `projectTemplate`. If the project shape has changed, update the manifest before generating broader scaffolding changes.
2. Keep setup advice and generated code aligned with the selected `projectTemplate` before applying module-specific guidance.
3. If the user's request implies Regira capabilities that are not covered by the current module list, read `.github/AGENTS.md` to choose additional modules, update `regira.modules.json`, and re-run the sync.
4. Do not load every Regira instruction file.
5. Load the matching module guide first.
6. Load setup, examples, namespaces, and signatures only when the current task needs them.
7. Prefer project-local instructions over shared Regira guidance when both exist.
8. Ask for feedback instead of guessing missing APIs or project-specific conventions.
9. Synced Regira guides live under `.github/instructions/regira/` at the repository root. Load the matching `*.instructions.md` file from that folder before answering Regira-related requests.

## Regira Module Guides

{{MODULE_GUIDES}}

Requested deep-reference files (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) are synced into the same folder when listed in `regira.modules.json`.
