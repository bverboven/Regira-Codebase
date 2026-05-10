Bootstrap a new Regira consumer project from scratch. Follow these steps precisely and do not skip the checkpoint.

## Step 1 — Understand what is being built

If the user has not described the project, ask:
- What does this project do?
- Does it need authentication? (API key / JWT / none)
- Does it need to be deployable as a Windows Service?

## Step 2 — Choose the project template

Select exactly one template. Confirm the choice with the user before proceeding.

| Requirement | Template |
|---|---|
| Script, batch job, CLI utility | `ConsoleWithLogging` |
| Standard hosted API, no auth | `BasicApi` |
| Lightweight internal API, no auth | `SelfHostingApi` |
| Must run as a Windows Service | `SelfHostingApi` |
| API with API key or JWT auth | `SelfHostingApiWithAuth` |

## Step 3 — Select the minimum Regira package set

Inspect any existing `*.csproj` files first. Then choose the smallest package set that covers the user's stated needs. Use the package routing tables in `.github/instructions/regira/project.setup.md` if available, or the table in `.github/copilot-instructions.md`.

## Step 4 — Generate `NuGet.Config`

Add the Regira feed alongside `nuget.org`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="Regira" value="https://packages.regira.com/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

## Step 5 — Add packages and restore

Add the chosen `PackageReference` items to the `.csproj`. Then run:

```bash
dotnet restore
dotnet build
```

Report the outcome. If restore or build fails, diagnose and fix before continuing.

## Step 6 — Checkpoint (mandatory stop)

Stop here. Do not write any application code yet.

Report:
- Template chosen
- Packages added
- Whether restore and build succeeded
- Which guide files were extracted to `.github/instructions/regira/` (list them)

Then explicitly ask: **"Ready to continue and generate application code?"**

Only proceed to application code after the user confirms.

## Step 7 — Load extracted guides and generate code

Read every applicable primary guide in `.github/instructions/regira/` in full before writing any entity models, services, controllers, DI registrations, or infrastructure code. Skipping this step is a workflow violation.
