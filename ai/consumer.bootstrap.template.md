# Project AI Bootstrap

Selected `projectTemplate`: `{{PROJECT_TEMPLATE}}`

Template guidance: {{PROJECT_TEMPLATE_SUMMARY}}

This project consumes the following Regira modules:
{{MODULES}}

Shared setup guides:
- `.github/instructions/regira/project.setup.md` - template-specific scaffolding baseline. Load this before changing project shape, hosting, logging, authentication, or DI structure.
- `.github/instructions/regira/shared.setup.md` - shared Regira setup rules. Load this before module-specific setup docs when bootstrapping or changing infrastructure.

Rules:
1. `regira.modules.json` is the authoritative manifest for the selected Regira modules, deep references, and the current `projectTemplate`. If the project shape has changed, update the manifest before generating broader scaffolding changes.
2. Keep setup advice and generated code aligned with the selected `projectTemplate` before applying module-specific guidance.
3. If the user's request implies Regira capabilities that are not covered by the current module list, read `.github/AGENTS.md` to choose additional modules, update `regira.modules.json`, and re-run the sync.
4. Do not load every Regira instruction file.
5. When scaffolding a new project or changing infrastructure, load the shared setup guides first.
6. Load the matching module guide after the shared setup guides.
7. Load setup, examples, namespaces, and signatures only when the current task needs them.
8. If `.github/instructions/regira/` contains matching `*.instructions.md` files, read every relevant guide in full before generating entity, service, controller, DI, or infrastructure code.
9. Prefer project-local instructions over shared Regira guidance when both exist.
10. Ask for feedback instead of guessing missing APIs or project-specific conventions.
11. Synced Regira guides live under `.github/instructions/regira/` at the repository root. Use that folder as the local source of truth for shared setup guides, matching module guides, and requested deep references.

## Regira Module Guides

{{MODULE_GUIDES}}

Requested deep-reference files (`*.setup.md`, `*.examples.md`, `*.signatures.md`, `*.namespaces.md`) are synced into the same folder when listed in `regira.modules.json`.
