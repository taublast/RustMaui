param(
    [string]$Output = "artifacts/nupkg"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Tool/Community.MauiRust.Tool.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Generators/Community.MauiRust.Generators.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Templates/Community.MauiRust.Templates.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
