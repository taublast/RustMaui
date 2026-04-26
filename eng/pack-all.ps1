$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

& (Join-Path $scriptRoot "pack.ps1") -Project "src/Community.MauiRust.Generators/Community.MauiRust.Generators.csproj"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& (Join-Path $scriptRoot "pack.ps1") -Project "src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}