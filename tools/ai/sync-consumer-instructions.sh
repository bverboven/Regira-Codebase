#!/usr/bin/env bash
# sync-consumer-instructions.sh
#
# Syncs Regira AI instruction files into a consumer project.
#
# Reads regira.modules.json, fetches the matching versioned snapshot from the
# Regira source repository (via a shallow sparse clone of the ai-v{aiVersion} tag),
# renders the bootstrap from consumer.bootstrap.template.md, and copies the
# selected module guides and deep-reference files.
#
# Supports an optional --source override so maintainers working inside a
# checked-out source repo can use their local ai/ folder directly.
#
# COMPATIBILITY NOTE:
#   This script requires bash 4.0 or later (uses mapfile and ${var,,} lowercasing).
#   macOS ships with bash 3.2 (/bin/bash). Run with a Homebrew-installed bash
#   (brew install bash) or use the PowerShell equivalent instead:
#     pwsh tools/ai/sync-consumer-instructions.ps1
#
# Usage:
#   ./tools/ai/sync-consumer-instructions.sh [--manifest PATH] [--source PATH] [--dest PATH]
#
# Options:
#   --manifest PATH   Path to regira.modules.json (default: regira.modules.json)
#   --source   PATH   Local ai/ directory; skips remote fetch when provided
#   --dest     PATH   Output root (default: .github)
#
# Examples:
#   # Sync from the pinned remote tag
#   ./tools/ai/sync-consumer-instructions.sh
#
#   # Use a local source checkout
#   ./tools/ai/sync-consumer-instructions.sh --source ../Regira-Codebase/ai

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

# ---------------------------------------------------------------------------
# Parse arguments
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
    case "$1" in
        --manifest) MANIFEST_PATH="$2"; shift 2 ;;
        --source)   SOURCE_PATH="$2";   shift 2 ;;
        --dest)     DEST="$2";           shift 2 ;;
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

json_value() {
    # json_value <file> <key>  — extract a top-level string from JSON
    python3 -c "import json,sys; d=json.load(open('$1')); print(d.get('$2',''))"
}

json_array() {
    # json_array <file> <key>  — extract a top-level string array, one item per line
    python3 -c "
import json, sys
d = json.load(open('$1'))
for v in d.get('$2', []):
    print(v)
"
}

json_ref_keys() {
    # json_ref_keys <file>  — print each 'module ref' pair from references object
    python3 -c "
import json, sys
d = json.load(open('$1'))
refs = d.get('references', {})
for module, suffixes in refs.items():
    for s in suffixes:
        print(module, s)
"
}

# ---------------------------------------------------------------------------
# 1. Read the manifest
# ---------------------------------------------------------------------------
if [[ ! -f "$MANIFEST_PATH" ]]; then
    echo "Manifest not found: $MANIFEST_PATH" >&2
    exit 1
fi

AI_VERSION="$(json_value "$MANIFEST_PATH" aiVersion)"
mapfile -t MODULES < <(json_array "$MANIFEST_PATH" modules)

echo "aiVersion : $AI_VERSION"
echo "modules   : ${MODULES[*]}"

# ---------------------------------------------------------------------------
# 2. Resolve the ai/ source directory
# ---------------------------------------------------------------------------
if [[ -n "$SOURCE_PATH" ]]; then
    AI_DIR="$SOURCE_PATH"
    if [[ ! -d "$AI_DIR" ]]; then
        echo "Local source path not found: $AI_DIR" >&2
        exit 1
    fi
    echo "Using local source: $AI_DIR"
else
    TAG="ai-v${AI_VERSION}"
    TMP_DIR="${TMPDIR:-/tmp}/regira-ai-${AI_VERSION}"
    REPO_URL="https://github.com/bverboven/Regira-Codebase.git"

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

    AI_DIR="$TMP_DIR/ai"
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

mkdir -p "$DEST"
BOOTSTRAP_OUT="$DEST/copilot-instructions.md"
# Write module lines to a temp file to avoid shell-quoting issues inside Python
MODULES_TMP="$(mktemp -t modules.XXXXXX)"
printf '%s' "$MODULE_LINES" > "$MODULES_TMP"
python3 - "$TEMPLATE_FILE" "$MODULES_TMP" "$BOOTSTRAP_OUT" << 'PYEOF'
import sys
template     = open(sys.argv[1], encoding='utf-8').read()
module_lines = open(sys.argv[2], encoding='utf-8').read()
result       = template.replace('{{MODULES}}', module_lines)
open(sys.argv[3], 'w', encoding='utf-8').write(result)
PYEOF
rm -f "$MODULES_TMP"
echo "Rendered bootstrap -> $BOOTSTRAP_OUT"

AGENTS_OUT="AGENTS.md"
cp "$BOOTSTRAP_OUT" "$AGENTS_OUT"
echo "Rendered bootstrap -> $AGENTS_OUT"

# ---------------------------------------------------------------------------
# 4. Copy module instruction guides
# ---------------------------------------------------------------------------
REGIRA_DIR="$DEST/instructions/regira"
mkdir -p "$REGIRA_DIR"

for module in "${MODULES[@]}"; do
    file_name="${module,,}.instructions.md"
    src_file="$AI_DIR/$file_name"
    if [[ -f "$src_file" ]]; then
        cp "$src_file" "$REGIRA_DIR/$file_name"
        echo "Copied module guide: $file_name"
    else
        echo "Warning: module guide not found (skipping): $file_name" >&2
    fi
done

# ---------------------------------------------------------------------------
# 5. Copy deep-reference files
# ---------------------------------------------------------------------------
while IFS=' ' read -r module ref; do
    file_name="${module,,}.${ref}.md"
    src_file="$AI_DIR/$file_name"
    if [[ -f "$src_file" ]]; then
        cp "$src_file" "$REGIRA_DIR/$file_name"
        echo "Copied deep reference: $file_name"
    else
        echo "Warning: deep reference not found (skipping): $file_name" >&2
    fi
done < <(json_ref_keys "$MANIFEST_PATH")

echo ""
echo "Sync complete."
