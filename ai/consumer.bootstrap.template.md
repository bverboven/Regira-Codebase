# Project AI Bootstrap

This project consumes the following Regira modules:
{{MODULES}}

Rules:
1. If the user's request implies Regira capabilities that are not covered by the current module list, read `AGENTS.md` at the repository root to choose additional modules, update `regira.modules.json`, and re-run the sync.
2. Do not load every Regira instruction file.
3. Load the matching module guide first.
4. Load setup, examples, namespaces, and signatures only when the current task needs them.
5. Prefer project-local instructions over shared Regira guidance when both exist.
6. Ask for feedback instead of guessing missing APIs or project-specific conventions.
7. Synced Regira guides live under `.github/instructions/regira/` at the repository root. Load the matching `*.instructions.md` file from that folder before answering Regira-related requests.

## Regira Module Guides

{{MODULE_GUIDES}}

Requested deep-reference files (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) are synced into the same folder when listed in `regira.modules.json`.
