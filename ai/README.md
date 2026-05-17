# AI Implementation Overview

## Overview: AI-Guided Code Generation for the Regira Framework

The strategy is to **ship AI guidance alongside the NuGet packages themselves**, so that any AI agent (Claude, Copilot, etc.) working in a consumer project automatically gets the right context for that project's specific package set.

---

## The Two-Layer Architecture

### Layer 1 — Source repository (`ai/` and `src/*/ai/`)

The root `ai/AGENTS.md` is the top-level bootstrap and routing guide. Every source module also carries its own `ai/` folder with two types of files:

| File pattern | Purpose |
|---|---|
| `*.instructions.md` | Primary guide — read in full before writing code in that area |
| `*.examples.md` | Deep reference — consulted surgically for specific usage patterns |
| `*.setup.md` | Setup details (e.g., DI registration, configuration) |
| `*.signatures.md` | API signatures for exact method/type names |
| `*.namespaces.md` | Namespace reference |

Guide files exist across modules: `Entities`, `IO.Storage`, `Office.*`, `Security`, `Web`, `Invoicing`, `Payments`, `Media`, `System`, `TreeList`.

### Layer 2 — Consumer projects (`.regira/instructions/`)

When a consumer project runs `dotnet build`, package `.targets` files **extract the embedded `ai/*.md` files** from the installed NuGet packages and copy them into `.regira/instructions/` at the solution root. This means the AI agent in the consumer project sees only the guides relevant to the packages actually installed — not the entire Regira source tree.

---

## The Core Strategy

### Narrowest-fit guidance

The agent loads only the most specific guide that covers the current task, not the whole `ai/` folder. The loading hierarchy is:

1. `project.setup.md` — for scaffolding or app-shape changes
2. `shared.setup.md` — for cross-module conventions
3. Matching `*.instructions.md` — for module-specific work
4. Deep references (`*.examples.md`, etc.) — only when needed for exact details

### Template-driven scaffolding

Four project templates (`ConsoleWithLogging`, `BasicApi`, `SelfHostingApi`, `SelfHostingApiWithAuth`) ensure generated code lands in the right architectural shape before any module code is written.

### Pre-flight → checkpoint → code

The workflow enforces a clear gate: install packages and build first so guides are extracted, then pause at a new-project checkpoint to confirm with the user, then generate application code using the extracted guides.

---

## `learnings.md`

A living memory file — intended to capture durable lessons discovered during work on the source repo itself. The agent reads it before starting substantial work and updates it when a task reveals something worth preserving.

---

## Summary

AI guides are first-class artifacts embedded in NuGet packages. The build system extracts them into consumer projects, giving the AI agent precisely the context it needs for that project's dependencies — without the agent needing to know the full Regira source tree.
