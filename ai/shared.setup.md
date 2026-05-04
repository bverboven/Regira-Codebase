# Shared Setup for Regira AI Instructions

> **Role:** Load this file for setup rules reused by multiple module guides and for consumer-guide sync or distribution mechanics.
>
> **Boundaries:** Use [`regira.capabilities.md`](./regira.capabilities.md) for module discovery and guide routing. Use [`project.setup.md`](./project.setup.md) for project-template choice and baseline app shape. Use [`AGENTS.md`](./AGENTS.md) or downstream `.github/AGENTS.md` for the execution-oriented consumer workflow.

Use this file when a module guide needs shared setup without repeating it.

## NuGet Feed

Regira packages are published at `https://packages.regira.com/v3/index.json`. Add this feed to `NuGet.Config` alongside `nuget.org`:

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

## Consumer Sync and Local Cache

- **Source repository**: the full `ai/` folder is available locally, so this file can point to adjacent guides.
- **Consumer project**: the normal entrypoint is `.github/AGENTS.md`; synced `.github/instructions/regira/*.md` files are optional cached guidance.

Keep these responsibilities separate in consumer projects:

- `.github/AGENTS.md` decides project template, module selection, package routing, and when extracted local guides must be read before code generation.
- `regira.modules.json` is an optional machine-readable manifest for `aiVersion`, `projectTemplate`, selected modules, and deep references.
- `.github/copilot-instructions.md` and `.github/instructions/regira/*.md` are optional generated outputs created by sync tooling or package extraction.
- `project.setup.md` stays focused on template selection and baseline app shape.
- `shared.setup.md` stays focused on setup rules reused across modules and sync mechanics.

Consumers do not need the source-repository `ai/` folder for the normal one-file flow. Use sync only when a team explicitly wants local cached shared and module guidance.

## Related Files

- [`AGENTS.md`](./AGENTS.md) — canonical downstream bootstrap to copy as `.github/AGENTS.md`
- [`regira.capabilities.md`](./regira.capabilities.md) — canonical Regira capability catalog for AI agents
- [`project.setup.md`](./project.setup.md) — canonical shared project-template guide; synced to `.github/instructions/regira/project.setup.md` when the optional sync is used
- [`regira.modules.template.json`](./regira.modules.template.json) — template for the committed consumer manifest
- [`consumer.copilot.stub.md`](./consumer.copilot.stub.md) — optional compatibility bridge for tools that require `.github/copilot-instructions.md`; not part of the normal one-file consumer flow
- [`consumer.bootstrap.template.md`](./consumer.bootstrap.template.md) — starter bootstrap for a consuming project
- [`tools/ai/README.md`](../tools/ai/README.md) — sync script usage, output layout, and versioning details for consumer projects

### Valid deep-reference suffixes for consumer manifests

When a consumer project's `regira.modules.json` includes a `references` section, use only these v1 suffix values:

- `setup`
- `examples`
- `signatures`
- `namespaces`

## Authoring Rules

When adding new instruction files:

1. Put shared setup here if more than one module needs it.
2. Keep module guides focused on routing, decision rules, pitfalls, and package-specific behavior.
3. Put examples, setup walkthroughs, and exact signatures in separate deep-reference files.
4. Do not duplicate the module catalog or project-template tables here.
