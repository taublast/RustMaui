$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$tempRoot = Join-Path (Join-Path ([System.IO.Path]::GetTempPath()) "RustMaui") "validate-template"
$packagesDir = Join-Path $tempRoot "nupkg"
$globalPackagesRoot = Join-Path $HOME ".nuget\packages"

Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

& (Join-Path $PSScriptRoot "pack-all.ps1") -Output $packagesDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$resolvedPackagesDir = (Resolve-Path $packagesDir).Path
$sourceList = dotnet nuget list source | Out-String
if ($sourceList -notmatch [Regex]::Escape($resolvedPackagesDir)) {
    dotnet nuget add source $resolvedPackagesDir --name rustmaui-local
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Remove-Item (Join-Path $globalPackagesRoot "rustmaui") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $globalPackagesRoot "rustmaui.generators") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $globalPackagesRoot "rustmaui.templates") -Recurse -Force -ErrorAction SilentlyContinue

$templatePackage = Get-ChildItem $packagesDir -Filter "RustMaui.Templates.*.nupkg" |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $templatePackage) {
    throw "RustMaui.Templates package not found in $packagesDir"
}

$legacyTemplateId = @('Community', 'MauiRust', 'Templates') -join '.'
$installedTemplatePackages = dotnet new uninstall | Out-String
if ($installedTemplatePackages -match 'RustMaui\.Templates') {
    dotnet new uninstall RustMaui.Templates | Out-Null
}

if ($installedTemplatePackages -match [Regex]::Escape($legacyTemplateId)) {
    dotnet new uninstall $legacyTemplateId | Out-Null
}

dotnet new install $templatePackage.FullName
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$appDir = Join-Path $tempRoot "TestApp"
dotnet new rustmaui -n TestApp -o $appDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet build (Join-Path $appDir "src/TestApp/TestApp.csproj") -f net10.0-windows10.0.19041.0 -p:ManagePackageVersionsCentrally=false
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}