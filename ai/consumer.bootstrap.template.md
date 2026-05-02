# Consumer Project Bootstrap Template

Use this template in a project that **consumes** Regira packages but does not contain the full Regira source repository. Keep it small and project-specific.

Pair it with a project template from [`project.setup.md`](./project.setup.md) and a local `regira.modules.json` manifest so onboarding becomes one action instead of a manual checklist.

## Suggested Template

```md
# Project AI Bootstrap

This project consumes the following Regira modules:
- Entities
- Office.PDF
- Security

Rules:
1. Do not load every Regira instruction file.
2. Load the matching module guide first.
3. Load setup, examples, namespaces, and signatures only when the current task needs them.
4. Prefer project-local instructions over shared Regira guidance when both exist.
5. Ask for feedback instead of guessing missing APIs or project-specific conventions.
```

## Suggested Manifest

Use [`regira.modules.template.json`](./regira.modules.template.json) as the starting point for a project-local `regira.modules.json` file.

## How the Template Is Used

The sync script should render this template into `.github/copilot-instructions.md` from `regira.modules.json` on every run. Do not copy a fixed bootstrap from the source repo, because the active module list is consumer-specific.

## What to Include Locally

- a tiny generated project bootstrap like the template above, stored as `.github/copilot-instructions.md`
- a `regira.modules.json` file that lists the selected Regira modules
- only the module guides that the app actively uses, copied into `.github/instructions/regira/`
- only the deep reference files that are important to the app, also copied into `.github/instructions/regira/`

## What Not to Include by Default

- the entire source-repository `ai/` folder
- unrelated module guides
- large example/reference files that the consumer project never uses

## Next Step

For a gradual rollout, follow [`consumer-project.migration-plan.md`](./consumer-project.migration-plan.md).
