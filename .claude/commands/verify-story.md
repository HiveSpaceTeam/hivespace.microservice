Audit whether the current backend repo changes cover the selected HiveSpace feature tasks. This command is verification-only: do not edit files, stage files, commit, format, generate migrations, or implement missing work.

Step 1 - Load context
- Read local `AGENTS.md` and `CLAUDE.md` if present.
- Read:
  - `../hivespace.spec/.specify/memory/constitution.md`
  - `../hivespace.spec/shared/api-catalog.md`
  - `../hivespace.spec/shared/event-catalog.md`
- Identify the feature from the user request. If not provided, read `../hivespace.spec/.specify/feature.json`; if still unclear, ask for the feature name.
- Read `../hivespace.spec/specs/[feature-name]/spec.md`, `plan.md`, `tasks.md`, and `tasks/backend.md`.
- Read `tasks/config.md`, `tasks/docs-catalog.md`, and `tasks/verification.md` entries that apply to backend/config/docs/catalog/backend verification.
- Read affected `../hivespace.spec/services/<service-name>/README.md` files named by the plan or backend tasks.
- Identify any `tasks/verification.md` item with a detail bullet that starts
  with `User-owned E2E:` and treat it as explicit user-run validation outside
  this command's executable scope.

Step 2 - Inspect current changes
- Run `git status --short` and `git diff --name-only` to identify changed, deleted, and untracked files.
- Use `git diff` and targeted searches to compare implementation behavior against each relevant task's exact acceptance text.
- Use GitNexus `detect_changes({ scope: "all" })` when available and report the risk level and unexpected affected flows.
- Treat untracked source files as part of the implementation surface, but call out temporary files, generated artifacts, `.keys`, logs, or unrelated config churn.

Step 3 - Verify task coverage
- Produce a table for every relevant backend, config, docs/catalog, and verification task with status `Covered`, `Partial`, `Missing`, or `Not Applicable`.
- Mark `User-owned E2E` tasks as `User-owned` or `Pending user` rather than
  `Missing` when implementation is otherwise complete.
- Include concrete evidence for every `Covered` or `Partial` status: file path, symbol/route/type, search result, diff evidence, or verification command.
- Do not mark a task `Covered` only because an expected file exists; verify behavior, constraints, forbidden behavior, and acceptance criteria.
- Verify that each planned backend scenario has a matching test or clearly
  explain the missing coverage.
- Check public endpoint changes against `shared/api-catalog.md`.
- Check event/message changes against `shared/event-catalog.md`; confirm reused contracts remain unchanged when tasks require no new event.
- Check repo rules that commonly regress backend stories: CQRS/Minimal API pattern, service boundaries, outbox for integration events, idempotent consumers, no package `Version=` attributes, and no direct cross-service database reads.

Step 4 - Run verification commands
- Run the smallest meaningful build/test command for each affected backend service, normally `dotnet build <affected-api-csproj>`.
- Run `.\quality-gate.ps1 -Scope backend:<ServiceName>` for each affected
  service when the local environment supports coverage collection.
- Run full `dotnet build` only when shared libraries, shared authorization, contracts, or multiple services changed.
- If a command cannot run because of local infrastructure, file locks, or environment issues, report the exact blocker and whether a retry was attempted.
- Do not run commands that intentionally mutate tracked files.

Step 5 - Report
- Start with the overall judgment: `Ready`, `Not ready`, or `Blocked`.
- If only `User-owned E2E` tasks remain, the overall judgment may still be
  `Ready`, but the pending user validation must be called out explicitly.
- List critical gaps first, with task IDs and file references.
- Include the task coverage table.
- Include verification commands and results.
- Call out any affected service that is below 90% measured line coverage and
  identify where additional tests are needed.
- List unrelated or suspicious changed files separately from expected story files.
- If gaps remain, recommend running `/start-story` or manual fixes for the specific missing task IDs, then rerun `/verify-story`.
