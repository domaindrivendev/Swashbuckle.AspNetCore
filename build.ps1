#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$coverageOutputPath = Join-Path $solutionPath "artifacts" "coverage"

if ([string]::IsNullOrEmpty(${env:GITHUB_ACTIONS})) {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed with exit code $LASTEXITCODE"
    }
}

dotnet pack --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
  throw "dotnet pack failed with exit code $LASTEXITCODE"
}

dotnet test --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
  throw "dotnet test failed with exit code $LASTEXITCODE"
}

$projectCoverageDirectories = Get-ChildItem -Path $coverageOutputPath -Directory -ErrorAction SilentlyContinue

foreach ($projectCoverageDirectory in $projectCoverageDirectories) {
    $projectName = $projectCoverageDirectory.Name
    $projectCoveragePath = $projectCoverageDirectory.FullName

    $coverageReports = @(
        Get-ChildItem -Path $projectCoveragePath -File -Filter "coverage.*.xml" -ErrorAction SilentlyContinue |
            Sort-Object -Property Name |
            Select-Object -ExpandProperty FullName
    )

    if ($coverageReports.Count -eq 0) {
        continue
    }

    $reportTypes = @("HTML")

    if (-Not [string]::IsNullOrEmpty(${env:GITHUB_STEP_SUMMARY})) {
        $reportTypes += "MarkdownSummaryGitHub"
    }

    $reportGeneratorArgs = @(
        "-reports:$($coverageReports -join ';')"
        "-targetdir:$projectCoveragePath"
        "-reporttypes:$($reportTypes -join ';')"
        "-title:$projectName"
        "-verbosity:Warning"
    )

    dotnet tool run reportgenerator @reportGeneratorArgs

    if ($LASTEXITCODE -ne 0) {
        throw "reportgenerator failed with exit code $LASTEXITCODE"
    }

    $coverageGitHubSummary = Join-Path $projectCoveragePath "SummaryGithub.md"

    if (-Not [string]::IsNullOrEmpty(${env:GITHUB_STEP_SUMMARY}) -and (Test-Path -LiteralPath $coverageGitHubSummary)) {
        $summaryContent = @(
            "<details><summary>:chart_with_upwards_trend: <b>$projectName Code Coverage report</b></summary>"
            ""
            (Get-Content -LiteralPath $coverageGitHubSummary -Raw)
            ""
            "</details>"
        ) -join [System.Environment]::NewLine

        try {
            Add-Content -LiteralPath ${env:GITHUB_STEP_SUMMARY} -Value $summaryContent
        }
        catch {
            Write-Warning "Failed to write GitHub step summary for ${projectName}: $_"
        }
    }
}
