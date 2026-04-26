#!/usr/bin/env bash
# PostToolUse hook: when CLAUDE.md or AGENTS.md is edited/written,
# remind Claude to keep the other file in sync.

INPUT=$(cat)

PARSE_FILE_PATH='
import sys, json
try:
    d = json.load(sys.stdin)
    print(d.get("tool_input", {}).get("file_path", ""))
except Exception:
    print("")
'

FILE_PATH=$(echo "$INPUT" | python -c "$PARSE_FILE_PATH" 2>/dev/null \
  || echo "$INPUT" | python3 -c "$PARSE_FILE_PATH" 2>/dev/null \
  || echo "")

# Fallback: grep the raw JSON if Python unavailable
if [[ -z "$FILE_PATH" ]]; then
    if echo "$INPUT" | grep -q '"file_path".*CLAUDE\.md'; then
        FILE_PATH="CLAUDE.md"
    elif echo "$INPUT" | grep -q '"file_path".*AGENTS\.md'; then
        FILE_PATH="AGENTS.md"
    fi
fi

if [[ "$FILE_PATH" == *"CLAUDE.md" ]]; then
    echo "CLAUDE.md was updated."
    echo "You MUST now update AGENTS.md to keep the quick reference in sync."
    echo "Specifically check: the service type table and the hard rules section."
    echo "Do not skip this — AGENTS.md must always reflect CLAUDE.md."
elif [[ "$FILE_PATH" == *"AGENTS.md" ]]; then
    echo "AGENTS.md was updated."
    echo "Check if CLAUDE.md needs the same update — specifically the Service Architecture section."
fi

exit 0
