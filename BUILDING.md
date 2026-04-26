# Building

This repo now builds and validates three packages together:

- `RustMaui`
- `RustMaui.Generators`
- `RustMaui.Templates`

## Prerequisites

- .NET SDK from `global.json`
- Rust toolchain when validating a generated project end to end
- MAUI workloads only when building an instantiated app from the template

## Local package build

Pack all NuGet packages to `artifacts/nupkg`:

```powershell
.\dev\pack-all.ps1
```

Pack a single package:

```powershell
.\dev\pack.ps1 -Project src/RustMaui.Templates/Community.MauiRust.Templates.csproj
```

or:

```powershell
.\dev\pack.ps1 -Project src/RustMaui.Generators/Community.MauiRust.Generators.csproj
```

## Local template validation

```powershell
.\dev\validate-template.ps1
```

That script packs all three packages, installs the template from the local `.nupkg`, adds the local package folder as a NuGet source, creates a temporary test app, and runs a Windows build.
That script uses `dotnet new rustmaui` and validates the local `RustMaui.Generators` package through the generated app build.

The generated app is created outside the repo tree so root `Directory.Packages.props` does not leak into the smoke test.