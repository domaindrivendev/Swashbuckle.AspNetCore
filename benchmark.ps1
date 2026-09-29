#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [string]$Configuration = "Release",
    [string]$Framework = "net10.0",
    [Parameter(Mandatory = $false)][string] $Job = "",
    [Parameter(Mandatory = $false)][string[]] $Runtimes = @("net10.0"),
    [Parameter(Mandatory = $false)][string] $Affinity = "",
    [Parameter(Mandatory = $false)][string] $Filter = "*",
    [Parameter(Mandatory = $false)][switch] $EnableMemoryDiagnoser,
    [Parameter(Mandatory = $false)][switch] $EnableEventPipeProfiler
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$benchmarks = (Join-Path $PSScriptRoot "perf" "Swashbuckle.AspNetCore.Benchmarks" "Swashbuckle.AspNetCore.Benchmarks.csproj")

$additionalArgs = @()

$additionalArgs += "--runtimes"
$additionalArgs += $Runtimes

if (-Not [string]::IsNullOrEmpty($Job)) {
    $additionalArgs += "--job"
    $additionalArgs += $Job
}

if (-Not [string]::IsNullOrEmpty($Affinity)) {
    $additionalArgs += "--affinity"
    $additionalArgs += $Affinity
}

if (-Not [string]::IsNullOrEmpty($Filter)) {
    $additionalArgs += "--filter"
    $additionalArgs += $Filter
}

if ($EnableMemoryDiagnoser) {
    $additionalArgs += "--memory"
}

if ($EnableEventPipeProfiler) {
    $additionalArgs += "--profiler"
    $additionalArgs += "EP"
}

if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
    $additionalArgs += "--exporters"
    $additionalArgs += "json"
}

$dotnetArgs = @(
    "run"
    "--configuration", $Configuration
    "--framework", $Framework
    "--project", $benchmarks
    "--"
) + $additionalArgs

$p = Start-Process -FilePath "dotnet" -ArgumentList $dotnetArgs -NoNewWindow -PassThru
$p.WaitForExit()

if ($p.ExitCode -ne 0) {
    throw "Benchmarks failed with exit code $($p.ExitCode)."
}
