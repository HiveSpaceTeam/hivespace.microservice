---
name: create-pr
description: "Use when the user asks to create a PR, submit changes, open a pull request, push and PR their work, or ship a feature branch. Trigger this skill whenever the user says anything like 'create a PR', 'open a pull request', 'submit my changes', 'push and PR', 'ship this', or 'PR this'. Also trigger when the user finishes a feature and asks what to do next — this skill handles the full branch → build → commit → push → review → PR flow for the hivespace.microservice repo."
---

# /create-pr — Build, Branch, Commit, Push, and Open a PR

This skill handles the full workflow from finishing a feature to opening a pull request.

**This session:** build → branch → analyze → stage → commit → push → sync config → create PR  
**New session (after PR is open):** /review → fix any findings

---

## Step 1 — Verify the build passes

Run `dotnet build` from the repo root. This must pass before anything is staged.

```bash
dotnet build
```

If the build fails, stop here. Report all errors to the user and do not proceed until they are fixed. The 6 nullability warnings in `HiveSpace.Domain.Shared` are expected and non-blocking — everything else is a real error.

---

## Step 2 — Determine the branch

Check the current branch:

```bash
git branch --show-current
```

- If already on a feature/bugfix branch (not `master` or `develop`), use it — no need to create a new one.
- If on `master` or `develop`, ask the user for a branch name or derive one from the change context.

**Branch naming convention** (follow what exists in this repo):
- `Features/<short-description>` — new features or refactors (e.g., `Features/order-checkout-saga`)
- `bugfix/<short-description>` — bug fixes (e.g., `bugfix/fix-product-detail`)
- `feat/<team-alias>/<date>` — team-scoped feature branches

Create and switch to the branch if needed:

```bash
git checkout -b Features/<branch-name>
```

---

## Step 3 — Check what changed

Run `gitnexus_detect_changes()` to get a clear picture of what symbols and execution flows were affected. Report this to the user so they know what scope is going into the commit.

Then run `git status` to see the full file list.

---

## Step 4 — Refresh the GitNexus index

Run `npx gitnexus analyze` **before** staging so the index reflects the current working state and the commit is captured accurately.

Check whether embeddings exist first:

```bash
cat .gitnexus/meta.json
```

If `stats.embeddings > 0`, preserve them:

```bash
npx gitnexus analyze --embeddings
```

Otherwise:

```bash
npx gitnexus analyze
```

---

## Step 5 — Stage non-JSON files

**This project prohibits agents from staging `*.json` files.** Agents must never add any `*.json` file to the index. This is because `appsettings.json`, `local.settings.json`, and other config files contain environment-specific values that need explicit user handling.

Stage all non-JSON changed files:

```bash
git diff --name-only HEAD | grep -v '\.json$' | xargs git add
git diff --cached --name-only  # confirm what's staged
```

If any `*.json` files are modified, tell the user:

> "These JSON files were changed but not staged by me — you need to stage them manually if you want them committed: [list files]"

Do not proceed to commit until the user confirms they are happy with the staged set. If the user has already staged `*.json` files themselves, leave those staged entries untouched and include them in the commit. Do not unstage, restage, or modify user-staged JSON files.

---

## Step 6 — Commit

Write a commit message that explains *why* the change exists, not just what files changed. Look at recent commit messages for style guidance:

```bash
git log --oneline -5
```

Common patterns in this repo:
- `feat: <description>` — new feature
- `fix: <description>` — bug fix
- `chore: <description>` — tooling, config, cleanup
- `Features/<description>` — feature branch merge style

Create the commit:

```bash
git commit -m "$(cat <<'EOF'
feat: <summary of change>

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
EOF
)"
```

---

## Step 7 — Pull latest from remote before pushing

Fetch the latest state of the remote to avoid push conflicts:

```bash
git fetch origin
git rebase origin/master
```

If the rebase has conflicts, resolve them with the user before continuing.

---

## Step 8 — Push the branch

```bash
git push -u origin HEAD
```

---

## Step 9 — Sync config files

This syncs all `appsettings.json` and `local.settings.json` files to `hivespace.config/`.

```bash
bash scripts/sync-config.sh
```

---

## Step 10 — Create the PR

Draft the PR body based on the commits and diff (`git log origin/master..HEAD` and `git diff origin/master...HEAD --stat`). Structure it as:

```
## Summary
- bullet points per service / area changed

## Test plan
- [ ] dotnet build passes
- [ ] one checkbox per changed service or flow
```

Then create the PR directly:

```
gh pr create --title "<title>" --body "..." --base master --head <branch>
```

---

## Step 11 — Hand off to a new session for review

After the PR is created, ask the user:

> "PR is open. To run a post-PR review:
> 1. **Start a new Claude Code session** in this repository
> 2. Run `/review` — this will review all changes on the branch
> 3. Apply any fixes the review surfaces and push them to the same branch"

---

## Quick reference — what each step guards against

| Step | Why it matters |
|------|---------------|
| `dotnet build` | Catches compile errors before anything is staged |
| `gitnexus_detect_changes` | Confirms the diff scope matches intent |
| `npx gitnexus analyze` (before stage) | Index reflects current working state before commit |
| No agent `*.json` staging | Keeps environment config changes under explicit user control |
| `scripts/sync-config.sh` | Keeps the config repo in sync with service appsettings |
| `gh pr create` | Opens the PR after build, index, commit, push, and config sync |
| New session + `/review` | Post-PR review in clean context; fixes pushed to same branch |
