#!/usr/bin/env bash
# PostToolUse hook: validate that AGENTS.md and CLAUDE.md stay in sync.

set -euo pipefail

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

if [[ -z "$FILE_PATH" ]]; then
    if echo "$INPUT" | grep -q '"file_path".*CLAUDE\.md'; then
        FILE_PATH="CLAUDE.md"
    elif echo "$INPUT" | grep -q '"file_path".*AGENTS\.md'; then
        FILE_PATH="AGENTS.md"
    fi
fi

case "$FILE_PATH" in
    *AGENTS.md|*CLAUDE.md) ;;
    *) exit 0 ;;
esac

ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
AGENTS_FILE="$ROOT_DIR/AGENTS.md"
CLAUDE_FILE="$ROOT_DIR/CLAUDE.md"

normalize_file() {
    local target_file="$1"
    python - "$target_file" <<'PY'
import pathlib
import sys

path = pathlib.Path(sys.argv[1])
text = path.read_text(encoding="utf-8")
lines = text.splitlines()
if len(lines) < 3:
    sys.stderr.write(f"Unexpected short file: {path}\n")
    sys.exit(2)
try:
    start = lines.index("## Tech Stack")
except ValueError:
    sys.stderr.write(f"Missing '## Tech Stack' heading in {path}\n")
    sys.exit(2)
print("\n".join(lines[start:]).strip())
PY
}

AGENTS_BODY=$(normalize_file "$AGENTS_FILE")
CLAUDE_BODY=$(normalize_file "$CLAUDE_FILE")

if [[ "$AGENTS_BODY" != "$CLAUDE_BODY" ]]; then
    echo "Instruction drift detected: AGENTS.md and CLAUDE.md no longer match."
    echo "Keep the two files synchronized. The file title and one-line intro may differ, but the remaining body must be identical."
    exit 2
fi

echo "Instruction sync verified: AGENTS.md and CLAUDE.md match."
exit 0
