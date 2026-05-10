# Regira Source Repository — Agent Guide

This file is for AI agents working **on** the Regira source codebase (adding modules, updating guides, fixing bugs, writing tests). It is not for consumer projects — see `ai/AGENTS.md` for the consumer bootstrap.

---

## What this repository is

A collection of .NET NuGet packages published to `https://packages.regira.com`. Each package:
- Contains source code under `src/{ModuleName}/`
- Embeds AI instruction files in `src/{ModuleName}/ai/`
- Ships an MSBuild `.targets` file in `src/{ModuleName}/build/` that extracts those AI files into consumer projects on `dotnet build`

The `ai/` folder at the repo root contains the consumer-facing bootstrap guide (`ai/AGENTS.md`) and a learnings log (`ai/learnings.md`). These are not source-repo contributor guides.

---

## Source layout

```
src/
  Common.Setup/          # Shared project templates and setup guides
    ai/                  # project.setup.md, shared.setup.md, CLAUDE.md, copilot-instructions.md (consumer-facing)
    build/               # Regira.Setup.targets
  Common.Entities/       # CRUD entity framework
    ai/                  # entities.instructions.md, entities.signatures.md, entities-agent.md, ...
    build/               # Regira.Entities.targets
  Common.Office/         # Office operations (PDF, Excel, Word, Mail, ...)
    ai/                  # office.instructions.md, office-agent.md, per-submodule guides, ...
    build/               # Regira.Office.targets
  Common.{Module}/       # Other modules follow the same pattern
ai/
  AGENTS.md              # Consumer bootstrap guide (do not use for source work)
  learnings.md           # Durable lessons from source-repo work — read this before starting
  README.md              # Overview of the AI strategy
.claude/
  commands/              # Slash commands (/new-entity, /new-project, /sync-guides, /update-guide, /evaluate)
  settings.json
```

---

## Working on a module

### Reading the right guides

Each module's `src/{Module}/ai/` folder contains the authoritative reference for that module's design. Read the relevant `*.instructions.md` before touching a module's source. Use `*.signatures.md` and `*.examples.md` for exact API detail.

Read `ai/learnings.md` before starting any substantial work. Update it when a task reveals a durable lesson.

### Adding a new module

1. Create `src/{ModuleName}/` with a `.csproj`, source files, `build/`, and `ai/`
2. Write the AI guides in `src/{ModuleName}/ai/` — at minimum `{module}.instructions.md` and `{module}.examples.md`
3. Create `src/{ModuleName}/build/Regira.{ModuleName}.targets` following the pattern in any existing `.targets` file
4. Add the targets file and AI files to the `.csproj` under `buildTransitive\` and `contentFiles\any\any\ai\` respectively
5. Add the module to the routing tables in `ai/AGENTS.md` and `src/Common.Setup/ai/copilot-instructions.md`

### Updating AI guides

Use the `/update-guide` slash command to identify what changed and propose a guide patch. For small notes and pitfalls that don't warrant a guide section, add a row to `ai/learnings.md`.

### Agent files

Modules that warrant a subagent (currently Entities and Office) ship an `{module}-agent.md` alongside their other AI files. This file is extracted to `.claude/agents/` in the consumer project, not used in the source repo.

---

## Slash commands

| Command | Purpose |
|---|---|
| `/new-entity` | Scaffold a full Regira entity in a consumer project |
| `/new-project` | Bootstrap a new consumer project |
| `/sync-guides` | Refresh stale extracted guides in a consumer project |
| `/update-guide` | Propose a guide patch after a source code change |
| `/evaluate` | Run a structured quality evaluation on a module |

---

## Key conventions

- Guides travel with packages — every public API change that affects usage patterns needs a corresponding guide update
- Never add consumer-scaffolding content to this file; it belongs in `ai/AGENTS.md`
- Keep `Program.cs` thin and use `IServiceCollection` extension methods
- Prefer abstractions over concrete types in cross-module dependencies
