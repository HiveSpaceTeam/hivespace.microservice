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

if echo "$COMMAND" | grep -q "gh pr create"; then
    cat <<'EOF'
PR CREATION BLOCKED - follow the required PR process first:

1. Run: bash scripts/sync-config.sh
   (syncs appsettings.json / local.settings.json to hivespace.config)
2. Run: npx gitnexus analyze
   (syncs the GitNexus index with all current changes)
3. Tell the user: "Please start a new session in this repository."
4. In the new session, run /review to review all current changes.
5. Apply any fixes or improvements from the review.
6. Only after the review is complete, run: gh pr create

Do not attempt to bypass this process.
EOF
    exit 2
fi

exit 0
