# Project AI Bootstrap

This project consumes the following Regira modules:
{{MODULES}}

Rules:
1. Do not load every Regira instruction file.
2. Load the matching module guide first.
3. Load setup, examples, namespaces, and signatures only when the current task needs them.
4. Prefer project-local instructions over shared Regira guidance when both exist.
5. Ask for feedback instead of guessing missing APIs or project-specific conventions.
6. Synced Regira guides live under `.github/instructions/regira/` at the repository root. Load the matching `*.instructions.md` file from that folder before answering Regira-related requests.

## Regira Module Guides

{{MODULE_GUIDES}}

Requested deep-reference files (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) are synced into the same folder when listed in `regira.modules.json`.
