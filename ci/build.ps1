# Version suffix - use preview suffix for CI builds that are not tagged (i.e. unofficial)
$VersionSuffix = ""
if ($env:APPVEYOR -eq "True" -and $env:APPVEYOR_REPO_TAG -eq "false") {
    $VersionSuffix = "preview-" + $env:APPVEYOR_BUILD_NUMBER.PadLeft(4, '0')
}

# Target folder for build artifacts (e.g. nugets)
$ArtifactsPath = "$(pwd)" + "\artifacts"

function install-dotnet-core {
    if ($env:APPVEYOR -eq "True") {
        $env:DOTNET_INSTALL_DIR = Join-Path "$(Convert-Path "$PSScriptRoot")" ".dotnetcli"
        mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        & $installScript -Version "$env:DOTNET_VERSION" -InstallDir "$env:DOTNET_INSTALL_DIR"
    }
}

function install-swagger-ui {
    Push-Location src/Swashbuckle.AspNetCore.SwaggerUI
    npm install
    Pop-Location
}

function install-redoc {
    Push-Location src/Swashbuckle.AspNetCore.ReDoc
    npm install
    Pop-Location
}

function dotnet-build {
    if ($VersionSuffix.Length -gt 0) {
        dotnet build -c Release --version-suffix $VersionSuffix
    }
    else {
        dotnet build -c Release
    }
}

function dotnet-pack {
    Get-ChildItem -Path src/** | ForEach-Object {
        if ($VersionSuffix.Length -gt 0) {
            dotnet pack $_ -c Release --no-build -o $ArtifactsPath --version-suffix $VersionSuffix
        }
        else {
            dotnet pack $_ -c Release --no-build -o $ArtifactsPath
        }
    }
}

@( "install-dotnet-core", "install-swagger-ui", "install-redoc", "dotnet-build", "dotnet-pack" ) | ForEach-Object {
    echo ""
    echo "***** $_ *****"
    echo ""

    # Invoke function and exit on error
    &$_
    if ($LastExitCode -ne 0) { Exit $LastExitCode }
}
