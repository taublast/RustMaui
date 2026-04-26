$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$packagesDir = Join-Path $repoRoot "artifacts/nupkg"
$tempRoot = Join-Path (Join-Path ([System.IO.Path]::GetTempPath()) "Community.MauiRust") "validate-template"

Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

& (Join-Path $PSScriptRoot "pack-all.ps1")
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet nuget remove source community-mauirust-local 2>$null
dotnet nuget add source $packagesDir --name community-mauirust-local
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$templatePackage = Get-ChildItem $packagesDir -Filter "Community.MauiRust.Templates.*.nupkg" |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $templatePackage) {
    throw "Community.MauiRust.Templates package not found in $packagesDir"
}

dotnet new uninstall Community.MauiRust.Templates 2>$null

dotnet new install $templatePackage.FullName
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$appDir = Join-Path $tempRoot "TestApp"
dotnet new maui-rust -n TestApp -o $appDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet build (Join-Path $appDir "src/TestApp/TestApp.csproj") -f net10.0-windows10.0.19041.0 -p:ManagePackageVersionsCentrally=false
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}