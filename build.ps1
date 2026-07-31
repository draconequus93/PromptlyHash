param(
    [switch]$Msi,
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
$projectRoot = $PSScriptRoot
$publishDirectory = Join-Path $projectRoot "artifacts\publish\win-x64"

& (Join-Path $projectRoot "tools\Generate-Icon.ps1")
dotnet build (Join-Path $projectRoot "PromptlyHash.sln") `
    -c Release `
    -p:Version=$Version
if ($LASTEXITCODE -ne 0) {
    throw "Solution build failed with exit code $LASTEXITCODE."
}

dotnet run --project (Join-Path $projectRoot "tests\PromptlyHash.Tests\PromptlyHash.Tests.csproj") `
    -c Release `
    --no-build
if ($LASTEXITCODE -ne 0) {
    throw "Automated checks failed with exit code $LASTEXITCODE."
}

dotnet publish (Join-Path $projectRoot "src\PromptlyHash\PromptlyHash.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Executable publishing failed with exit code $LASTEXITCODE."
}

Write-Host "Executable created at: $publishDirectory\PromptlyHash.exe"

if ($Msi) {
    dotnet build (Join-Path $projectRoot "installer\PromptlyHash.wixproj") `
        -c Release `
        -p:PublishDirectory=$publishDirectory `
        -p:ProductVersion=$Version
    if ($LASTEXITCODE -ne 0) {
        throw "MSI packaging failed with exit code $LASTEXITCODE."
    }

    Write-Host "MSI created in: $projectRoot\installer\bin\x64\Release"
}
