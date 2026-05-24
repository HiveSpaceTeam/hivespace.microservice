---
name: refresh-master
description: "Use when the user asks to stash current changes, switch or checkout to master, pull the latest code, then pop or reapply the stash. Trigger this skill for phrases like \"refresh master\", \"checkout master and pull latest\", \"stash my changes and update master\", \"pull new code then pop stash\", or similar git sync workflows."
---

# /refresh-master - Stash Work, Update Master, Reapply Work

Use this skill when the user wants to preserve local work, update the `master` branch, and reapply the saved changes.

## Workflow

Run from the repository root. Prefix shell commands with `rtk`, per the repo's global RTK instruction.

1. Check the working tree:

```bash
rtk git status --short
```

2. If there are local changes, create a named stash that includes untracked files:

```bash
rtk git stash push -u -m "refresh-master: $(date -u +%Y%m%dT%H%M%SZ)"
```

If there are no local changes, skip the stash step and do not pop an older existing stash later.

3. Switch to `master`:

```bash
rtk git checkout master
```

4. Pull the latest code without creating a merge commit:

```bash
rtk git pull --ff-only
```

If this fails, stop and report that `master` needs manual reconciliation. Do not run a merge or rebase unless the user asks.

5. If this workflow created a stash, reapply it:

```bash
rtk git stash pop 'stash@{0}'
```

If stash pop causes conflicts, stop and report the conflicted files from `rtk git status --short`. Do not try to resolve conflicts automatically.

## Safety Rules

- Only pop a stash created during this workflow. Never pop a pre-existing stash when the working tree was already clean.
- Preserve untracked files by using `git stash push -u`.
- Prefer `git pull --ff-only` so the workflow does not create accidental merge commits.
- Report the final branch and working tree status after the workflow completes.
