<div align="center">

# RustMaui

**Rust superpowers for .NET MAUI** 🍬🌴

<img width="800" alt="RustMaui" src="https://github.com/user-attachments/assets/6869bd41-22d9-4193-b2fa-48552593f8ed" />

</div>

---

**RustMaui** brings the **performance, safety, and joy of Rust** directly into your .NET MAUI applications, includes everything you need:

- A friendly `dotnet tool` for instant setup
- Automatic build-time Rust bindings generation
- Optional `dotnet new` template for new projects

## Quick Start

### Install the tool

```bash
dotnet tool install --global RustMaui
```

### Create a new MAUI + Rust app

```bash
rustmaui new MyAwesomeApp
```
### Or add Rust to an existing MAUI project

```bash
rustmaui init .
```

## Packages

### `RustMaui`

The primary entry point: a .NET tool for both greenfield and existing-app setup.

- install with `dotnet tool install --global RustMaui`
- run `rustmaui new MyApp` to create a new app from the shared scaffold
- run `rustmaui init path/to/MyApp.csproj` to add Rust boilerplate and `RustMaui.Generators` to an existing app

Package docs: [src/RustMaui.Tool/README.md](src/RustMaui.Tool/README.md).

### `RustMaui.Generators`

Build-time package that discovers Rust exports, generates `Rust.Generated.cs`, and wires Rust native build targets.

- ships the generator/build package
- packs `build/RustMaui.Generators.targets`
- produces the analyzer/build assets consumed by MAUI apps

Package docs: [src/RustMaui.Generators/README.md](src/RustMaui.Generators/README.md).


### `RustMaui.Templates`

An optional `dotnet new` template package for new-app-only flows. It scaffolds a MAUI app already configured to use the generator package. 

- ships the `dotnet new rustmaui` template
- carries the scaffold under `src/RustMaui.Templates/content/MauiRust`
- emits a scaffolded app that references `RustMaui.Generators`

Package docs: [src/RustMaui.Templates/README.md](src/RustMaui.Templates/README.md)

---

## Contribute

Please feel free to bring in your works, we need you!

### Development Commands

Pack all three packages:

```powershell
.\dev\pack-all.ps1
```

Pack one package:

```powershell
.\dev\pack.ps1 -Project src/RustMaui.Generators/Community.MauiRust.Generators.csproj
```

Validate the template against locally packed packages:

```powershell
.\dev\validate-template.ps1
```

That validator packs all three packages, installs the local template package, generates a temporary app outside the repo tree, reuses the local package folder as a NuGet source, and runs a Windows build against the generated app.

The template remains available, but the preferred install path for end users is `dotnet tool install --global RustMaui`.

See [`nugets.md`](./nugets/nugets.md) for the shared GitHub Actions release workflow and required secrets.

---

 
