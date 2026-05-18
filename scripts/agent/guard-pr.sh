#!/usr/bin/env bash
# Shared PreToolUse hook logic: block premature PR creation and require strict sync checks before commit/PR.

set -euo pipefail

INPUT=$(cat)
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

PARSE_COMMAND='
import sys, json
try:
    data = json.load(sys.stdin)
    print(data.get("tool_input", {}).get("command", ""))
except Exception:
    print("")
'

COMMAND=$(printf '%s' "$INPUT" | python -c "$PARSE_COMMAND" 2>/dev/null \
  || printf '%s' "$INPUT" | python3 -c "$PARSE_COMMAND" 2>/dev/null \
  || echo "")

if [[ -z "$COMMAND" ]]; then
    if echo "$INPUT" | grep -q '"command".*gh pr create'; then
        COMMAND="gh pr create"
    elif echo "$INPUT" | grep -q '"command".*git commit'; then
        COMMAND="git commit"
    fi
fi

if echo "$COMMAND" | grep -Eq '(^|[[:space:]])git commit([[:space:]]|$)|(^|[[:space:]])gh pr create([[:space:]]|$)'; then
    if ! printf '%s' "$INPUT" | bash "$SCRIPT_DIR/check-instructions.sh" strict; then
        exit 2
    fi
fi

exit 0
