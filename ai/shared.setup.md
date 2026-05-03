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
- **Consumer project**: prefer `pwsh tools/ai/sync-consumer-instructions.ps1 -Init` at repository root to create `NuGet.Config`, `regira.modules.json`, and the canonical `AGENTS.md` bootstrap, then let the same flow run the first sync. If the repository uses GitHub Copilot before the first sync and cannot run the init flow yet, mirror `consumer.copilot.stub.md` into `.github/copilot-instructions.md`. Sync only the module guides that the app actually uses with the repo-root script.

## Consumer-Project References

- [`regira.modules.template.json`](./regira.modules.template.json) — template for the committed consumer manifest
- [`consumer.agents.stub.md`](./consumer.agents.stub.md) — canonical downstream `AGENTS.md` bootstrap with module-selection guidance and package mapping
- [`consumer.copilot.stub.md`](./consumer.copilot.stub.md) — optional GitHub Copilot bridge that points to `AGENTS.md` before the first sync
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
