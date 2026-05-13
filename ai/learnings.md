# My AI Memories
---

## Lessons Learned

### NuGet AI file packaging

- Pack AI instruction files under `ai\` (package root), **not** `contentFiles\any\any\ai\`. The `contentFiles` convention causes NuGet to copy files into the consuming project and add them as `<None>` project items — which is undesirable for AI guides.
- Each module ships both a `.props` and a `.targets` file under `buildTransitive\`:
  - `.props` — sets `DefaultItemExcludes` to exclude `.regira\**` and `.claude\**` before SDK globs run, preventing extracted files from appearing as project items.
  - `.targets` — copies AI files to `.regira\instructions\` (and agent files to `.claude\agents\`, command files to `.claude\commands\`) on first build.
- The destination folder for extracted instruction files is `.regira\instructions\` (dot-prefixed), not `regira\instructions\`.
