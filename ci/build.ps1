# Version suffix - use preview suffix for CI builds that are not tagged (i.e. unofficial)
$VersionSuffix = ""
if ($env:APPVEYOR -eq "True" -and $env:APPVEYOR_REPO_TAG -eq "false") {
    $VersionSuffix = "preview-" + $env:APPVEYOR_BUILD_NUMBER.PadLeft(4, '0')
}

# Target folder for build artifacts (e.g. nugets)
$ArtifactsPath = "$(pwd)" + "\artifacts"

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
    Get-ChildItem -Path src/** -Directory | ForEach-Object {
        if ($VersionSuffix.Length -gt 0) {
            dotnet pack $_ -c Release --no-build -o $ArtifactsPath --version-suffix $VersionSuffix
        }
        else {
            dotnet pack $_ -c Release --no-build -o $ArtifactsPath
        }
    }
}

@( "install-swagger-ui", "install-redoc", "dotnet-build", "dotnet-pack" ) | ForEach-Object {
    echo ""
    echo "***** $_ *****"
    echo ""

    # Invoke function and exit on error
    &$_
    if ($LastExitCode -ne 0) { Exit $LastExitCode }
}
