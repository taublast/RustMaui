<div align="center">

# RustMaui

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet release master](https://github.com/taublast/RustMaui/actions/workflows/nuget-release.yml/badge.svg?branch=master)](https://github.com/taublast/RustMaui/actions/workflows/nuget-release.yml)

**Rust superpowers for .NET MAUI** 🍬🌴

<img width="800" alt="RustMaui" src="https://github.com/user-attachments/assets/6869bd41-22d9-4193-b2fa-48552593f8ed" />

</div>

---

**RustMaui** brings the **performance, safety, and joy of Rust** directly into your .NET MAUI applications on **Android, iOS, MacCatalyst and Windows**.

It's a combination of cross-platform Rust build support and bindings generators and includes everything you need:

- A friendly `dotnet tool` for instant setup
- Automatic build-time Rust bindings generation
- Optional `dotnet new` template for new projects

Read [the article behind this project](https://taublast.github.io/posts/MauiRust/)

## Quick Start

### Install the tool

```bash
dotnet tool install --global RustMaui
```

### Update Older Version

```bash
dotnet tool update --all
```

### Create a new MAUI + Rust app

```bash
rustmaui new MyAwesomeApp
```
### Or add Rust boilerplate to an existing MAUI project

```bash
rustmaui init .
```

## How this works

### Init

When you create MAUI+Rust app from template or add Rust support to an existing app, simple Rust files are created and added to your solution. Appropriate package is added to your MAUI app to handle Rust build and auto-generate Rust bindings.

```text
MyApp/
├── src/
│   └── MauiRust/
│       ├── AppShell.xaml
│       ├── MainPage.xaml
│       ├── MainPage.xaml.cs
│       ├── Rust.cs <-- override bindings here
│       ├── Rust.Generated.cs <--  auto-generated bindings
│       └── MauiRust.csproj
├── rust/
│   ├── Cargo.toml
│   └── lib.rs <--  add exported Rust code here
└── MyApp.sln
```

### Build

Your Rust library will be automatically built and packaged along with your MAUI project on Android, iOS, MacCatalyst or Windows. [Prerequisites](src/RustMaui.Templates/content/MauiRust/Prerequisites.md) apply.

On Apple targets, RustMaui uses one consistent model:

- iOS device and iOS simulator: Rust is linked as a static archive and imported from `__Internal`
- MacCatalyst: Rust is bundled as a dynamic library and imported by library name

If you write custom Rust platform-specific loading code, treat all iOS targets as the static-link path and reserve dynamic loading behavior for MacCatalyst.

### Edit

On first build the generator creates `Rust.cs` if it is missing and regenerates `Rust.Generated.cs` on every build.

If you want a different base name, set `RustBindingsName` in your app project. For example, `RustBindingsName=MyBindings` gives you `MyBindings.cs`, `MyBindings.Generated.cs`, and a `MyBindings` partial class.

Write a Rust export:

```rust
#[no_mangle]
pub extern "C" fn compute_me(value: f32) -> f32 {
    value * 2.0
}
```

The generator automatically emits a C# binding following .NET naming conventions:

```csharp
// Rust.Generated.cs — do not edit
[LibraryImport(Lib, EntryPoint = "compute_me")]
[UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
public static partial float ComputeMe(float value);
```

Call it directly from your .NET code:

```csharp
var result = Rust.ComputeMe(3.14f);
```


## Packages

### `RustMaui`

The primary entry point: a .NET tool for both greenfield and existing-app setup.

- install with `dotnet tool install --global RustMaui`
- run `rustmaui new MyApp` to create a new app from the shared scaffold
- run `rustmaui init path/to/MyApp.csproj` to add Rust boilerplate and `RustMaui.Generators` to an existing app

Package docs: [src/RustMaui.Tool/README.md](src/RustMaui.Tool/README.md).

### `RustMaui.Generators`

Build-time package that discovers Rust exports, creates the user-owned companion file when missing, generates the bindings file, and wires Rust native build targets.

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
.\dev\pack.ps1 -Project src/RustMaui.Generators/RustMaui.Generators.csproj
```

Validate the template against locally packed packages:

```powershell
.\dev\validate-template.ps1
```

That validator packs all three packages, installs the local template package, generates a temporary app outside the repo tree, reuses the local package folder as a NuGet source, and runs a Windows build against the generated app.

The template remains available, but the preferred install path for end users is `dotnet tool install --global RustMaui`.

See [`nugets.md`](./nugets/nugets.md) for the shared GitHub Actions release workflow and required secrets.

---

 
