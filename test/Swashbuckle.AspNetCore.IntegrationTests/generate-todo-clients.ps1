#! /usr/bin/env pwsh

param(
    [Parameter(Mandatory = $false)][string] $OpenApiDocument = "./snapshots/VerifyTests.Swagger_IsValidJson_No_Startup_entryPointType=TodoApp.Program_swaggerRequestUri=v1.DotNet10_0.verified.txt",
    [Parameter(Mandatory = $false)][switch] $Regenerate
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$env:KIOTA_TUTORIAL_ENABLED = "false"

dotnet kiota generate `
    --additional-data false `
    --class-name KiotaTodoApiClient `
    --clean-output `
    --exclude-backward-compatible `
    --language csharp `
    --namespace-name TodoApp.KiotaClient `
    --openapi $OpenApiDocument `
    --output "./KiotaTodoClient" `
    --structured-mime-types "application/json"

if ($LASTEXITCODE -ne 0) {
    throw "Kiota client generation failed with exit code ${LASTEXITCODE}"
}

dotnet nswag openapi2csclient `
    "/input:${OpenApiDocument}" `
    /classname:NSwagTodoApiClient `
    /namespace:TodoApp.NSwagClient `
    /output:./NSwagTodoClient/NSwagTodoClient.cs

if ($LASTEXITCODE -ne 0) {
    throw "NSwag client generation failed with exit code ${LASTEXITCODE}"
}
