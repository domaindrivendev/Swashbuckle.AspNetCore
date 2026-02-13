#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Framework = "net10.0",
    [Parameter(Mandatory = $false)][string] $Job = ""
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$benchmarks = (Join-Path $PSScriptRoot "perf" "Swashbuckle.AspNetCore.Benchmarks" "Swashbuckle.AspNetCore.Benchmarks.csproj")

$additionalArgs = @()

if (-Not [string]::IsNullOrEmpty($Job)) {
    $additionalArgs += "--job"
    $additionalArgs += $Job
}

if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
    $additionalArgs += "--exporters"
    $additionalArgs += "json"
}

dotnet run --project $benchmarks --configuration "Release" --framework $Framework -- $additionalArgs --% --filter *
