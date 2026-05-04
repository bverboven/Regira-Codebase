# Shared Setup for Regira AI Instructions

Use this file for setup guidance that applies across multiple Regira modules. Module guides should reference this file instead of repeating the same shared setup blocks.

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

## Source Repository vs Consumer Project

- **Source repository**: the full `ai/` folder is available locally, so the bootstrap can route to module guides and deep references.
- **Consumer project**: the primary entrypoint is a single `.github/AGENTS.md` file based on [`AGENTS.md`](./AGENTS.md). The agent chooses the project template, package set, and code changes directly from that file.

For consumer projects, keep these responsibilities separate:

- `.github/AGENTS.md` is the AI-facing bootstrap that decides project template, Regira modules, package routing, and when extracted local guides must be read before code generation.
- `regira.modules.json` is an optional machine-readable manifest that pins `aiVersion`, records `projectTemplate`, and selects synced modules and deep references.
- `.github/copilot-instructions.md` and `.github/instructions/regira/*.md` are optional local generated outputs. The sync can create them, and installed Regira packages can also extract `.github/instructions/regira/*.md` during build when those packages ship AI files. When these files exist, they provide local shared setup guidance (`project.setup.md`, `shared.setup.md`) plus module-specific docs.

Consumers do not need source-repository files for the normal flow. Use the optional sync tooling only when a team explicitly wants local cached shared setup and module instruction files.

## Consumer-Project References

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

## Guidance for New Shared Content

When adding new instruction files:

1. Put shared setup here if more than one module needs it.
2. Keep module guides focused on routing, decision rules, pitfalls, and package-specific behavior.
3. Put examples, setup walkthroughs, and exact signatures in separate deep-reference files.
