---
name: ui-api-debug-playwright
description: Use when the user wants to debug a local HiveSpace UI flow with Playwright, reproduce a frontend bug, inspect failing browser/API behavior, or trace a UI issue back to backend endpoints. Trigger for requests involving the admin, seller, or buyer apps, seeded local accounts, browser console errors, failed network calls, auth redirect issues, or "debug this from the UI" workflows.
---

# UI API Debug Playwright

Use this skill to debug HiveSpace through the browser first, then map what you find to the backend.

This skill is for the three local browser apps only:

- `admin` -> `http://localhost:5173`
- `seller` -> `http://localhost:5174`
- `buyer` -> `http://localhost:5175`

Assume backend services are already running locally. Do not try to start frontend dev servers from this skill.

## Default accounts

Read `src/SEEDED_ACCOUNTS.md` before logging in. Use these defaults unless the user explicitly asks for another seeded account:

- `admin` app -> `admin` / `Admin123$`
- `seller` app -> `tiki` / `TikiTrading123$`
- `buyer` app -> `alice` / `AliceSmith123$`

Allowed override examples:

- `admin` app can use `sysadmin`
- `buyer` app can use `bob`
- `seller` app can use `giver` or `phuongdong`

If the user names a seeded account that does not fit the target app, call that out before proceeding. Treat wrong app/account pairing as a likely cause when login fails.

## What to do

1. Identify the target app: `admin`, `seller`, or `buyer`.
2. Open the matching local URL in Playwright.
3. Take a fresh page snapshot before interacting so selectors come from the live page state.
4. Log in through the visible UI using the seeded account chosen for the run.
5. Reproduce the reported flow exactly, keeping the steps minimal and concrete.
6. Inspect browser evidence:
   - page state from snapshots
   - console messages
   - failing or suspicious API/network activity visible from the browser flow
   - screenshots for key failure states
7. Map each failure to the likely backend area when possible.
8. Return a compact debugging report with evidence and next checks.

## Browser interaction rules

- Do not invent selectors before taking a fresh snapshot.
- Prefer the visible UI flow over direct URL guessing unless the user explicitly asks for a deep link.
- If the page clearly failed to load, collect the current URL, screenshot, and console output before trying alternatives.
- If login redirects loop or land on the wrong app, treat that as a primary finding rather than masking it with extra retries.

## Backend mapping hints

Use the request path and app context to narrow the likely backend owner:

- `/api/v1/admins/...` usually points to admin-focused endpoints in Identity, User, or Catalog service flows
- seller flows usually involve seller-scoped catalog, order, media, or notification endpoints
- buyer flows usually involve storefront, checkout, payment, cart, and order endpoints
- auth/session problems often involve IdentityService and browser session cookies
- cross-app redirect issues often involve the configured frontend origins:
  - `admin` -> `http://localhost:5173`
  - `seller` -> `http://localhost:5174`
  - `buyer` -> `http://localhost:5175`

When uncertain, say it is an inference and cite the route or behavior that led you there.

## Failure handling

If the issue reproduces, capture:

- the exact step where it failed
- the visible UI symptom
- the failing request or redirect if present
- console errors or warnings that materially explain the failure

If the issue does not reproduce, capture:

- the account used
- the app URL used
- the successful path taken
- any near-miss warnings or flaky behavior seen during the run

If the UI is unavailable, report whether the likely cause is:

- frontend app not running on the expected port
- backend services unavailable
- auth service unavailable
- wrong app URL

## Output format

Use this structure in the final report:

```markdown
App: <admin|seller|buyer>
URL: <local app url>
Account: <seeded account used>
Reproduced: <yes|no|blocked>

Summary:
<one short paragraph>

Evidence:
- Screenshot: <path or "not captured">
- Console: <key errors/warnings or "none">
- Network/API: <failed request(s) or "none observed">

Likely backend area:
- <service or endpoint area>

Next check:
- <single most useful follow-up>
```

Keep the final report concise. The value of this skill is fast correlation from UI behavior to likely API ownership, not a long transcript.
