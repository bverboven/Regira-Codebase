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
- **Consumer project**: keep only a tiny local bootstrap plus the module guides that the app actually uses. Do not copy the entire `ai/` folder by default.

## Consumer-Project References

- [`consumer.bootstrap.template.md`](./consumer.bootstrap.template.md) — starter bootstrap for a consuming project
- [`consumer-project.migration-plan.md`](./consumer-project.migration-plan.md) — phased migration plan toward a scalable consumer-project model

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
