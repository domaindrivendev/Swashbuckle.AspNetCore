#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

dotnet pack --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
  throw "dotnet pack failed with exit code $LASTEXITCODE"
}

$additionalArgs = @()

if (![string]::IsNullOrEmpty($env:GITHUB_SHA)) {
    $additionalArgs += "--logger:GitHubActions;report-warnings=false"
    $additionalArgs += "--logger:junit;LogFilePath=junit.xml"
}

dotnet test --configuration $Configuration $additionalArgs

if ($LASTEXITCODE -ne 0) {
  throw "dotnet test failed with exit code $LASTEXITCODE"
}
