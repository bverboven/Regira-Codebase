# Consumer Project Bootstrap Template

Use this template in a project that **consumes** Regira packages but does not contain the full Regira source repository. Keep it small and project-specific.

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

## What to Include Locally

- a tiny project bootstrap like the template above
- only the module guides that the app actively uses
- only the deep reference files that are important to the app

## What Not to Include by Default

- the entire source-repository `ai/` folder
- unrelated module guides
- large example/reference files that the consumer project never uses

## Next Step

For a gradual rollout, follow [`consumer-project.migration-plan.md`](./consumer-project.migration-plan.md).
