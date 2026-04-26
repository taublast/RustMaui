# Community.MauiRust

This repository now carries both NuGet packages that make up the MAUI + Rust workflow:

- `Community.MauiRust.Generators`: build-time package that discovers Rust exports, generates `Rust.Generated.cs`, and wires Rust native build targets.
- `Community.MauiRust.Templates`: `dotnet new` template package that scaffolds a MAUI app already configured to use the generator package.

The local folder is still `MauiRust.Templates` for now. The intended repository identity is `Community.MauiRust` once the remote rename is done.

## Repository layout

```text
Community.MauiRust/
├── Community.MauiRust.slnx
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── build/
├── eng/
├── nugets/
├── src/
│   ├── Community.MauiRust.Generators/
│   └── Community.MauiRust.Templates/
```

## Packages

`src/Community.MauiRust.Generators/Community.MauiRust.Generators.csproj`

- ships the generator/build package
- packs `build/Community.MauiRust.Generators.targets`
- produces the analyzer/build assets consumed by MAUI apps

`src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj`

- ships the `dotnet new maui-rust` template
- carries the scaffold under `src/Community.MauiRust.Templates/content/MauiRust`
- emits a scaffolded app that references `Community.MauiRust.Generators`

## Generated template shape

The template produces a repo with this top-level layout:

```text
MyApp/
├── MauiRust.sln
├── check-prerequisites.ps1
├── check-prerequisites.sh
├── Prerequisites.md
├── app/
├── rust/
└── src/
```

The generated app project lives under `src/<AppName>/<AppName>.csproj`.

## Local commands

Pack both packages:

```powershell
.\eng\pack-all.ps1
```

Pack one package:

```powershell
.\eng\pack.ps1 -Project src/Community.MauiRust.Generators/Community.MauiRust.Generators.csproj
```

Validate the template against locally packed packages:

```powershell
.\eng\validate-template.ps1
```

That validator packs both packages, installs the local template package, generates a temporary app outside the repo tree, adds the local package folder as a NuGet source, and runs a Windows build against the generated app.

See `nugets/nugets.md` for the shared GitHub Actions release workflow and required secrets.