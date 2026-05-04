# My AI Memories
---

## Lessons Learned

- Keep downstream Regira adoption standalone-first: `AGENTS.md` alone must be enough to choose packages and guide code generation; `regira.modules.json` and synced `.github/instructions/regira/*.md` files are optional accelerators, not prerequisites.
- When changing the consumer bootstrap flow, validate with both Windows PowerShell and PowerShell 7. Avoid relying on PowerShell 7-only conveniences such as `ConvertFrom-Json -AsHashtable` or multi-argument `Join-Path` unless a fallback exists.
- Keep the consumer-facing docs centered on the one-file `AGENTS.md` workflow. Mention optional sync scripts and local caches only as secondary tooling, not as the default path.

