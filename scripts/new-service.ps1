param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$ServiceName,
    [Parameter(Mandatory = $true, Position = 1)]
    [string]$TemplateName,
    [Parameter(Mandatory = $false)]
    [switch]$AddToSolution
)

# Fail fast
$ErrorActionPreference = 'Stop'

Write-Host "Script root: $PSScriptRoot"

# Resolve repository root (script is located in ./scripts)
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
if (-not (Test-Path $repoRoot)) {
    Throw "Repository root not found: $repoRoot"
}

# Ensure src exists
$srcPath = Join-Path $repoRoot 'src'
if (-not (Test-Path $srcPath)) {
    Throw "Source folder not found: $srcPath"
}

Set-Location $srcPath

Write-Host "Generating new service '$ServiceName' using template '$TemplateName' in $srcPath..."

# Helper: check if template exists
function Test-TemplateExists {
    param([string]$ShortName)
    $list = & dotnet new list 2>$null
    if ($LASTEXITCODE -ne 0) { return $false }
    return $list -match [regex]::Escape($ShortName)
}

# If template missing, try to find a local template folder and install it
if (-not (Test-TemplateExists -ShortName $TemplateName)) {
    Write-Warning "Template '$TemplateName' not found in 'dotnet new list'. Attempting to locate a local template in ./templates..."
    $templatesRoot = Join-Path $repoRoot 'templates'
    $installed = $false
    if (Test-Path $templatesRoot) {
        Get-ChildItem -Path $templatesRoot -Directory | ForEach-Object {
            $candidate = $_
            $candidatePath = $candidate.FullName
            $templateConfig = Join-Path $candidatePath '.template.config\template.json'
            $matchFound = $false
            # Match by folder name (exact or partial)
            if ($candidate.Name -eq $TemplateName -or $candidate.Name -like "*$TemplateName*") { $matchFound = $true }
            if (Test-Path $templateConfig) {
                try {
                    $cfg = Get-Content $templateConfig -Raw | ConvertFrom-Json -ErrorAction Stop
                    $short = $cfg.shortName
                    if ($short -eq $TemplateName -or $short -like "*$TemplateName*") { $matchFound = $true }
                } catch {
                    # ignore parse errors
                }
            }

            if ($matchFound) {
                Write-Host "Installing local template from: $candidatePath"
                dotnet new install $candidatePath
                if ($LASTEXITCODE -eq 0) { $installed = $true; return }
            }
        }
    }
    if (-not $installed) {
        Write-Warning "Could not locate a local template matching '$TemplateName'. You can install one with 'dotnet new install <path>' or use an installed template name.";
    }
}

# Run dotnet new and capture output
$newArgs = @($TemplateName, '-n', $ServiceName)
$newCmd = "dotnet new $($newArgs -join ' ')"
Write-Host "Running: $newCmd"
& dotnet new @newArgs

# Remove placeholder files
$placeholders = Get-ChildItem -Path $ServiceName -Recurse -Filter '__EMPTY_FOLDER__README.md' -ErrorAction SilentlyContinue
if ($placeholders) {
    Write-Host "Removing placeholder files..."
    $placeholders | Remove-Item -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "No placeholder files found."
}

if ($AddToSolution.IsPresent) {
    # Locate solution file in repo root
    $solutionPath = Join-Path $repoRoot 'hivespace.microservice.sln'
    if (-not (Test-Path $solutionPath)) {
        Write-Warning "Solution file not found at $solutionPath. Skipping dotnet sln add."
    } else {
        $csprojects = Get-ChildItem -Path $ServiceName -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
        if ($csprojects) {
            foreach ($proj in $csprojects) {
                Write-Host "Adding project to solution: $($proj.FullName)"
                dotnet sln $solutionPath add $proj.FullName
            }
        } else {
            Write-Warning "No .csproj files found under $ServiceName to add to solution."
        }
    }
} else {
    Write-Host "Skipping solution modification. To add projects to solution automatically, pass -AddToSolution.`nYou can add projects manually with 'dotnet sln add <path-to-csproj>'."
}

Write-Host "Service '$ServiceName' created."
