# Building

## Prerequisites

- .NET SDK from `global.json`
- Rust toolchain when validating a generated project end to end
- MAUI workloads only when building an instantiated app from the template

## Local package build

Pack both NuGet packages to `artifacts/nupkg`:

```powershell
.\eng\pack-all.ps1
```

Pack a single package:

```powershell
.\eng\pack.ps1 -Project src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj
```

## Local template validation

```powershell
.\eng\validate-template.ps1
```

That script packs both packages, installs the template from the local `.nupkg`, adds the local package folder as a NuGet source, creates a temporary test app, and runs a Windows build.