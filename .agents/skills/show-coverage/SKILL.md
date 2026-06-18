---
name: show-coverage
description: "Use when the user wants to run code coverage, check test coverage, see how much is covered, or display the coverage report. Trigger for phrases like 'show coverage', 'run coverage', 'check coverage', 'what's the coverage', 'how much is covered', 'generate coverage report', or 'display coverage'."
---

# /show-coverage — Run Tests with Coverage and Display the Report

This skill checks whether a fresh coverage report already exists (< 1 hour old). If it does, it reuses it. Otherwise it runs `coverage.ps1`, prints the summary inline, and opens the HTML report in the browser.

---

## Step 1 — Find the most recent report

The coverage script writes each run into a timestamped subdirectory: `coverage-report/{RunId}/Summary.txt`.
Never read the root-level `coverage-report/Summary.txt` — it is not updated by the script and may be stale from a prior session.

Run this to locate the newest summary and check its age:

```powershell
$latestSummary = Get-ChildItem -Path "coverage-report" -Recurse -Filter "Summary.txt" |
    Where-Object { $_.DirectoryName -ne (Resolve-Path "coverage-report").Path } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($latestSummary) {
    $ageMinutes = [int]((Get-Date) - $latestSummary.LastWriteTime).TotalMinutes
    Write-Host "Latest report: $($latestSummary.FullName)"
    Write-Host "Age: $ageMinutes minutes"
} else {
    Write-Host "No existing report found."
    $ageMinutes = 999
}
```

- If `$ageMinutes -lt 60` → **skip to Step 3** (reuse the existing report)
- Otherwise → **proceed to Step 2** (re-run coverage)

---

## Step 2 — Ensure reportgenerator is installed, then run coverage.ps1

```powershell
dotnet tool restore
```

Expected output: `Tool 'dotnet-reportgenerator-globaltool' ... was restored.` or already installed.
If this fails, stop and report the error.

Then run the coverage script. If the user specified a service (e.g. "show coverage for OrderService"), pass `-Service <ServiceName>`. Otherwise run all services.

```powershell
# All services
.\coverage.ps1

# Single service
.\coverage.ps1 -Service OrderService
```

After the script finishes, re-run the Step 1 query to get `$latestSummary` pointing at the newly generated report.

---

## Step 3 — Display the summary

Read the summary from the most recent **timestamped** subdirectory (the `$latestSummary` found in Step 1):

```powershell
Get-Content $latestSummary.FullName
```

Report the line coverage, branch coverage, and method coverage percentages. Highlight any assembly with line coverage below 60%.

Tell the user:
- Whether the report was freshly generated or reused from cache (and how old it is)
- The overall line/branch/method coverage percentages
- The full path to the HTML report so they can open it: replace `Summary.txt` with `index.html` in `$latestSummary.FullName`
- Whether any tests failed during the run (only applicable when Step 2 was executed; the script exits non-zero and lists failed projects)
