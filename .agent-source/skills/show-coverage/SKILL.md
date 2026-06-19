---
name: show-coverage
description: "Use when the user wants to run code coverage, check test coverage, see how much is covered, or display the coverage report. Trigger for phrases like 'show coverage', 'run coverage', 'check coverage', 'what's the coverage', 'how much is covered', 'generate coverage report', or 'display coverage'."
---

# /show-coverage — Run Tests with Coverage and Display the Report

This skill runs `coverage.ps1`, prints the coverage summary inline, and opens the HTML report in the browser.

---

## Step 1 — Ensure reportgenerator is installed

Run `dotnet tool restore` from the repo root. This is a no-op if the tool is already installed.

```powershell
dotnet tool restore
```

Expected output: `Tool 'dotnet-reportgenerator-globaltool' ... was restored.` or `Tool 'dotnet-reportgenerator-globaltool' ... is already installed.`

If this fails, stop and report the error to the user.

---

## Step 2 — Run coverage.ps1

Run the coverage script. If the user specified a service (e.g. "show coverage for OrderService"), pass `-Service <ServiceName>`. Otherwise run all services.

```powershell
# All services
.\coverage.ps1

# Single service
.\coverage.ps1 -Service OrderService
```

The script:
- Cleans `TestResults/` and `coverage-report/`
- Runs `dotnet test --collect:"XPlat Code Coverage"` per project
- Aggregates all Cobertura XML files
- Generates `coverage-report/index.html` and `coverage-report/Summary.txt`
- Opens the HTML report in the default browser

---

## Step 3 — Display the summary

Read `coverage-report/Summary.txt` and display its contents inline so the user can see the coverage numbers without opening the browser.

```powershell
Get-Content coverage-report/Summary.txt
```

Report the line coverage, branch coverage, and method coverage percentages. Highlight any service with line coverage below 60%.

---

## Step 4 — Report to user

Tell the user:
- The overall line/branch coverage percentages from the summary
- That the full HTML report is open in their browser at `coverage-report/index.html`
- Whether any tests failed during the run (the script exits non-zero and lists failed projects)
