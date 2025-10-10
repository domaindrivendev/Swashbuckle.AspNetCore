#! /usr/bin/env pwsh
param(
    [Parameter(Mandatory = $false)][string] $NextVersion
)

$ErrorActionPreference = "Stop"

if ($NextVersion.StartsWith("v")) {
    $NextVersion = $NextVersion.Substring(1)
}

$repo = Join-Path $PSScriptRoot ".."
$properties = Join-Path $repo "Directory.Build.props"

$xml = [xml](Get-Content $properties)
$versionPrefix = $xml.SelectSingleNode('Project/PropertyGroup/VersionPrefix')
$publishedVersion = $versionPrefix.InnerText

if (-Not [string]::IsNullOrEmpty($NextVersion)) {
  $version = [System.Version]::new($NextVersion)
} else {
  $version = [System.Version]::new($publishedVersion)
  $version = [System.Version]::new($version.Major, $version.Minor, $version.Build + 1)
}

$updatedVersion = $version.ToString()
$versionPrefix.InnerText = $updatedVersion

$packageValidationBaselineVersion = $xml.SelectSingleNode('Project/PropertyGroup/PackageValidationBaselineVersion')
$packageValidationBaselineVersion.InnerText = $publishedVersion

Write-Output "Bumping version from $publishedVersion to $version"

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)
$settings.Indent = $true
$settings.OmitXmlDeclaration = $true

try {
    $writer = [System.Xml.XmlWriter]::Create($properties, $settings)
    $xml.Save($writer)
} finally {
    if ($writer) {
        $writer.Flush()
        $writer.Dispose()
        "" >> $properties
    }
    $writer = $null
}

$githubOutput = ${env:GITHUB_OUTPUT}

if (($null -ne $githubOutput) -and (Test-Path $githubOutput)) {
  "version=${updatedVersion}" >> $githubOutput
}
