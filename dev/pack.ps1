param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$Configuration = "Release",

    [string]$Output = "artifacts/nupkg",

    [string]$PackageVersion
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot $Project
$outputPath = if ([System.IO.Path]::IsPathRooted($Output)) { $Output } else { Join-Path $repoRoot $Output }

if (-not (Test-Path $projectPath)) {
    throw "Project not found: $Project"
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

$arguments = @(
    "pack"
    $projectPath
    "--configuration"
    $Configuration
    "--output"
    $outputPath
    "--nologo"
    "-p:ContinuousIntegrationBuild=true"
)

if ($PackageVersion) {
    $arguments += "-p:PackageVersion=$PackageVersion"
}

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}