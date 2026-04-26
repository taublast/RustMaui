param(
    [string]$Output = "artifacts/nupkg"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Tool/RustMaui.Tool.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Generators/RustMaui.Generators.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& (Join-Path $scriptRoot "pack.ps1") -Project "src/RustMaui.Templates/RustMaui.Templates.csproj" -Output $Output
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}