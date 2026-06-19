param(
    [Parameter(Mandatory = $true)]
    [string]$Scope,

    [string]$AcceptedRiskScope = "",
    [string]$AcceptedRiskReason = "",
    [string]$AcceptedRiskApprovingRole = "maintainer",
    [string]$AcceptedRiskExpiresWhen = "coverage implemented"
)

$ErrorActionPreference = "Stop"

$serviceProjects = [ordered]@{
    "IdentityService" = "tests/HiveSpace.IdentityService.Tests/HiveSpace.IdentityService.Tests.csproj"
    "UserService" = "tests/HiveSpace.UserService.Tests/HiveSpace.UserService.Tests.csproj"
    "CatalogService" = "tests/HiveSpace.CatalogService.Tests/HiveSpace.CatalogService.Tests.csproj"
    "OrderService" = "tests/HiveSpace.OrderService.Tests/HiveSpace.OrderService.Tests.csproj"
    "PaymentService" = "tests/HiveSpace.PaymentService.Tests/HiveSpace.PaymentService.Tests.csproj"
    "MediaService" = "tests/HiveSpace.MediaService.Tests/HiveSpace.MediaService.Tests.csproj"
    "NotificationService" = "tests/HiveSpace.NotificationService.Tests/HiveSpace.NotificationService.Tests.csproj"
}

$sharedProject = "tests/HiveSpace.Testing.Shared/HiveSpace.Testing.Shared.csproj"
$validScopes = @("docs-only", "shared", "release") + ($serviceProjects.Keys | ForEach-Object { "backend:$($_)" })

if ($validScopes -notcontains $Scope) {
    throw "Invalid scope '$Scope'. Valid scopes: $($validScopes -join ', ')"
}

function New-CheckResult {
    param(
        [string]$CheckId,
        [string]$Name,
        [string]$Journey,
        [string]$Audience,
        [string]$OwnerSurface,
        [string]$Status,
        [AllowNull()]
        [object]$FailureCategory,
        [string]$BlockingDecision,
        [int]$RerunCount,
        [string]$Summary,
        [AllowNull()]
        [object]$CoveragePct
    )

    [ordered]@{
        checkId         = $CheckId
        name            = $Name
        journey         = $Journey
        audience        = $Audience
        ownerSurface    = $OwnerSurface
        status          = $Status
        failureCategory = $FailureCategory
        blockingDecision = $BlockingDecision
        rerunCount      = $RerunCount
        coveragePct     = $CoveragePct
        summary         = $Summary
    }
}

$CoverageThreshold = 0.80
$CoverageResultsRoot = Join-Path $PSScriptRoot "TestResults\quality-gate-$([DateTime]::UtcNow.ToString('yyyyMMddHHmmssfff'))"
$CoverageSettings = Join-Path $PSScriptRoot "coverage.runsettings"

function Invoke-DotnetTest {
    param([string]$Project, [switch]$CollectCoverage)

    if ($CollectCoverage -and (Test-Path $CoverageSettings)) {
        $resultsDir = Join-Path $CoverageResultsRoot ([System.IO.Path]::GetFileNameWithoutExtension($Project))
        $output = & dotnet test $Project --nologo --verbosity minimal `
            --collect:"XPlat Code Coverage" `
            --settings $CoverageSettings `
            --results-directory $resultsDir 2>&1 | Out-String
        [ordered]@{
            exitCode   = $LASTEXITCODE
            output     = $output.Trim()
            resultsDir = $resultsDir
        }
    } else {
        $output = & dotnet test $Project --nologo --verbosity minimal 2>&1 | Out-String
        [ordered]@{
            exitCode   = $LASTEXITCODE
            output     = $output.Trim()
            resultsDir = $null
        }
    }
}

function Get-CoverageLineRate {
    param([string]$ResultsDir)

    if (-not $ResultsDir -or -not (Test-Path $ResultsDir)) { return $null }

    $xmlFiles = @(Get-ChildItem -Path $ResultsDir -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue |
                  Sort-Object LastWriteTime -Descending)
    if ($xmlFiles.Count -eq 0) { return $null }

    [xml]$xml = Get-Content $xmlFiles[0].FullName -Raw
    $rate = $xml.coverage.'line-rate'
    if ($null -eq $rate) { return $null }
    return [double]$rate
}

function New-CoverageGap {
    param(
        [string]$Journey,
        [string]$Reason,
        [string]$RiskLevel,
        [string]$Resolution
    )

    [ordered]@{
        journey = $Journey
        reason = $Reason
        riskLevel = $RiskLevel
        resolution = $Resolution
    }
}

function New-AcceptedRisk {
    param(
        [string]$CheckId,
        [string]$Scope,
        [string]$ApprovingRole,
        [string]$Reason,
        [string]$ExpiresWhen
    )

    [ordered]@{
        checkId = $CheckId
        scope = $Scope
        approvingRole = $ApprovingRole
        reason = $Reason
        expiresWhen = $ExpiresWhen
    }
}

function Get-FailureCategory {
    param([string]$Output)

    if ($Output -match "(?i)(sql|database|connection|socket|network|rabbitmq|kafka|azurite|blob|no such host|refused)") {
        return "environment_readiness"
    }

    if ($Output -match "(?i)(fixture|seed|test data|stub configured)") {
        return "missing_test_data"
    }

    return "product_behavior"
}

function Invoke-ProjectCheck {
    param(
        [string]$Project,
        [string]$CheckId,
        [string]$Name,
        [string]$Journey,
        [string]$Audience,
        [string]$OwnerSurface,
        [switch]$WithCoverage
    )

    $first = Invoke-DotnetTest $Project -CollectCoverage:$WithCoverage
    if ($first.exitCode -eq 0) {
        $coveragePct = if ($WithCoverage) { Get-CoverageLineRate $first.resultsDir } else { $null }

        if ($WithCoverage -and $null -ne $coveragePct -and $coveragePct -lt $CoverageThreshold) {
            $pctDisplay = [math]::Round($coveragePct * 100, 1)
            return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface `
                "fail" "coverage_below_threshold" "merge_blocking" 0 `
                "$Name tests passed but line coverage is $pctDisplay% (threshold: $([int]($CoverageThreshold*100))%)" `
                $coveragePct
        }

        return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface "pass" $null "none" 0 "$Name passed" $coveragePct
    }

    $second = Invoke-DotnetTest $Project -CollectCoverage:$WithCoverage
    if ($second.exitCode -eq 0) {
        $coveragePct = if ($WithCoverage) { Get-CoverageLineRate $second.resultsDir } else { $null }

        if ($WithCoverage -and $null -ne $coveragePct -and $coveragePct -lt $CoverageThreshold) {
            $pctDisplay = [math]::Round($coveragePct * 100, 1)
            return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface `
                "fail" "coverage_below_threshold" "merge_blocking" 1 `
                "$Name tests passed (after rerun) but line coverage is $pctDisplay% (threshold: $([int]($CoverageThreshold*100))%)" `
                $coveragePct
        }

        return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface "pass" $null "none" 1 "$Name passed after one rerun" $coveragePct
    }

    if ($first.exitCode -ne $second.exitCode) {
        return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface `
            "unstable_check" "unstable_check" "merge_blocking" 1 `
            "$Name produced inconsistent exit codes $($first.exitCode) and $($second.exitCode)" $null
    }

    $failureCategory = Get-FailureCategory $second.output
    $status = if ($failureCategory -eq "environment_readiness") { "environment_failure" } elseif ($failureCategory -eq "missing_test_data") { "missing_data" } else { "fail" }
    $summary = if ([string]::IsNullOrWhiteSpace($second.output)) { "$Name failed with exit code $($second.exitCode)" } else { ($second.output -split "`n" | Select-Object -Last 8) -join "`n" }

    return New-CheckResult $CheckId $Name $Journey $Audience $OwnerSurface $status $failureCategory "merge_blocking" 1 $summary $null
}

function Get-ServiceJourney {
    param([string]$ServiceName)

    switch ($ServiceName) {
        "IdentityService" { "buyer-authentication-session" }
        "ApiGateway" { "public-route-ownership" }
        "UserService" { "profile-settings-address-store" }
        "CatalogService" { "storefront-catalog-discovery" }
        "OrderService" { "cart-checkout-orders-fulfillment" }
        "PaymentService" { "payment-result-handling" }
        "MediaService" { "media-upload-confirmation" }
        "NotificationService" { "notification-delivery-visibility" }
        default { "shared-backend-baseline" }
    }
}

function Get-OverallResult {
    param([array]$Results)

    $priority = @("fail", "unstable_check", "environment_failure", "missing_data", "accepted_risk", "not_applicable", "pass")
    foreach ($status in $priority) {
        if ($Results.status -contains $status) {
            return $status
        }
    }

    return "pass"
}

$startedAt = (Get-Date).ToUniversalTime()
$checkResults = @()
$coverageGaps = @()
$acceptedRisks = @()

if ($Scope -eq "docs-only") {
    $checkResults += New-CheckResult "backend.docs.runtime-not-applicable" "Backend runtime checks not applicable" "documentation-only-change" "contributor" "backend" "not_applicable" $null "none" 0 "Documentation-only scope does not run backend tests" $null
}
elseif ($Scope -eq "shared") {
    $checkResults += Invoke-ProjectCheck $sharedProject "backend.shared.test-infrastructure" "Shared backend test infrastructure" "shared-backend-baseline" "service" "HiveSpace.Testing.Shared"
}
elseif ($Scope -eq "release") {
    $checkResults += Invoke-ProjectCheck $sharedProject "backend.shared.test-infrastructure" "Shared backend test infrastructure" "shared-backend-baseline" "service" "HiveSpace.Testing.Shared"
    foreach ($serviceName in $serviceProjects.Keys) {
        $checkResults += Invoke-ProjectCheck $serviceProjects[$serviceName] "backend.$($serviceName.ToLowerInvariant()).tests" "$serviceName backend tests" (Get-ServiceJourney $serviceName) "service" $serviceName -WithCoverage
    }
}
else {
    $serviceName = $Scope.Substring("backend:".Length)
    $checkResults += Invoke-ProjectCheck $sharedProject "backend.shared.test-infrastructure" "Shared backend test infrastructure" "shared-backend-baseline" "service" "HiveSpace.Testing.Shared"
    $checkResults += Invoke-ProjectCheck $serviceProjects[$serviceName] "backend.$($serviceName.ToLowerInvariant()).tests" "$serviceName backend tests" (Get-ServiceJourney $serviceName) "service" $serviceName -WithCoverage
}

if ($Scope -eq "backend:OrderService" -or $Scope -eq "release") {
    $gapCheckId = "backend.order.seller-coupon-promotion-coverage"
    if ([string]::IsNullOrWhiteSpace($AcceptedRiskScope)) {
        $coverageGaps += New-CoverageGap `
            "seller-coupon-promotion" `
            "Backend has coupon domain coverage; frontend promotion surface is outside backend-only scope." `
            "medium" `
            "Complete frontend F009 or provide an accepted release risk."
    }
    else {
        $acceptedRisks += New-AcceptedRisk `
            $gapCheckId `
            $AcceptedRiskScope `
            $AcceptedRiskApprovingRole `
            $AcceptedRiskReason `
            $AcceptedRiskExpiresWhen

        $checkResults += New-CheckResult `
            $gapCheckId `
            "Seller coupon promotion accepted coverage gap" `
            "seller-coupon-promotion" `
            "seller" `
            "OrderService" `
            "accepted_risk" `
            "accepted_coverage_gap" `
            "blocked_until_risk_accepted" `
            0 `
            "Accepted risk recorded for backend-visible coupon promotion gap" `
            $null
    }
}

$completedAt = (Get-Date).ToUniversalTime()
$report = [ordered]@{
    gate = [ordered]@{
        scope = $Scope
        result = Get-OverallResult $checkResults
        startedAt = $startedAt.ToString("o")
        completedAt = $completedAt.ToString("o")
        durationSeconds = [math]::Round(($completedAt - $startedAt).TotalSeconds, 3)
    }
    checkResults = $checkResults
    coverageGaps = $coverageGaps
    acceptedRisks = $acceptedRisks
}

$report | ConvertTo-Json -Depth 8
