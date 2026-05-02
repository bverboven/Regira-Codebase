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
# Replace {{MODULES}} with the module list
python3 -c "
import sys
template = open('$TEMPLATE_FILE', encoding='utf-8').read()
result   = template.replace('{{MODULES}}', '''$MODULE_LINES''')
open('$BOOTSTRAP_OUT', 'w', encoding='utf-8').write(result)
"
echo "Rendered bootstrap -> $BOOTSTRAP_OUT"

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
