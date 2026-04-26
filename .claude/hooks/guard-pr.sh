#!/usr/bin/env bash
# PreToolUse hook: block gh pr create and require a review session first.

INPUT=$(cat)

PARSE_COMMAND='
import sys, json
try:
    d = json.load(sys.stdin)
    print(d.get("tool_input", {}).get("command", ""))
except Exception:
    print("")
'

COMMAND=$(echo "$INPUT" | python -c "$PARSE_COMMAND" 2>/dev/null \
  || echo "$INPUT" | python3 -c "$PARSE_COMMAND" 2>/dev/null \
  || echo "")

# Fallback: grep the raw JSON if Python unavailable
if [[ -z "$COMMAND" ]]; then
    if echo "$INPUT" | grep -q '"command".*gh pr create'; then
        COMMAND="gh pr create"
    fi
fi

if echo "$COMMAND" | grep -q "gh pr create"; then
    cat <<'EOF'
PR CREATION BLOCKED — follow the required PR process first:

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
