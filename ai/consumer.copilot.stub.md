# Regira Consumer Bootstrap

This repository uses `AGENTS.md` at the repository root as the canonical Regira consumer bootstrap.

Rules:
1. Read `AGENTS.md` at the repository root and follow it as the primary Regira setup guidance.
2. Prefer synced Regira guides under `.github/instructions/regira/` when they exist.
3. If `.github/instructions/regira/` is missing, incomplete, or stale relative to `regira.modules.json`, run or ask the user to run `pwsh tools/ai/sync-consumer-instructions.ps1` from the repository root. Use `-Force` if the cached remote snapshot may be stale or corrupted.
4. If `AGENTS.md` or `regira.modules.json` is missing, ask the user to run `pwsh tools/ai/sync-consumer-instructions.ps1 -Init` from the repository root. If the local script is missing, run the same command directly from a local Regira-Codebase checkout or vendor the script first.
5. If the sync script is not present locally, ask the user to vendor it from Regira-Codebase or run it directly from a local Regira-Codebase checkout.
6. Ask for feedback instead of guessing missing APIs or project-specific conventions.

After you copy this file to `.github/copilot-instructions.md`, the init flow or sync script replaces that destination with a rendered Copilot bootstrap that lists the exact guide paths for the selected modules.