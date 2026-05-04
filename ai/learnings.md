# My AI Memories
---

## Lessons Learned

- Keep downstream Regira adoption standalone-first: `AGENTS.md` alone must be enough to choose packages and guide code generation; extracted `.github/instructions/regira/*.md` files are optional accelerators, not prerequisites.
- When changing the consumer bootstrap flow, validate with both Windows PowerShell and PowerShell 7. Avoid relying on PowerShell 7-only conveniences such as `ConvertFrom-Json -AsHashtable` or multi-argument `Join-Path` unless a fallback exists.
- Keep the consumer-facing docs centered on the one-file `AGENTS.md` workflow plus package-based guide extraction. Do not reintroduce separate sync/bootstrap systems unless they add clear value.
- Keep AI-only guidance files narrowly scoped: `AGENTS.md` owns top-level routing, consumer bootstrap behavior, and fallback general rules; `project.setup.md` owns scaffolding and app-shape defaults; `shared.setup.md` owns cross-module setup and local guide extraction behavior. Avoid repeating the same broad guidance across all three.
- For default SQLite starter projects in Regira Entities guidance, prefer `Database.EnsureCreated()` and a disposable local database over introducing EF migration files. Treat migration-based schema management as an explicit later-stage choice.
- When consumer evaluations expose avoidable mistakes, prefer instruction hardening at the decision point: make Scalar-versus-Swagger rules explicit in the bootstrap/template docs, and surface common Regira Entities pitfalls such as `[NotMapped]` query filters, `AddMapping<TSource, TTarget>()`, and seeding behavior earlier in the workflow.

