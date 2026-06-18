param(
    [Parameter(Mandatory = $true)]
    [string]$CoverageFile,
    [Parameter(Mandatory = $true)]
    [string]$RepoRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$typeMapCache = @{}

function Get-ResolvedSourcePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $normalized = $RelativePath -replace "[/\\]", [IO.Path]::DirectorySeparatorChar
    return [IO.Path]::GetFullPath((Join-Path $RepoRoot $normalized))
}

function Get-TypeKindMap {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath
    )

    if ($typeMapCache.ContainsKey($SourcePath)) {
        return $typeMapCache[$SourcePath]
    }

    $map = @{}
    if (Test-Path $SourcePath) {
        $content = Get-Content $SourcePath -Raw
        $matches = [regex]::Matches(
            $content,
            "(?m)^\s*(?:file\s+)?(?:(?:public|internal|private|protected|sealed|abstract|static|partial|readonly)\s+)*(?<kind>record\s+class|record\s+struct|record|class|interface|enum|struct)\s+(?<name>@?[A-Za-z_][A-Za-z0-9_]*)"
        )

        foreach ($match in $matches) {
            $name = $match.Groups["name"].Value.TrimStart("@")
            if (-not $map.ContainsKey($name)) {
                $map[$name] = $match.Groups["kind"].Value
            }
        }
    }

    $typeMapCache[$SourcePath] = $map
    return $map
}

function Get-SimpleTypeName {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ClassName
    )

    $leaf = ($ClassName -split "[.+/]")[-1]
    return ($leaf -replace '`[0-9]+$', '')
}

function Test-DataOnlyType {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$ClassNode
    )

    $methods = @($ClassNode.SelectNodes("./methods/method"))
    if ($methods.Count -eq 0) {
        return $true
    }

    foreach ($method in $methods) {
        $name = [string]$method.GetAttribute("name")
        if ($name -notmatch '^(\.ctor|\.cctor|get_|set_|init_)') {
            return $false
        }
    }

    return $true
}

function Get-ClassCoverageStats {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$ClassNode
    )

    $linesCovered = 0
    $linesValid = 0
    $branchesCovered = 0
    $branchesValid = 0

    $lines = @($ClassNode.SelectNodes(".//line"))
    foreach ($line in $lines) {
        $linesValid++
        if ([int]$line.GetAttribute("hits") -gt 0) {
            $linesCovered++
        }

        $conditionCoverage = [string]$line.GetAttribute("condition-coverage")
        if ($conditionCoverage -match "\((\d+)/(\d+)\)") {
            $branchesCovered += [int]$matches[1]
            $branchesValid += [int]$matches[2]
        }
    }

    $complexity = 0.0
    $complexityValue = [string]$ClassNode.GetAttribute("complexity")
    if ($complexityValue) {
        $complexity = [double]$complexityValue
    }

    return @{
        LinesCovered = $linesCovered
        LinesValid = $linesValid
        BranchesCovered = $branchesCovered
        BranchesValid = $branchesValid
        Complexity = $complexity
    }
}

function Set-PackageCoverageStats {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$PackageNode
    )

    $linesCovered = 0
    $linesValid = 0
    $branchesCovered = 0
    $branchesValid = 0
    $complexity = 0.0

    $classes = @($PackageNode.SelectNodes("./classes/class"))
    foreach ($classNode in $classes) {
        $stats = Get-ClassCoverageStats -ClassNode $classNode
        $linesCovered += $stats.LinesCovered
        $linesValid += $stats.LinesValid
        $branchesCovered += $stats.BranchesCovered
        $branchesValid += $stats.BranchesValid
        $complexity += $stats.Complexity
    }

    $lineRate = if ($linesValid -eq 0) { 0.0 } else { $linesCovered / $linesValid }
    $branchRate = if ($branchesValid -eq 0) { 0.0 } else { $branchesCovered / $branchesValid }

    $PackageNode.SetAttribute("line-rate", $lineRate.ToString([Globalization.CultureInfo]::InvariantCulture))
    $PackageNode.SetAttribute("branch-rate", $branchRate.ToString([Globalization.CultureInfo]::InvariantCulture))
    $PackageNode.SetAttribute("complexity", $complexity.ToString([Globalization.CultureInfo]::InvariantCulture))

    return @{
        LinesCovered = $linesCovered
        LinesValid = $linesValid
        BranchesCovered = $branchesCovered
        BranchesValid = $branchesValid
        Complexity = $complexity
    }
}

[xml]$document = Get-Content $CoverageFile
$packagesNode = $document.coverage.packages
if ($null -eq $packagesNode) {
    return
}

$packages = @($packagesNode.SelectNodes("./package"))
foreach ($packageNode in $packages) {
    $classNodes = @($packageNode.SelectNodes("./classes/class"))
    foreach ($classNode in $classNodes) {
        $excludeClass = Test-DataOnlyType -ClassNode $classNode

        if (-not $excludeClass) {
            $filename = [string]$classNode.GetAttribute("filename")
            if ($filename) {
                $sourcePath = Get-ResolvedSourcePath -RelativePath $filename
                $typeMap = Get-TypeKindMap -SourcePath $sourcePath
                $simpleName = Get-SimpleTypeName -ClassName ([string]$classNode.GetAttribute("name"))
                if ($typeMap.ContainsKey($simpleName)) {
                    $kind = [string]$typeMap[$simpleName]
                    if ($kind -in @("interface", "enum")) {
                        $excludeClass = $true
                    }
                }
            }
        }

        if ($excludeClass) {
            [void]$classNode.ParentNode.RemoveChild($classNode)
        }
    }

    if ($packageNode.SelectNodes("./classes/class").Count -eq 0) {
        [void]$packageNode.ParentNode.RemoveChild($packageNode)
        continue
    }

    [void](Set-PackageCoverageStats -PackageNode $packageNode)
}

$rootLinesCovered = 0
$rootLinesValid = 0
$rootBranchesCovered = 0
$rootBranchesValid = 0

$remainingPackages = @($packagesNode.SelectNodes("./package"))
foreach ($packageNode in $remainingPackages) {
    $stats = Set-PackageCoverageStats -PackageNode $packageNode
    $rootLinesCovered += $stats.LinesCovered
    $rootLinesValid += $stats.LinesValid
    $rootBranchesCovered += $stats.BranchesCovered
    $rootBranchesValid += $stats.BranchesValid
}

$rootLineRate = if ($rootLinesValid -eq 0) { 0.0 } else { $rootLinesCovered / $rootLinesValid }
$rootBranchRate = if ($rootBranchesValid -eq 0) { 0.0 } else { $rootBranchesCovered / $rootBranchesValid }

$document.coverage.SetAttribute("lines-covered", $rootLinesCovered.ToString([Globalization.CultureInfo]::InvariantCulture))
$document.coverage.SetAttribute("lines-valid", $rootLinesValid.ToString([Globalization.CultureInfo]::InvariantCulture))
$document.coverage.SetAttribute("branches-covered", $rootBranchesCovered.ToString([Globalization.CultureInfo]::InvariantCulture))
$document.coverage.SetAttribute("branches-valid", $rootBranchesValid.ToString([Globalization.CultureInfo]::InvariantCulture))
$document.coverage.SetAttribute("line-rate", $rootLineRate.ToString([Globalization.CultureInfo]::InvariantCulture))
$document.coverage.SetAttribute("branch-rate", $rootBranchRate.ToString([Globalization.CultureInfo]::InvariantCulture))

$writer = New-Object System.IO.StreamWriter($CoverageFile, $false, [System.Text.UTF8Encoding]::new($false))
try {
    $document.Save($writer)
}
finally {
    $writer.Dispose()
}
