<#
.SYNOPSIS
    Run tests with code coverage and generate an HTML report.

.PARAMETER Service
    Optional. Scope to a single service, e.g. "OrderService".
    Omit to run all service test projects.

.EXAMPLE
    dotnet tool restore          # once per machine
    .\coverage.ps1               # all services
    .\coverage.ps1 -Service OrderService
#>
param(
    [string]$Service = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot   = $PSScriptRoot
$ResultsDir = Join-Path $RepoRoot "TestResults"
$ReportRoot = Join-Path $RepoRoot "coverage-report"
$Normalizer = Join-Path $RepoRoot "scripts\coverage\Normalize-Cobertura.ps1"
$RunId      = [DateTime]::UtcNow.ToString("yyyyMMddHHmmssfff")
$RunResultsDir = Join-Path $ResultsDir $RunId
$ReportDir  = Join-Path $ReportRoot $RunId

# --- resolve test projects ---
$TestsRoot = Join-Path $RepoRoot "tests"

if ($Service -ne "") {
    $pattern = "*$Service.Tests*"
    $projects = @(Get-ChildItem -Path $TestsRoot -Directory -Filter $pattern |
                Where-Object { $_.Name -ne "HiveSpace.Testing.Shared" })
} else {
    $projects = @(Get-ChildItem -Path $TestsRoot -Directory |
                Where-Object { $_.Name -like "*.Tests" -and $_.Name -ne "HiveSpace.Testing.Shared" })
}

if ($projects.Count -eq 0) {
    Write-Error "No test projects found$(if ($Service) { " matching '$Service'" })."
}

Write-Host "`nRunning coverage for $($projects.Count) project(s):`n"
foreach ($p in $projects) { Write-Host "  $($p.Name)" }
Write-Host ""

# --- run tests ---
$failed = @()
foreach ($p in $projects) {
    Write-Host ">> $($p.Name)"
    $outDir = Join-Path $RunResultsDir $p.Name
    dotnet test $p.FullName `
        --nologo `
        --verbosity minimal `
        --collect:"XPlat Code Coverage" `
        --settings (Join-Path $RepoRoot "coverage.runsettings") `
        --results-directory $outDir
    if ($LASTEXITCODE -ne 0) { $failed += $p.Name }
}

# --- locate cobertura files ---
$xmlFiles = @(Get-ChildItem -Path $RunResultsDir -Recurse -Filter "coverage.cobertura.xml" |
            Select-Object -ExpandProperty FullName)

if ($xmlFiles.Count -eq 0) {
    Write-Error "No coverage XML files found. Tests may have failed before collecting coverage."
}

# --- normalize cobertura files ---
foreach ($xmlFile in $xmlFiles) {
    & $Normalizer -CoverageFile $xmlFile -RepoRoot $RepoRoot
}

$reports = $xmlFiles -join ";"

# --- generate HTML report ---
Write-Host "`nGenerating HTML report..."
dotnet reportgenerator `
    "-reports:$reports" `
    "-targetdir:$ReportDir" `
    "-reporttypes:Html;TextSummary" `
    "-verbosity:Warning"

if ($LASTEXITCODE -ne 0) {
    Write-Error "reportgenerator failed. Run 'dotnet tool restore' if the tool is missing."
}

# --- print summary ---
$summaryFile = Join-Path $ReportDir "Summary.txt"
if (Test-Path $summaryFile) {
    Write-Host ""
    Get-Content $summaryFile | Where-Object { $_ -match "%" } | ForEach-Object { Write-Host "  $_" }
}

Write-Host "`nReport: $ReportDir\index.html"

if ($failed.Count -gt 0) {
    Write-Host "`nFailed projects:" -ForegroundColor Yellow
    foreach ($f in $failed) { Write-Host "  $f" -ForegroundColor Yellow }
}

# --- open report ---
Start-Process (Join-Path $ReportDir "index.html")
