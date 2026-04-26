$ErrorActionPreference = "Continue"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$packagesDir = Join-Path $repoRoot "artifacts/nupkg"
$tempRoot = Join-Path (Join-Path ([System.IO.Path]::GetTempPath()) "RustMaui") "validate-template"

Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

& (Join-Path $PSScriptRoot "pack-all.ps1")
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

dotnet nuget remove source rustmaui-local 2>$null; $global:LASTEXITCODE = 0
dotnet nuget add source $packagesDir --name rustmaui-local 2>$null; $global:LASTEXITCODE = 0
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$templatePackage = Get-ChildItem $packagesDir -Filter "RustMaui.Templates.*.nupkg" |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $templatePackage) {
    throw "RustMaui.Templates package not found in $packagesDir"
}

dotnet new uninstall RustMaui.Templates 2>$null; $global:LASTEXITCODE = 0

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
