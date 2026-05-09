#!/usr/bin/env python3
import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / ".agent-source" / "skills"
TARGETS = (
    ROOT / ".agents" / "skills",
    ROOT / ".claude" / "skills",
)


def main() -> int:
    if not SOURCE.exists():
        raise SystemExit(f"Missing canonical skill source: {SOURCE}")

    for target in TARGETS:
        target.mkdir(parents=True, exist_ok=True)
        shutil.copytree(SOURCE, target, dirs_exist_ok=True)
        print(f"Synced {SOURCE.relative_to(ROOT)} -> {target.relative_to(ROOT)}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
