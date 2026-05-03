<#
.SYNOPSIS
    Syncs Regira AI instruction files into a consumer project.

.DESCRIPTION
    Reads regira.modules.json, fetches the matching versioned snapshot from the
    Regira source repository (via a shallow sparse clone of the ai-v{aiVersion} tag),
    renders the bootstrap from consumer.bootstrap.template.md, writes the root
    Copilot bootstrap, and copies the selected module guides and deep-reference
    files.

    Supports an optional -SourcePath override so maintainers working inside a
    checked-out source repo can use their local repository root directly.

.PARAMETER ManifestPath
    Path to regira.modules.json. Defaults to "regira.modules.json" in the current directory.

.PARAMETER SourcePath
    Optional path to the repository root.
    When provided, the remote fetch is skipped.

.PARAMETER Destination
    Root destination folder for the generated files. Defaults to ".github".

.PARAMETER Force
    Refresh the cached remote snapshot for the selected aiVersion before syncing.
    Use this when a tag was retagged during authoring or the temp cache became stale or corrupted.

.EXAMPLE
    # Sync from the pinned remote tag
    ./tools/ai/sync-consumer-instructions.ps1

.EXAMPLE
    # Use a local source checkout
    ./tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase

.EXAMPLE
    # Force a refresh of the cached remote snapshot
    ./tools/ai/sync-consumer-instructions.ps1 -Force
#>
param(
    [string]$ManifestPath = "regira.modules.json",
    [string]$SourcePath   = "",
    [string]$Destination  = ".github",
    [switch]$Force
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
# 2. Resolve the source repository root
# ---------------------------------------------------------------------------
if ($SourcePath) {
    $repoRoot = $SourcePath
    if (-not (Test-Path (Join-Path $repoRoot "ai"))) {
        Write-Error "Local source path must be the Regira repository root: $repoRoot"
        exit 1
    }
    Write-Host "Using local source: $repoRoot"
    if ($Force) {
        Write-Host "Ignoring -Force because -SourcePath bypasses the cached remote snapshot."
    }
} else {
    $tag    = "ai-v$aiVersion"
    $tmpDir = Join-Path ([System.IO.Path]::GetTempPath()) "regira-ai-$aiVersion"

    if ($Force -and (Test-Path $tmpDir)) {
        Write-Host "Refreshing cached snapshot: $tmpDir"
        Remove-Item $tmpDir -Recurse -Force
    }

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

    $repoRoot = $tmpDir
}

$aiDir = Join-Path $repoRoot "ai"
$moduleSourcesPath = Join-Path $aiDir "module-sources.json"
if (-not (Test-Path $moduleSourcesPath)) {
    Write-Error "Module source mapping not found: $moduleSourcesPath"
    exit 1
}

$moduleSources = Get-Content -Raw $moduleSourcesPath | ConvertFrom-Json -AsHashtable

if (-not $SourcePath) {
    $sparsePaths = @("ai") + @(
        $moduleSources.Values |
            ForEach-Object { $_["sourcePath"] } |
            Sort-Object -Unique
    )
    git -C $repoRoot sparse-checkout set $sparsePaths
}

# ---------------------------------------------------------------------------
# Helpers: resolve module source metadata and file paths
# ---------------------------------------------------------------------------
function Get-ModuleSource {
    param([string]$Module)

    if ($moduleSources.ContainsKey($Module)) {
        return $moduleSources[$Module]
    }

    return $null
}

function Get-ModuleFileName {
    param([string]$Module, [string]$Suffix)

    $moduleSource = Get-ModuleSource -Module $Module
    if (-not $moduleSource) {
        return $null
    }

    return "$($moduleSource["baseName"]).$Suffix.md"
}

function Resolve-ModuleFile {
    param([string]$Module, [string]$Suffix)

    $moduleSource = Get-ModuleSource -Module $Module
    if (-not $moduleSource) {
        return $null
    }

    $fileName = Get-ModuleFileName -Module $Module -Suffix $Suffix
    $candidate = Join-Path $repoRoot $moduleSource["sourcePath"]
    $candidate = Join-Path $candidate $fileName

    if (Test-Path $candidate) {
        return [ordered]@{
            FileName = $fileName
            FilePath = $candidate
        }
    }

    return $null
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
$guideLines  = @()

foreach ($module in $modules) {
    $fileName = Get-ModuleFileName -Module $module -Suffix "instructions"
    if ($fileName) {
        $guideLines += "- ${module}: .github/instructions/regira/$fileName"
    } else {
        Write-Warning "Module source mapping not found (skipping bootstrap entry): $module"
    }
}

if ($references) {
    foreach ($prop in $references.PSObject.Properties) {
        $module = $prop.Name
        foreach ($ref in @($prop.Value)) {
            $fileName = Get-ModuleFileName -Module $module -Suffix $ref
            if ($fileName) {
                $guideLines += "- ${module} ${ref}: .github/instructions/regira/$fileName"
            } else {
                Write-Warning "Module source mapping not found for deep reference (skipping bootstrap entry): $module.$ref"
            }
        }
    }
}

$moduleGuideLines = if ($guideLines.Count -gt 0) {
    $guideLines -join "`n"
} else {
    "- No Regira module guides selected."
}

$bootstrap = $template -replace [regex]::Escape("{{MODULES}}"), $moduleLines
$bootstrap = $bootstrap -replace [regex]::Escape("{{MODULE_GUIDES}}"), $moduleGuideLines

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
    $moduleFile = Resolve-ModuleFile -Module $module -Suffix "instructions"
    if ($moduleFile) {
        $dest = Join-Path $regiraDir $moduleFile["FileName"]
        Copy-Item $moduleFile["FilePath"] -Destination $dest -Force
        Write-Host "Copied module guide: $($moduleFile["FileName"])"
    } else {
        Write-Warning "Module guide not found (skipping): $module"
    }
}

# ---------------------------------------------------------------------------
# 5. Copy deep-reference files
# ---------------------------------------------------------------------------
if ($references) {
    foreach ($prop in $references.PSObject.Properties) {
        $module = $prop.Name
        foreach ($ref in @($prop.Value)) {
            $moduleFile = Resolve-ModuleFile -Module $module -Suffix $ref
            if ($moduleFile) {
                $dest = Join-Path $regiraDir $moduleFile["FileName"]
                Copy-Item $moduleFile["FilePath"] -Destination $dest -Force
                Write-Host "Copied deep reference: $($moduleFile["FileName"])"
            } else {
                Write-Warning "Deep reference not found (skipping): $module.$ref"
            }
        }
    }
}

Write-Host ""
Write-Host "Sync complete."
