#!/usr/bin/env bash
# sync-consumer-instructions.sh
#
# Syncs Regira AI instruction files into a consumer project.
#
# Reads regira.modules.json, fetches the matching versioned snapshot from the
# Regira source repository (via a shallow sparse clone of the ai-v{aiVersion} tag),
# renders the bootstrap from consumer.bootstrap.template.md, and copies the
# selected module guides and deep-reference files.
# Writes .github/copilot-instructions.md for GitHub Copilot while leaving AGENTS.md as the canonical static bootstrap.
#
# Supports an optional --source override for local repository-root testing
# without fetching the pinned remote snapshot.
#
# COMPATIBILITY NOTE:
#   This script requires bash 4.0 or later (uses mapfile and ${var,,} lowercasing).
#   macOS ships with bash 3.2 (/bin/bash). Run with a Homebrew-installed bash
#   (brew install bash) or use the PowerShell equivalent instead:
#     pwsh tools/ai/sync-consumer-instructions.ps1
#
# Usage:
#   ./tools/ai/sync-consumer-instructions.sh [--manifest PATH] [--source PATH] [--dest PATH] [--force]
#
# Options:
#   --manifest PATH   Path to regira.modules.json (default: regira.modules.json)
#   --source   PATH   Local repository root; skips remote fetch when provided
#   --dest     PATH   Output root (default: .github)
#   --force          Refresh the cached remote snapshot before syncing
#
# Examples:
#   # Sync from the pinned remote tag
#   ./tools/ai/sync-consumer-instructions.sh
#
#
#   # Force a refresh of the cached remote snapshot
#   ./tools/ai/sync-consumer-instructions.sh --force
set -euo pipefail

# Require bash 4.0+ (mapfile and ${var,,} are not available in bash 3.x shipped with macOS)
if [[ "${BASH_VERSINFO[0]}" -lt 4 ]]; then
    echo "Error: bash 4.0 or later is required (found ${BASH_VERSION})." >&2
    echo "macOS ships with bash 3.2. Options:" >&2
    echo "  1. Install a newer bash: brew install bash" >&2
    echo "  2. Use the PowerShell equivalent: pwsh tools/ai/sync-consumer-instructions.ps1" >&2
    exit 1
fi

# ---------------------------------------------------------------------------
# Defaults
# ---------------------------------------------------------------------------
MANIFEST_PATH="regira.modules.json"
SOURCE_PATH=""
DEST=".github"
FORCE_REFRESH=0

# ---------------------------------------------------------------------------
# Parse arguments
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
    case "$1" in
        --manifest) MANIFEST_PATH="$2"; shift 2 ;;
        --source)   SOURCE_PATH="$2";   shift 2 ;;
        --dest)     DEST="$2";           shift 2 ;;
        --force)    FORCE_REFRESH=1;      shift ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
require_tool() {
    command -v "$1" >/dev/null 2>&1 || { echo "Required tool not found: $1" >&2; exit 1; }
}

require_tool git
require_tool python3  # used for JSON parsing (available on all modern systems)

get_project_template_summary() {
    local project_template="$1"

    if [[ -z "$project_template" ]]; then
        printf '%s' "No project template selected yet. Ask the user which project shape they need before changing scaffolding."
        return 0
    fi

    case "$project_template" in
        ConsoleWithLogging)
            printf '%s' "Script, batch job, or CLI utility with host-based configuration and Serilog logging."
            ;;
        BasicApi)
            printf '%s' "Standard hosted ASP.NET Core API with Minimal API or controllers and no authentication."
            ;;
        SelfHostingApi)
            printf '%s' "Lightweight self-hosted internal API or Windows Service without authentication."
            ;;
        SelfHostingApiWithAuth)
            printf '%s' "Self-hosted API with API key and/or JWT Bearer authentication."
            ;;
        *)
            printf "Custom project template '%s'. Keep setup guidance aligned with the existing project conventions." "$project_template"
            ;;
    esac
}

json_value() {
    # json_value <file> <key>  -- extract a top-level string from JSON
    python3 -c "import json,sys; d=json.load(open('$1', encoding='utf-8-sig')); print(d.get('$2',''))"
}

json_array() {
    # json_array <file> <key>  -- extract a top-level string array, one item per line
    python3 -c "
import json, sys
d = json.load(open('$1', encoding='utf-8-sig'))
for v in d.get('$2', []):
    print(v)
"
}

json_ref_keys() {
    # json_ref_keys <file>  -- print each 'module ref' pair from references object
    python3 -c "
import json, sys
d = json.load(open('$1', encoding='utf-8-sig'))
refs = d.get('references', {})
for module, suffixes in refs.items():
    for s in suffixes:
        print(module, s)
"
}

load_module_sources() {
    python3 - "$1" << 'PYEOF'
import json
import sys

with open(sys.argv[1], encoding='utf-8-sig') as handle:
    data = json.load(handle)

for module, config in data.items():
    print(f"{module}\t{config['sourcePath']}\t{config['baseName']}")
PYEOF
}

module_file_name() {
    local module="$1"
    local suffix="$2"
    local base_name="${MODULE_BASE_NAMES[$module]:-}"

    if [[ -z "$base_name" ]]; then
        return 1
    fi

    printf '%s.%s.md' "$base_name" "$suffix"
}

resolve_module_file() {
    local repo_root="$1"
    local module="$2"
    local suffix="$3"
    local source_path="${MODULE_SOURCE_PATHS[$module]:-}"
    local file_name

    if [[ -z "$source_path" ]]; then
        return 1
    fi

    file_name="$(module_file_name "$module" "$suffix")" || return 1

    local candidate="$repo_root/$source_path/$file_name"
    if [[ -f "$candidate" ]]; then
        echo "$candidate"
        return 0
    fi

    return 1
}

# ---------------------------------------------------------------------------
# 1. Read the manifest
# ---------------------------------------------------------------------------
if [[ ! -f "$MANIFEST_PATH" ]]; then
    echo "Manifest not found: $MANIFEST_PATH" >&2
    exit 1
fi

AI_VERSION="$(json_value "$MANIFEST_PATH" aiVersion | tr -d '\r')"
PROJECT_TEMPLATE="$(json_value "$MANIFEST_PATH" projectTemplate | tr -d '\r')"
mapfile -t MODULES < <(json_array "$MANIFEST_PATH" modules | tr -d '\r')
SHARED_GUIDE_FILES=("project.setup.md" "shared.setup.md")

echo "aiVersion : $AI_VERSION"
echo "template  : $PROJECT_TEMPLATE"
echo "modules   : ${MODULES[*]}"

# ---------------------------------------------------------------------------
# 2. Resolve the source repository root
# ---------------------------------------------------------------------------
if [[ -n "$SOURCE_PATH" ]]; then
    REPO_ROOT="$SOURCE_PATH"
    if [[ ! -d "$REPO_ROOT/ai" ]]; then
        echo "Local source path must be the Regira repository root: $REPO_ROOT" >&2
        exit 1
    fi
    echo "Using local source: $REPO_ROOT"
    if [[ "$FORCE_REFRESH" -eq 1 ]]; then
        echo "Ignoring --force because --source bypasses the cached remote snapshot."
    fi
else
    TAG="ai-v${AI_VERSION}"
    TMP_DIR="${TMPDIR:-/tmp}/regira-ai-${AI_VERSION}"
    REPO_URL="https://github.com/bverboven/Regira-Codebase.git"

    if [[ "$FORCE_REFRESH" -eq 1 && -d "$TMP_DIR" ]]; then
        echo "Refreshing cached snapshot: $TMP_DIR"
        rm -rf "$TMP_DIR"
    fi

    if [[ -d "$TMP_DIR" ]]; then
        echo "Reusing cached snapshot: $TMP_DIR"
    else
        echo "Fetching snapshot for tag $TAG ..."

        # Verify the tag exists before attempting a clone
        if [[ -z "$(git ls-remote --tags "$REPO_URL" "refs/tags/$TAG")" ]]; then
            echo "Tag '$TAG' not found in $REPO_URL. Update aiVersion in $MANIFEST_PATH." >&2
            exit 1
        fi

        git clone --depth 1 --filter=blob:none --sparse --branch "$TAG" "$REPO_URL" "$TMP_DIR"
        git -C "$TMP_DIR" sparse-checkout set ai
    fi

    REPO_ROOT="$TMP_DIR"
fi

AI_DIR="$REPO_ROOT/ai"
MODULE_SOURCES_FILE="$AI_DIR/module-sources.json"
if [[ ! -f "$MODULE_SOURCES_FILE" ]]; then
    echo "Module source mapping not found: $MODULE_SOURCES_FILE" >&2
    exit 1
fi

declare -A MODULE_SOURCE_PATHS=()
declare -A MODULE_BASE_NAMES=()
while IFS=$'\t' read -r module source_path base_name; do
    MODULE_SOURCE_PATHS["$module"]="$source_path"
    MODULE_BASE_NAMES["$module"]="$base_name"
done < <(load_module_sources "$MODULE_SOURCES_FILE" | tr -d '\r')

if [[ -z "$SOURCE_PATH" ]]; then
    declare -A UNIQUE_SOURCE_PATHS=()
    SPARSE_PATHS=("ai")
    for source_path in "${MODULE_SOURCE_PATHS[@]}"; do
        if [[ -z "${UNIQUE_SOURCE_PATHS[$source_path]:-}" ]]; then
            UNIQUE_SOURCE_PATHS["$source_path"]=1
            SPARSE_PATHS+=("$source_path")
        fi
    done
    git -C "$REPO_ROOT" sparse-checkout set "${SPARSE_PATHS[@]}"
fi

# ---------------------------------------------------------------------------
# 3. Render the bootstrap from the template
# ---------------------------------------------------------------------------
TEMPLATE_FILE="$AI_DIR/consumer.bootstrap.template.md"
if [[ ! -f "$TEMPLATE_FILE" ]]; then
    echo "Bootstrap template not found: $TEMPLATE_FILE" >&2
    exit 1
fi

# Build module list: one "- ModuleName" line per module
MODULE_LINES=""
for module in "${MODULES[@]}"; do
    MODULE_LINES="${MODULE_LINES}- ${module}"$'\n'
done
# Remove trailing newline so the substitution does not add a blank line
MODULE_LINES="${MODULE_LINES%$'\n'}"

GUIDE_LINES=""
for module in "${MODULES[@]}"; do
    if file_name="$(module_file_name "$module" "instructions")"; then
        GUIDE_LINES="${GUIDE_LINES}- ${module}: .github/instructions/regira/${file_name}"$'\n'
    else
        echo "Warning: module source mapping not found (skipping bootstrap entry): $module" >&2
    fi
done

while IFS=' ' read -r module ref; do
    if [[ -z "$module" || -z "$ref" ]]; then
        continue
    fi

    if file_name="$(module_file_name "$module" "$ref")"; then
        GUIDE_LINES="${GUIDE_LINES}- ${module} ${ref}: .github/instructions/regira/${file_name}"$'\n'
    else
        echo "Warning: module source mapping not found for deep reference (skipping bootstrap entry): ${module}.${ref}" >&2
    fi
done < <(json_ref_keys "$MANIFEST_PATH" | tr -d '\r')

GUIDE_LINES="${GUIDE_LINES%$'\n'}"
if [[ -z "$GUIDE_LINES" ]]; then
    GUIDE_LINES="- No Regira module guides selected."
fi

if [[ -z "$PROJECT_TEMPLATE" ]]; then
    RESOLVED_PROJECT_TEMPLATE="Not specified"
else
    RESOLVED_PROJECT_TEMPLATE="$PROJECT_TEMPLATE"
fi
PROJECT_TEMPLATE_SUMMARY="$(get_project_template_summary "$PROJECT_TEMPLATE")"

mkdir -p "$DEST"
BOOTSTRAP_OUT="$DEST/copilot-instructions.md"
# Write module lines to a temp file to avoid shell-quoting issues inside Python
PROJECT_TEMPLATE_TMP="$(mktemp -t project-template.XXXXXX)"
printf '%s' "$RESOLVED_PROJECT_TEMPLATE" > "$PROJECT_TEMPLATE_TMP"
PROJECT_TEMPLATE_SUMMARY_TMP="$(mktemp -t project-template-summary.XXXXXX)"
printf '%s' "$PROJECT_TEMPLATE_SUMMARY" > "$PROJECT_TEMPLATE_SUMMARY_TMP"
MODULES_TMP="$(mktemp -t modules.XXXXXX)"
printf '%s' "$MODULE_LINES" > "$MODULES_TMP"
GUIDES_TMP="$(mktemp -t guides.XXXXXX)"
printf '%s' "$GUIDE_LINES" > "$GUIDES_TMP"
python3 - "$TEMPLATE_FILE" "$PROJECT_TEMPLATE_TMP" "$PROJECT_TEMPLATE_SUMMARY_TMP" "$MODULES_TMP" "$GUIDES_TMP" "$BOOTSTRAP_OUT" << 'PYEOF'
import sys
template                 = open(sys.argv[1], encoding='utf-8').read()
project_template         = open(sys.argv[2], encoding='utf-8').read()
project_template_summary = open(sys.argv[3], encoding='utf-8').read()
module_lines             = open(sys.argv[4], encoding='utf-8').read()
guide_lines              = open(sys.argv[5], encoding='utf-8').read()
result = (
    template
    .replace('{{PROJECT_TEMPLATE}}', project_template)
    .replace('{{PROJECT_TEMPLATE_SUMMARY}}', project_template_summary)
    .replace('{{MODULES}}', module_lines)
    .replace('{{MODULE_GUIDES}}', guide_lines)
)
open(sys.argv[6], 'w', encoding='utf-8').write(result)
PYEOF
rm -f "$PROJECT_TEMPLATE_TMP"
rm -f "$PROJECT_TEMPLATE_SUMMARY_TMP"
rm -f "$MODULES_TMP"
rm -f "$GUIDES_TMP"
echo "Rendered bootstrap -> $BOOTSTRAP_OUT"

# ---------------------------------------------------------------------------
# 4. Copy module instruction guides
# ---------------------------------------------------------------------------
REGIRA_DIR="$DEST/instructions/regira"
mkdir -p "$REGIRA_DIR"

for shared_file in "${SHARED_GUIDE_FILES[@]}"; do
    src_file="$AI_DIR/$shared_file"
    if [[ -f "$src_file" ]]; then
        cp "$src_file" "$REGIRA_DIR/$shared_file"
        echo "Copied shared setup guide: $shared_file"
    else
        echo "Warning: shared setup guide not found (skipping): $shared_file" >&2
    fi
done

for module in "${MODULES[@]}"; do
    if ! file_name="$(module_file_name "$module" "instructions")"; then
        echo "Warning: module source mapping not found (skipping): $module" >&2
        continue
    fi

    if src_file="$(resolve_module_file "$REPO_ROOT" "$module" "instructions")"; then
        cp "$src_file" "$REGIRA_DIR/$file_name"
        echo "Copied module guide: $file_name"
    else
        echo "Warning: module guide not found (skipping): $module" >&2
    fi
done

# ---------------------------------------------------------------------------
# 5. Copy deep-reference files
# ---------------------------------------------------------------------------
while IFS=' ' read -r module ref; do
    if [[ -z "$module" || -z "$ref" ]]; then
        continue
    fi

    if ! file_name="$(module_file_name "$module" "$ref")"; then
        echo "Warning: module source mapping not found for deep reference (skipping): ${module}.${ref}" >&2
        continue
    fi

    if src_file="$(resolve_module_file "$REPO_ROOT" "$module" "$ref")"; then
        cp "$src_file" "$REGIRA_DIR/$file_name"
        echo "Copied deep reference: $file_name"
    else
        echo "Warning: deep reference not found (skipping): ${module}.${ref}" >&2
    fi
done < <(json_ref_keys "$MANIFEST_PATH" | tr -d '\r')

echo ""
echo "Sync complete."
