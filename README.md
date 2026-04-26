# Community.MauiRust

Monorepo for the two NuGet packages that make up the current MAUI + Rust experience:

- `Community.MauiRust.Generators`: build-time package that discovers Rust exports, generates `Rust.Generated.cs`, and wires Rust native build targets.
- `Community.MauiRust.Templates`: `dotnet new` template package that scaffolds a MAUI app already configured to use the generator package.

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
│   └── Community.MauiRust.Generators/
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
- references the generator package version in the scaffolded app

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

See `nugets/nugets.md` for the GitHub Actions release workflow and secrets.