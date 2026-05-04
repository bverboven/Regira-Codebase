<#
.SYNOPSIS
    Bootstraps and syncs Regira AI instruction files into a consumer project.

.DESCRIPTION
    Reads regira.modules.json, fetches the matching versioned snapshot from the
    Regira source repository (via a shallow sparse clone of the ai-v{aiVersion} tag),
    renders the bootstrap from consumer.bootstrap.template.md, writes the root
    Copilot bootstrap, and copies the selected module guides and deep-reference
    files.

    Use -Init for a first-run interactive bootstrap that can create or update
    NuGet.Config, write regira.modules.json, copy .github/AGENTS.md, optionally vendor
    this PowerShell script into the consumer repository, and then continue with
    the normal sync.

    Supports an optional -SourcePath override for local repository-root testing
    without fetching the pinned remote snapshot.

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

.PARAMETER Init
    Interactive bootstrap for a new or partially configured consumer repository.
    Prompts for aiVersion, projectTemplate, modules, and deep references before
    running the normal sync flow.

.EXAMPLE
    # Sync from the pinned remote tag
    ./tools/ai/sync-consumer-instructions.ps1

.EXAMPLE
    # Use a local source checkout
    ./tools/ai/sync-consumer-instructions.ps1 -SourcePath ../Regira-Codebase

.EXAMPLE
    # Force a refresh of the cached remote snapshot
    ./tools/ai/sync-consumer-instructions.ps1 -Force

.EXAMPLE
    # Interactive bootstrap for a new consumer repo, then sync
    ./tools/ai/sync-consumer-instructions.ps1 -Init
#>
param(
    [string]$ManifestPath = "regira.modules.json",
    [string]$SourcePath   = "",
    [string]$Destination  = ".github",
    [switch]$Force,
    [switch]$Init
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoUrl = "https://github.com/bverboven/Regira-Codebase.git"
$nugetFeedUrl = "https://api.nuget.org/v3/index.json"
$regiraFeedUrl = "https://packages.regira.com/v3/index.json"
$deepReferenceSuffixes = @("setup", "examples", "signatures", "namespaces")
$projectTemplateDescriptions = [ordered]@{
    ConsoleWithLogging = "Script, batch job, or CLI utility with host-based configuration and Serilog logging."
    BasicApi = "Standard hosted ASP.NET Core API with Minimal API or controllers and no authentication."
    SelfHostingApi = "Lightweight self-hosted internal API or Windows Service without authentication."
    SelfHostingApiWithAuth = "Self-hosted API with API key and/or JWT Bearer authentication."
}

function Get-AbsolutePath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Get-ExistingManifest {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return $null
    }

    return Get-Content -Raw $Path | ConvertFrom-Json
}

function Get-ManifestReferences {
    param([object]$Manifest)

    if (-not $Manifest) {
        return $null
    }

    $referencesProperty = $Manifest.PSObject.Properties["references"]
    if (-not $referencesProperty) {
        return $null
    }

    return $referencesProperty.Value
}

function Get-LocalTemplateDefaults {
    if (-not $PSScriptRoot) {
        return $null
    }

    $candidateRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
    $templatePath = Join-Path $candidateRoot "ai\regira.modules.template.json"
    if (-not (Test-Path $templatePath)) {
        return $null
    }

    return Get-Content -Raw $templatePath | ConvertFrom-Json
}

function ConvertTo-Hashtable {
    param([object]$InputObject)

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $dictionary = @{}
        foreach ($key in $InputObject.Keys) {
            $dictionary[$key] = ConvertTo-Hashtable -InputObject $InputObject[$key]
        }

        return $dictionary
    }

    if (($InputObject -is [System.Collections.IEnumerable]) -and -not ($InputObject -is [string])) {
        return @($InputObject | ForEach-Object { ConvertTo-Hashtable -InputObject $_ })
    }

    $properties = @($InputObject.PSObject.Properties)
    if ($InputObject -is [psobject] -and $properties.Count -gt 0) {
        $dictionary = @{}
        foreach ($property in $properties) {
            $dictionary[$property.Name] = ConvertTo-Hashtable -InputObject $property.Value
        }

        return $dictionary
    }

    return $InputObject
}

function Read-JsonAsHashtable {
    param([string]$Path)

    $json = Get-Content -Raw $Path
    $convertFromJson = Get-Command ConvertFrom-Json
    if ($convertFromJson.Parameters.ContainsKey("AsHashtable")) {
        return $json | ConvertFrom-Json -AsHashtable
    }

    return ConvertTo-Hashtable -InputObject ($json | ConvertFrom-Json)
}

function Get-ProjectTemplateSummary {
    param([string]$ProjectTemplate)

    if ([string]::IsNullOrWhiteSpace($ProjectTemplate)) {
        return "No project template selected yet. Ask the user which project shape they need before changing scaffolding."
    }

    if ($projectTemplateDescriptions.Contains($ProjectTemplate)) {
        return $projectTemplateDescriptions[$ProjectTemplate]
    }

    return "Custom project template '$ProjectTemplate'. Keep setup guidance aligned with the existing project conventions."
}

function Prompt-Input {
    param(
        [string]$Label,
        [string]$Default = ""
    )

    $prompt = if ([string]::IsNullOrWhiteSpace($Default)) {
        $Label
    } else {
        "$Label [$Default]"
    }

    $value = Read-Host $prompt
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    }

    return $value.Trim()
}

function Prompt-RequiredInput {
    param(
        [string]$Label,
        [string]$Default = ""
    )

    while ($true) {
        $value = Prompt-Input -Label $Label -Default $Default
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }

        Write-Warning "$Label is required."
    }
}

function Prompt-YesNo {
    param(
        [string]$Label,
        [bool]$Default = $true
    )

    $defaultHint = if ($Default) { "Y/n" } else { "y/N" }

    while ($true) {
        $response = Read-Host "$Label [$defaultHint]"
        if ([string]::IsNullOrWhiteSpace($response)) {
            return $Default
        }

        switch -Regex ($response.Trim()) {
            '^(y|yes)$' { return $true }
            '^(n|no)$' { return $false }
            default { Write-Warning "Enter yes or no." }
        }
    }
}

function Split-CommaValues {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return @()
    }

    return @(
        $Value.Split(',') |
            ForEach-Object { $_.Trim() } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Resolve-SelectionValues {
    param(
        [string[]]$Available,
        [string]$InputValue,
        [string[]]$Default = @()
    )

    $tokens = @(Split-CommaValues -Value $InputValue)
    if ($tokens.Count -eq 0) {
        return @($Default)
    }

    $selected = New-Object 'System.Collections.Generic.List[string]'
    foreach ($token in $tokens) {
        if ($token -match '^[0-9]+$') {
            $index = [int]$token
            if ($index -lt 1 -or $index -gt $Available.Count) {
                throw "Selection '$token' is outside the available module list."
            }

            $value = $Available[$index - 1]
        } else {
            $match = @($Available | Where-Object { $_ -ieq $token })
            if ($match.Count -ne 1) {
                throw "Unknown selection '$token'. Use an exact module name or list number."
            }

            $value = $match[0]
        }

        if (-not $selected.Contains($value)) {
            $selected.Add($value)
        }
    }

    return $selected.ToArray()
}

function Prompt-ModuleSelection {
    param(
        [string[]]$AvailableModules,
        [string[]]$DefaultModules = @()
    )

    Write-Host "Available modules:"
    for ($i = 0; $i -lt $AvailableModules.Count; $i++) {
        Write-Host ("  [{0}] {1}" -f ($i + 1), $AvailableModules[$i])
    }

    $defaultLabel = if ($DefaultModules.Count -gt 0) {
        $DefaultModules -join ", "
    } else {
        ""
    }

    while ($true) {
        $rawSelection = Prompt-Input -Label "Select modules by number or exact name (comma-separated)" -Default $defaultLabel
        try {
            $selectedModules = Resolve-SelectionValues -Available $AvailableModules -InputValue $rawSelection -Default $DefaultModules
            if ($selectedModules.Count -eq 0) {
                Write-Warning "Select at least one module."
                continue
            }

            return $selectedModules
        } catch {
            Write-Warning $_.Exception.Message
        }
    }
}

function Prompt-ReferenceSelection {
    param(
        [string]$Module,
        [string[]]$DefaultReferences = @()
    )

    $normalizedDefaultReferences = @($DefaultReferences | Where-Object { $null -ne $_ })

    $defaultLabel = if ($normalizedDefaultReferences.Count -gt 0) {
        $normalizedDefaultReferences -join ", "
    } else {
        ""
    }

    $prompt = if ($normalizedDefaultReferences.Count -gt 0) {
        "Deep references for $Module (comma-separated: setup, examples, signatures, namespaces; press Enter to reuse the current list; type none to clear)"
    } else {
        "Deep references for $Module (comma-separated: setup, examples, signatures, namespaces; blank for none)"
    }

    while ($true) {
        $rawSelection = Prompt-Input -Label $prompt -Default $defaultLabel
        if ($rawSelection -ieq "none") {
            return @()
        }

        $items = @(Split-CommaValues -Value $rawSelection)
        if ($items.Count -eq 0) {
            return @()
        }

        $selected = New-Object 'System.Collections.Generic.List[string]'
        $isValid = $true
        foreach ($item in $items) {
            $match = @($deepReferenceSuffixes | Where-Object { $_ -ieq $item })
            if ($match.Count -ne 1) {
                Write-Warning "Unknown deep reference '$item' for $Module. Use: $($deepReferenceSuffixes -join ', ')"
                $isValid = $false
                break
            }

            $value = $match[0]
            if (-not $selected.Contains($value)) {
                $selected.Add($value)
            }
        }

        if ($isValid) {
            return $selected.ToArray()
        }
    }
}

function Get-OrCreateXmlElement {
    param(
        [xml]$Xml,
        [System.Xml.XmlNode]$Parent,
        [string]$Name
    )

    $node = $Parent.SelectSingleNode($Name)
    if ($node) {
        return $node
    }

    $node = $Xml.CreateElement($Name)
    $null = $Parent.AppendChild($node)
    return $node
}

function Ensure-PackageSource {
    param(
        [xml]$Xml,
        [System.Xml.XmlNode]$PackageSourcesNode,
        [string]$Key,
        [string]$Value
    )

    $existingNodes = @($PackageSourcesNode.SelectNodes("add[@key='$Key']"))
    if ($existingNodes.Count -gt 0) {
        foreach ($node in $existingNodes) {
            $node.SetAttribute("value", $Value)
        }
        return
    }

    $newNode = $Xml.CreateElement("add")
    $newNode.SetAttribute("key", $Key)
    $newNode.SetAttribute("value", $Value)
    $null = $PackageSourcesNode.AppendChild($newNode)
}

function Ensure-NuGetConfig {
    param([string]$ConsumerRoot)

    $nugetConfigPath = Join-Path $ConsumerRoot "NuGet.Config"
    if (-not (Test-Path $nugetConfigPath)) {
        $content = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="NuGet" value="$nugetFeedUrl" />
    <add key="Regira" value="$regiraFeedUrl" />
  </packageSources>
</configuration>
"@

        [System.IO.File]::WriteAllText($nugetConfigPath, $content.TrimStart(), [System.Text.Encoding]::UTF8)
        Write-Host "Created NuGet.Config -> $nugetConfigPath"
        return
    }

    [xml]$xml = Get-Content -Raw $nugetConfigPath
    $configurationNode = $xml.SelectSingleNode("/configuration")
    if (-not $configurationNode) {
        $configurationNode = $xml.CreateElement("configuration")
        $null = $xml.AppendChild($configurationNode)
    }

    $packageSourcesNode = Get-OrCreateXmlElement -Xml $xml -Parent $configurationNode -Name "packageSources"

    # Keep updates conservative: add or correct the explicit feeds we need, but do not
    # inject <clear /> into an existing NuGet.Config and unexpectedly drop inherited sources.
    Ensure-PackageSource -Xml $xml -PackageSourcesNode $packageSourcesNode -Key "NuGet" -Value $nugetFeedUrl
    Ensure-PackageSource -Xml $xml -PackageSourcesNode $packageSourcesNode -Key "Regira" -Value $regiraFeedUrl
    $xml.Save($nugetConfigPath)

    Write-Host "Updated NuGet.Config -> $nugetConfigPath"
}

function Write-ConsumerManifest {
    param(
        [string]$Path,
        [string]$AiVersion,
        [string]$ProjectTemplate,
        [string[]]$Modules,
        [System.Collections.IDictionary]$References
    )

    $manifest = [ordered]@{
        aiVersion = $AiVersion
        projectTemplate = $ProjectTemplate
        modules = $Modules
    }

    if ($References.Count -gt 0) {
        $manifest.references = [ordered]@{}
        foreach ($module in $References.Keys) {
            $manifest.references[$module] = $References[$module]
        }
    }

    $json = ($manifest | ConvertTo-Json -Depth 8) + [Environment]::NewLine
    [System.IO.File]::WriteAllText($Path, $json, [System.Text.Encoding]::UTF8)
    Write-Host "Wrote consumer manifest -> $Path"
}

function Copy-FileWithPrompt {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [string]$Label,
        [bool]$OverwriteDefault = $false
    )

    $destinationDir = Split-Path -Parent $DestinationPath
    if ($destinationDir) {
        $null = New-Item -ItemType Directory -Force -Path $destinationDir
    }

    if (Test-Path $DestinationPath) {
        $shouldOverwrite = Prompt-YesNo -Label "$Label already exists at $DestinationPath. Overwrite?" -Default $OverwriteDefault
        if (-not $shouldOverwrite) {
            Write-Host "Keeping existing $Label -> $DestinationPath"
            return $false
        }
    }

    Copy-Item $SourcePath -Destination $DestinationPath -Force
    Write-Host "Copied $Label -> $DestinationPath"
    return $true
}

function Resolve-SourceRepositoryRoot {
    param(
        [string]$RequestedSourcePath,
        [string]$AiVersion,
        [bool]$RefreshCache
    )

    if ($RequestedSourcePath) {
        $repoRoot = $RequestedSourcePath
        if (-not (Test-Path (Join-Path $repoRoot "ai"))) {
            Write-Error "Local source path must be the Regira repository root: $repoRoot"
            exit 1
        }

        Write-Host "Using local source: $repoRoot"
        if ($RefreshCache) {
            Write-Host "Ignoring -Force because -SourcePath bypasses the cached remote snapshot."
        }

        return $repoRoot
    }

    $tag = "ai-v$AiVersion"
    $tmpDir = Join-Path ([System.IO.Path]::GetTempPath()) "regira-ai-$AiVersion"

    if ($RefreshCache -and (Test-Path $tmpDir)) {
        Write-Host "Refreshing cached snapshot: $tmpDir"
        Remove-Item $tmpDir -Recurse -Force
    }

    if (Test-Path $tmpDir) {
        Write-Host "Reusing cached snapshot: $tmpDir"
        return $tmpDir
    }

    Write-Host "Fetching snapshot for tag $tag ..."

    $tagCheck = git ls-remote --tags $repoUrl "refs/tags/$tag" 2>&1
    if (-not ($tagCheck -match "refs/tags/$tag")) {
        Write-Error "Tag '$tag' not found in $repoUrl. Update aiVersion in $ManifestPath."
        exit 1
    }

    git clone --depth 1 --filter=blob:none --sparse --branch $tag $repoUrl $tmpDir
    git -C $tmpDir sparse-checkout set ai

    return $tmpDir
}

function Get-ModuleDefaultsFromManifest {
    param([object]$Manifest)

    if (-not $Manifest -or -not $Manifest.modules) {
        return @()
    }

    return @($Manifest.modules)
}

function Get-ReferencesForModule {
    param(
        [object]$References,
        [string]$Module
    )

    if (-not $References) {
        return @()
    }

    $referenceProperty = $References.PSObject.Properties[$Module]
    if (-not $referenceProperty) {
        return @()
    }

    return @($referenceProperty.Value)
}

function Invoke-InteractiveBootstrap {
    param([string]$ResolvedManifestPath)

    $consumerRoot = Split-Path -Parent $ResolvedManifestPath
    $existingManifest = Get-ExistingManifest -Path $ResolvedManifestPath
    $existingReferences = Get-ManifestReferences -Manifest $existingManifest
    $localTemplateDefaults = Get-LocalTemplateDefaults

    $defaultAiVersion = ""
    if ($existingManifest -and $existingManifest.aiVersion) {
        $defaultAiVersion = $existingManifest.aiVersion
    } elseif ($localTemplateDefaults -and $localTemplateDefaults.aiVersion) {
        $defaultAiVersion = $localTemplateDefaults.aiVersion
    }

    $defaultProjectTemplate = "BasicApi"
    if ($localTemplateDefaults -and $localTemplateDefaults.projectTemplate) {
        $defaultProjectTemplate = $localTemplateDefaults.projectTemplate
    }
    if ($existingManifest -and $existingManifest.projectTemplate) {
        $defaultProjectTemplate = $existingManifest.projectTemplate
    }

    Write-Host "Initializing Regira consumer bootstrap in $consumerRoot"

    $aiVersion = Prompt-RequiredInput -Label "aiVersion" -Default $defaultAiVersion
    $repoRoot = Resolve-SourceRepositoryRoot -RequestedSourcePath $SourcePath -AiVersion $aiVersion -RefreshCache:$Force
    $aiDir = Join-Path $repoRoot "ai"
    $moduleSourcesPath = Join-Path $aiDir "module-sources.json"
    if (-not (Test-Path $moduleSourcesPath)) {
        Write-Error "Module source mapping not found: $moduleSourcesPath"
        exit 1
    }

    $moduleSources = Read-JsonAsHashtable -Path $moduleSourcesPath
    $availableModules = @($moduleSources.Keys | Sort-Object)
    $defaultModules = Get-ModuleDefaultsFromManifest -Manifest $existingManifest
    $projectTemplate = Prompt-RequiredInput -Label "projectTemplate" -Default $defaultProjectTemplate
    $selectedModules = Prompt-ModuleSelection -AvailableModules $availableModules -DefaultModules $defaultModules

    $selectedReferences = [ordered]@{}
    foreach ($module in $selectedModules) {
        $defaultReferences = Get-ReferencesForModule -References $existingReferences -Module $module
        $references = @(Prompt-ReferenceSelection -Module $module -DefaultReferences $defaultReferences)
        if ($references.Count -gt 0) {
            $selectedReferences[$module] = $references
        }
    }

    Ensure-NuGetConfig -ConsumerRoot $consumerRoot
    Write-ConsumerManifest -Path $ResolvedManifestPath -AiVersion $aiVersion -ProjectTemplate $projectTemplate -Modules $selectedModules -References $selectedReferences

    $agentsSource = Join-Path $aiDir "AGENTS.md"
    $agentsDestination = Join-Path $consumerRoot ".github\AGENTS.md"
    if (-not (Test-Path $agentsSource)) {
        Write-Error "Consumer AGENTS bootstrap not found: $agentsSource"
        exit 1
    }

    $null = Copy-FileWithPrompt -SourcePath $agentsSource -DestinationPath $agentsDestination -Label "AGENTS bootstrap" -OverwriteDefault:$false

    if ($PSCommandPath) {
        $currentScriptPath = Get-AbsolutePath -Path $PSCommandPath
        $vendoredScriptPath = Join-Path $consumerRoot "tools\ai\sync-consumer-instructions.ps1"
        if (-not [System.String]::Equals($currentScriptPath, $vendoredScriptPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            $shouldVendorScript = Prompt-YesNo -Label "Copy this PowerShell sync script into tools/ai/ in the consumer repository?" -Default $true
            if ($shouldVendorScript) {
                $null = Copy-FileWithPrompt -SourcePath $currentScriptPath -DestinationPath $vendoredScriptPath -Label "PowerShell sync script" -OverwriteDefault:$true
            }
        }
    }

    Write-Host "Bootstrap initialized. Continuing with the normal sync..."
}

# ---------------------------------------------------------------------------
# 1. Initialize the consumer repo when requested
# ---------------------------------------------------------------------------
$resolvedManifestPath = Get-AbsolutePath -Path $ManifestPath

if ($Init) {
    Invoke-InteractiveBootstrap -ResolvedManifestPath $resolvedManifestPath
}

# ---------------------------------------------------------------------------
# 2. Read the manifest
# ---------------------------------------------------------------------------
if (-not (Test-Path $resolvedManifestPath)) {
    Write-Error "Manifest not found: $ManifestPath"
    exit 1
}

$manifest    = Get-Content -Raw $resolvedManifestPath | ConvertFrom-Json
$aiVersion   = $manifest.aiVersion
$projectTemplate = if ($manifest.PSObject.Properties["projectTemplate"]) { [string]$manifest.projectTemplate } else { "" }
$modules     = @($manifest.modules)
$references  = Get-ManifestReferences -Manifest $manifest

Write-Host "aiVersion : $aiVersion"
Write-Host "template  : $projectTemplate"
Write-Host "modules   : $($modules -join ', ')"

# ---------------------------------------------------------------------------
# 3. Resolve the source repository root
# ---------------------------------------------------------------------------
$repoRoot = Resolve-SourceRepositoryRoot -RequestedSourcePath $SourcePath -AiVersion $aiVersion -RefreshCache:$Force

$aiDir = Join-Path $repoRoot "ai"
$moduleSourcesPath = Join-Path $aiDir "module-sources.json"
if (-not (Test-Path $moduleSourcesPath)) {
    Write-Error "Module source mapping not found: $moduleSourcesPath"
    exit 1
}

$moduleSources = Read-JsonAsHashtable -Path $moduleSourcesPath

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

$sharedGuideFiles = @(
    "project.setup.md",
    "shared.setup.md"
)

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

$resolvedProjectTemplate = if ([string]::IsNullOrWhiteSpace($projectTemplate)) {
    "Not specified"
} else {
    $projectTemplate
}
$projectTemplateSummary = Get-ProjectTemplateSummary -ProjectTemplate $projectTemplate

$bootstrap = $template -replace [regex]::Escape("{{PROJECT_TEMPLATE}}"), $resolvedProjectTemplate
$bootstrap = $bootstrap -replace [regex]::Escape("{{PROJECT_TEMPLATE_SUMMARY}}"), $projectTemplateSummary
$bootstrap = $bootstrap -replace [regex]::Escape("{{MODULES}}"), $moduleLines
$bootstrap = $bootstrap -replace [regex]::Escape("{{MODULE_GUIDES}}"), $moduleGuideLines

$destDir = $Destination
$null    = New-Item -ItemType Directory -Force -Path $destDir
$bootstrapOut = Join-Path $destDir "copilot-instructions.md"
[System.IO.File]::WriteAllText($bootstrapOut, $bootstrap, [System.Text.Encoding]::UTF8)
Write-Host "Rendered bootstrap -> $bootstrapOut"

# ---------------------------------------------------------------------------
# 4. Copy module instruction guides
# ---------------------------------------------------------------------------
$regiraDir = Join-Path (Join-Path $Destination "instructions") "regira"
$null      = New-Item -ItemType Directory -Force -Path $regiraDir

foreach ($sharedGuideFile in $sharedGuideFiles) {
    $source = Join-Path $aiDir $sharedGuideFile
    if (Test-Path $source) {
        $dest = Join-Path $regiraDir $sharedGuideFile
        Copy-Item $source -Destination $dest -Force
        Write-Host "Copied shared setup guide: $sharedGuideFile"
    } else {
        Write-Warning "Shared setup guide not found (skipping): $sharedGuideFile"
    }
}

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
