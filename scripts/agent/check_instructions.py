#!/usr/bin/env python3
import filecmp
import json
import os
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WATCHED_PREFIXES = (
    "AGENTS.md",
    "CLAUDE.md",
    ".agent-source/skills/",
    "scripts/agent/",
    "docs/agent/",
    ".claude/hooks/",
    ".agents/hooks/",
    ".codex/",
)


def parse_payload() -> dict:
    raw = sys.stdin.read()
    if not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return {}


def edited_file(payload: dict) -> str:
    file_path = payload.get("tool_input", {}).get("file_path", "")
    if file_path:
        return file_path.replace("\\", "/")
    raw = json.dumps(payload)
    for candidate in ("AGENTS.md", "CLAUDE.md"):
        if candidate in raw:
            return candidate
    return ""


def should_check(mode: str, file_path: str) -> bool:
    if mode == "strict":
        return True
    if not file_path:
        return False
    normalized = file_path.replace("\\", "/")
    return any(
        normalized == prefix.rstrip("/") or normalized.startswith(prefix)
        for prefix in WATCHED_PREFIXES
    )


def normalize_body(path: Path) -> str:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    if len(lines) < 3:
        raise ValueError(f"Unexpected short file: {path}")
    try:
        start = lines.index("## Tech Stack")
    except ValueError as exc:
        raise ValueError(f"Missing '## Tech Stack' heading in {path}") from exc
    return "\n".join(lines[start:]).strip()


def ensure_contains(text: str, needle: str, description: str, errors: list[str]) -> None:
    if needle not in text:
        errors.append(description)


def compare_tree(source: Path, target: Path) -> list[str]:
    if not source.exists():
        return [f"Missing canonical skill source: {source.relative_to(ROOT)}"]
    if not target.exists():
        return [f"Missing synced skill copy: {target.relative_to(ROOT)}"]

    comparison = filecmp.dircmp(source, target)
    errors: list[str] = []
    if comparison.left_only:
        errors.append(
            f"{target.relative_to(ROOT)} is missing: {', '.join(sorted(comparison.left_only))}"
        )
    if comparison.right_only:
        errors.append(
            f"{target.relative_to(ROOT)} has unexpected entries: {', '.join(sorted(comparison.right_only))}"
        )
    if comparison.diff_files:
        errors.append(
            f"{target.relative_to(ROOT)} differs from canonical source for: {', '.join(sorted(comparison.diff_files))}"
        )
    for subdir in sorted(comparison.common_dirs):
        errors.extend(compare_tree(source / subdir, target / subdir))
    return errors


def validate() -> list[str]:
    errors: list[str] = []

    agents = ROOT / "AGENTS.md"
    claude = ROOT / "CLAUDE.md"
    agents_text = agents.read_text(encoding="utf-8")
    claude_text = claude.read_text(encoding="utf-8")

    if normalize_body(agents) != normalize_body(claude):
        errors.append(
            "Instruction drift detected: AGENTS.md and CLAUDE.md must match from '## Tech Stack' onward."
        )

    for path, text in ((agents, agents_text), (claude, claude_text)):
        rel = path.relative_to(ROOT)
        if ".claude/docs/" in text:
            errors.append(f"{rel} still references deprecated .claude/docs paths.")
        ensure_contains(text, "docs/agent/", f"{rel} must reference docs/agent/.", errors)
        ensure_contains(text, ".agent-source/skills/", f"{rel} must reference .agent-source/skills/.", errors)
        ensure_contains(text, ".agents/skills/", f"{rel} must reference .agents/skills/.", errors)
        ensure_contains(text, ".claude/skills/", f"{rel} must reference .claude/skills/.", errors)
        ensure_contains(text, ".agents/hooks/", f"{rel} must reference .agents/hooks/.", errors)
        ensure_contains(text, ".claude/hooks/", f"{rel} must reference .claude/hooks/.", errors)
        ensure_contains(text, "scripts/agent/", f"{rel} must reference scripts/agent/.", errors)
        ensure_contains(text, ".codex/", f"{rel} must reference .codex/.", errors)

    errors.extend(compare_tree(ROOT / ".agent-source" / "skills", ROOT / ".agents" / "skills"))
    errors.extend(compare_tree(ROOT / ".agent-source" / "skills", ROOT / ".claude" / "skills"))

    for hook in ("guard-pr.sh", "sync-docs.sh"):
        if not (ROOT / ".claude" / "hooks" / hook).exists():
            errors.append(f"Missing Claude hook wrapper: .claude/hooks/{hook}")
        if not (ROOT / ".agents" / "hooks" / hook).exists():
            errors.append(f"Missing agent hook wrapper: .agents/hooks/{hook}")

    if not (ROOT / ".codex" / "config.toml").exists():
        errors.append("Missing Codex config: .codex/config.toml")
    if not (ROOT / ".codex" / "hooks.json").exists():
        errors.append("Missing Codex hook config: .codex/hooks.json")

    return errors


def main() -> int:
    mode = sys.argv[1] if len(sys.argv) > 1 else "warn"
    payload = parse_payload()
    file_path = edited_file(payload)

    if not should_check(mode, file_path):
        return 0

    errors = validate()
    if not errors:
        if mode == "warn":
            print("Instruction sync verified: shared docs, skills, and hook adapters match.")
        return 0

    heading = "Instruction sync warning:" if mode == "warn" else "Instruction sync check failed:"
    print(heading, file=sys.stderr)
    for error in errors:
        print(f"- {error}", file=sys.stderr)

    if mode == "warn":
        print(
            "Update the paired instruction file and synced agent assets before commit or PR creation.",
            file=sys.stderr,
        )
        return 0

    return 2


if __name__ == "__main__":
    sys.exit(main())
