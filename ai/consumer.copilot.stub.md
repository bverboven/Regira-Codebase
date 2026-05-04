# Regira Consumer Copilot Bridge

Use this file only when a tool requires `.github/copilot-instructions.md` in addition to `.github/AGENTS.md`.

Rules:
1. Read `.github/AGENTS.md` and treat it as the authoritative consumer bootstrap.
2. If `regira.modules.json` or `.github/instructions/regira/` exists, treat them as extra local context, not as prerequisites.
3. Do not require sync scripts or source-repository files before selecting packages or generating code.
4. Ask for feedback instead of guessing missing APIs or project-specific conventions.

This file is optional. It exists only as a compatibility bridge to the canonical `.github/AGENTS.md` flow.