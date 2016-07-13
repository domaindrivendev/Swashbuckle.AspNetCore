Properties {

    # This number will be used to replace the * in all versions of all libraries.
    # This should be overwritten by a CI system like VSTS, AppVeyor, TeamCity, ...
    $BuildNumber = "local" + ((Get-Date).ToUniversalTime().ToString("yyyyMMddHHmm"))

    # The build configuration used for compilation
    $BuildConfiguration = "Release"

    # The folder in which all output artifacts should be placed
    $ArtifactsPath = "artifacts"

    # Artifacts-subfolder in which test results should be placed
    $ArtifactsPathTests = "tests"

    # Artifacts-subfolder in which NuGet packages should be placed
    $ArtifactsPathNuGet = "nuget"

    # A list of projects for which NuGet packages should be created
    $NugetLibraries = @(
        "src/Swashbuckle",
        "src/Swashbuckle.Swagger",
        "src/Swashbuckle.SwaggerGen",
        "src/Swashbuckle.SwaggerUi"
    )
}

FormatTaskName ("`n" + ("-"*25) + "[{0}]" + ("-"*25) + "`n")

Task Default -depends init, clean, dotnet-install, dotnet-restore, dotnet-build, dotnet-test, dotnet-pack

Task init {

    Write-Host "BuildNumber: $BuildNumber"
    Write-Host "BuildConfiguration: $BuildConfiguration"
    Write-Host "ArtifactsPath: $ArtifactsPath"
    Write-Host "ArtifactsPathTests: $ArtifactsPathTests"
    Write-Host "ArtifactsPathNuGet: $ArtifactsPathNuGet"

    Assert ($BuildNumber -ne $null) "Property 'BuildNumber' may not be null."
    Assert ($BuildConfiguration -ne $null) "Property 'BuildConfiguration' may not be null."
    Assert ($ArtifactsPath -ne $null) "Property 'ArtifactsPath' may not be null."
    Assert ($ArtifactsPathTests -ne $null) "Property 'ArtifactsPathTests' may not be null."
    Assert ($ArtifactsPathNuGet -ne $null) "Property 'ArtifactsPathNuGet' may not be null."
}

Task clean {

    if (Test-Path $ArtifactsPath) { Remove-Item -Path $ArtifactsPath -Recurse -Force -ErrorAction Ignore }
    New-Item $ArtifactsPath -ItemType Directory -ErrorAction Ignore | Out-Null

    Write-Host "Created artifacts folder '$ArtifactsPath'"
}

Task dotnet-install {

    if (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) {
        Write-Host "dotnet SDK already installed"
        exec { dotnet --version }
    } else {
        Write-Host "Installing dotnet SDK"
        
        $installScript = Join-Path $ArtifactsPath "dotnet-install.ps1"
        
        Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/install.ps1" `
            -OutFile $installScript
            
        & $installScript
    }
}

Task dotnet-restore {

    exec { dotnet restore -v Minimal }
}

Task dotnet-build {

    exec { dotnet build **\project.json -c $BuildConfiguration --version-suffix $BuildNumber }
}

Task dotnet-test {

    $testOutput = Join-Path $ArtifactsPath $ArtifactsPathTests
    New-Item $testOutput -ItemType Directory -ErrorAction Ignore | Out-Null

    # Find every library that has a configured testRunner and test it.
    # Run all libraries, even if one fails to make sure we have test results for all libraries.

    $testFailed = $false

    Get-ChildItem -Filter project.json -Recurse | ForEach-Object {

        $projectJson = Get-Content -Path $_.FullName -Raw -ErrorAction Ignore | ConvertFrom-Json -ErrorAction Ignore

        if ($projectJson -and $projectJson.testRunner -ne $null)
        {
            $library = Split-Path $_.DirectoryName -Leaf
            $testResultOutput = Join-Path $testOutput "$library.xml"

            Write-Host ""
            Write-Host "Testing $library"
            Write-Host ""

            dotnet test $_.Directory -c $BuildConfiguration --no-build -xml $testResultOutput

            if ($LASTEXITCODE -ne 0) {
                $testFailed = $true
            }
        }
    }

    if ($testFailed) {
        throw "Tests for at least one library failed"
    }
}

Task dotnet-pack {

    if ($NugetLibraries -eq $null -or $NugetLibraries.Count -eq 0) {
        Write-Host "No NugetLibraries configured"
        return
    }

    $NugetLibraries | ForEach-Object {

        $library = $_
        $libraryOutput = Join-Path $ArtifactsPath $ArtifactsPathNuGet

        Write-Host ""
        Write-Host "Packaging $library to $libraryOutput"
        Write-Host ""

        exec { dotnet pack $library -c $BuildConfiguration --version-suffix $BuildNumber --no-build -o $libraryOutput }
    }
}