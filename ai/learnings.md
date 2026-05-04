# My AI Memories
---

## Lessons Learned

- Keep downstream Regira adoption standalone-first: `AGENTS.md` alone must be enough to choose packages and guide code generation; `regira.modules.json` and synced `.github/instructions/regira/*.md` files are optional accelerators, not prerequisites.
- When changing the consumer bootstrap flow, validate with both Windows PowerShell and PowerShell 7. Avoid relying on PowerShell 7-only conveniences such as `ConvertFrom-Json -AsHashtable` or multi-argument `Join-Path` unless a fallback exists.
- Keep the consumer-facing docs centered on the one-file `AGENTS.md` workflow. Mention optional sync scripts and local caches only as secondary tooling, not as the default path.
- Keep AI-only guidance files narrowly scoped: `regira.capabilities.md` routes to the right guide, `project.setup.md` owns scaffolding and app-shape defaults, and `shared.setup.md` owns cross-module setup and sync mechanics. Avoid repeating the same consumer-flow framing across all three.
- For default SQLite starter projects in Regira Entities guidance, prefer `Database.EnsureCreated()` and a disposable local database over introducing EF migration files. Treat migration-based schema management as an explicit later-stage choice.

