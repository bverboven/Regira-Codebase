<#
.SYNOPSIS
    Syncs Regira AI instruction files into a consumer project.

.DESCRIPTION
    Reads regira.modules.json, fetches the matching versioned snapshot from the
    Regira source repository (via a shallow sparse clone of the ai-v{aiVersion} tag),
    renders the bootstrap from consumer.bootstrap.template.md, and copies the
    selected module guides and deep-reference files.

    Supports an optional -SourcePath override so maintainers working inside a
    checked-out source repo can use their local ai/ folder directly.

.PARAMETER ManifestPath
    Path to regira.modules.json. Defaults to "regira.modules.json" in the current directory.

.PARAMETER SourcePath
    Optional path to a local ai/ directory. When provided, the remote fetch is skipped.

.PARAMETER Destination
    Root destination folder for the generated files. Defaults to ".github".

.EXAMPLE
    # Sync from the pinned remote tag
    ./tools/ai/sync-consumer-instructions.ps1

.EXAMPLE
    # Use a local source checkout
    ./tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase/ai
#>
param(
    [string]$ManifestPath = "regira.modules.json",
    [string]$SourcePath   = "",
    [string]$Destination  = ".github"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# 1. Read the manifest
# ---------------------------------------------------------------------------
if (-not (Test-Path $ManifestPath)) {
    Write-Error "Manifest not found: $ManifestPath"
    exit 1
}

$manifest    = Get-Content -Raw $ManifestPath | ConvertFrom-Json
$aiVersion   = $manifest.aiVersion
$modules     = @($manifest.modules)
$references  = $manifest.PSObject.Properties["references"]?.Value

Write-Host "aiVersion : $aiVersion"
Write-Host "modules   : $($modules -join ', ')"

# ---------------------------------------------------------------------------
# 2. Resolve the ai/ source directory
# ---------------------------------------------------------------------------
if ($SourcePath) {
    $aiDir = $SourcePath
    if (-not (Test-Path $aiDir)) {
        Write-Error "Local source path not found: $aiDir"
        exit 1
    }
    Write-Host "Using local source: $aiDir"
} else {
    $tag    = "ai-v$aiVersion"
    $tmpDir = Join-Path ([System.IO.Path]::GetTempPath()) "regira-ai-$aiVersion"

    if (Test-Path $tmpDir) {
        Write-Host "Reusing cached snapshot: $tmpDir"
    } else {
        Write-Host "Fetching snapshot for tag $tag ..."
        $repoUrl = "https://github.com/bverboven/Regira-Codebase.git"

        # Verify the tag exists before attempting a clone
        $tagCheck = git ls-remote --tags $repoUrl "refs/tags/$tag" 2>&1
        if (-not ($tagCheck -match "refs/tags/$tag")) {
            Write-Error "Tag '$tag' not found in $repoUrl. Update aiVersion in $ManifestPath."
            exit 1
        }

        git clone --depth 1 --filter=blob:none --sparse --branch $tag $repoUrl $tmpDir
        git -C $tmpDir sparse-checkout set ai
    }

    $aiDir = Join-Path $tmpDir "ai"
}

# ---------------------------------------------------------------------------
# 3. Render the bootstrap from the template
# ---------------------------------------------------------------------------
$templateFile = Join-Path $aiDir "consumer.bootstrap.template.md"
if (-not (Test-Path $templateFile)) {
    Write-Error "Bootstrap template not found: $templateFile"
    exit 1
}

$template    = Get-Content -Raw $templateFile
$moduleLines = ($modules | ForEach-Object { "- $_" }) -join "`n"
$bootstrap   = $template -replace [regex]::Escape("{{MODULES}}"), $moduleLines

$destDir = $Destination
$null    = New-Item -ItemType Directory -Force -Path $destDir
$bootstrapOut = Join-Path $destDir "copilot-instructions.md"
[System.IO.File]::WriteAllText($bootstrapOut, $bootstrap, [System.Text.Encoding]::UTF8)
Write-Host "Rendered bootstrap -> $bootstrapOut"

# ---------------------------------------------------------------------------
# 4. Copy module instruction guides
# ---------------------------------------------------------------------------
$regiraDir = Join-Path $Destination "instructions" "regira"
$null      = New-Item -ItemType Directory -Force -Path $regiraDir

foreach ($module in $modules) {
    $fileName = "$($module.ToLower()).instructions.md"
    $srcFile  = Join-Path $aiDir $fileName
    if (Test-Path $srcFile) {
        $dest = Join-Path $regiraDir $fileName
        Copy-Item $srcFile -Destination $dest -Force
        Write-Host "Copied module guide: $fileName"
    } else {
        Write-Warning "Module guide not found (skipping): $fileName"
    }
}

# ---------------------------------------------------------------------------
# 5. Copy deep-reference files
# ---------------------------------------------------------------------------
if ($references) {
    foreach ($prop in $references.PSObject.Properties) {
        $module = $prop.Name
        foreach ($ref in @($prop.Value)) {
            $fileName = "$($module.ToLower()).$ref.md"
            $srcFile  = Join-Path $aiDir $fileName
            if (Test-Path $srcFile) {
                $dest = Join-Path $regiraDir $fileName
                Copy-Item $srcFile -Destination $dest -Force
                Write-Host "Copied deep reference: $fileName"
            } else {
                Write-Warning "Deep reference not found (skipping): $fileName"
            }
        }
    }
}

Write-Host ""
Write-Host "Sync complete."
